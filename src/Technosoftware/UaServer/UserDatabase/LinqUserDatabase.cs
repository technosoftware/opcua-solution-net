#region Copyright (c) 2011-2024 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2024 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is subject to the Technosoftware GmbH Software License 
// Agreement, which can be found here:
// https://technosoftware.com/documents/Source_License_Agreement.pdf
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2024 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Newtonsoft.Json;

using Opc.Ua;
#endregion

namespace Technosoftware.UaServer.UserDatabase
{
    [Serializable]
    class User
    {
        [JsonRequired]
        public Guid ID { get; set; }
        public string UserName { get; set; }
        public string Hash { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }

    /// <summary>
    /// Implementation of a Serializable User Database using LINQ for querying users
    /// </summary>
    [Serializable]
    public class LinqUserDatabase : IUaUserDatabase
    {
        #region IUaUserDatabase
        /// <summary>
        /// initializes the collection the users database is working with
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <inheritdoc/>
        public bool CreateUser(string userName, string password, IEnumerable<Role> roles)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }
            if (//User Exists
                Users.SingleOrDefault(x => x.UserName == userName) != null)
            {
                return false;
            }

            string hash = Hash(password);

            var user = new User { UserName = userName, Hash = hash, Roles = roles };

            Users.Add(user);

            SaveChanges();

            return true;
        }
        /// <inheritdoc/>
        public bool DeleteUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }

            var user = Users.SingleOrDefault(x => x.UserName == userName);

            if (user == null)
            {
                return false;
            }
            Users.Remove(user);
            return true;
        }
        /// <inheritdoc/>
        public bool CheckCredentials(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }

            var user = Users.SingleOrDefault(x => x.UserName == userName);

            if (user == null)
            {
                return false;
            }

            return Check(user.Hash, password);
        }
        /// <inheritdoc/>
        public IEnumerable<Role> GetUserRoles(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }
            var user = Users.SingleOrDefault(x => x.UserName == userName);

            if (user == null)
            {
                throw new ArgumentException("No user found with the UserName " + userName);
            }

            return user.Roles;
        }
        /// <inheritdoc/>
        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("UserName cannot be empty.", nameof(userName));
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                throw new ArgumentException("Current Password cannot be empty.", nameof(oldPassword));
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("New Password cannot be empty.", nameof(newPassword));
            }

            var user = Users.SingleOrDefault(x => x.UserName == userName);

            if (user == null)
            {
                return false;
            }

            if (Check(user.Hash, oldPassword))
            {
                var newHash = Hash(newPassword);
                user.Hash = newHash;
                return true;
            }
            return false;
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Persists the changes to the users database
        /// </summary>
        public virtual void Save()
        {
        }
        #endregion

        #region Private Members
        private void SaveChanges()
        {
            lock (Lock)
            {
                queryCounterResetTime = DateTime.UtcNow;
                // assign IDs to new users
                var queryNewUsers = from x in Users
                                    where x.ID == Guid.Empty
                                    select x;
                if (Users.Count > 0)
                {
                    foreach (var user in queryNewUsers)
                    {
                        user.ID = Guid.NewGuid();
                    }
                }
                Save();
            }
        }
        #endregion

        #region IPasswordHasher
        private string Hash(string password)
        {
#if NETSTANDARD2_0 || NET462
#pragma warning disable CA5379 // Ensure Key Derivation Function algorithm is sufficiently strong
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                kSaltSize,
                kIterations))
            {
#pragma warning restore CA5379 // Ensure Key Derivation Function algorithm is sufficiently strong
#else
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                kSaltSize,
                kIterations,
                HashAlgorithmName.SHA512))
            {
#endif
                var key = Convert.ToBase64String(algorithm.GetBytes(kKeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{kIterations}.{salt}.{key}";
            }
        }

        private bool Check(string hash, string password)
        {
            var separator = new Char[] { '.' };
            var parts = hash.Split(separator, 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. " +
                  "Should be formatted as `{iterations}.{salt}.{hash}`");
            }

            var iterations = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture.NumberFormat);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

#if NETSTANDARD2_0 || NET462
#pragma warning disable CA5379 // Ensure Key Derivation Function algorithm is sufficiently strong
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations))
            {
#pragma warning restore CA5379 // Ensure Key Derivation Function algorithm is sufficiently strong
#else
            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512))
            {
#endif
                var keyToCheck = algorithm.GetBytes(kKeySize);

                var verified = keyToCheck.SequenceEqual(key);

                return verified;
            }
        }

        #endregion

        #region Internal Members
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Lock = new object();
            queryCounterResetTime = DateTime.UtcNow;
        }
        #endregion

        #region Internal Fields
        [NonSerialized]
        internal object Lock = new object();
        [NonSerialized]
        internal DateTime queryCounterResetTime = DateTime.UtcNow;
        [NonSerialized]
        private const int kSaltSize = 16; // 128 bit
        [NonSerialized]
        private const int kIterations = 10000; // 10k
        [NonSerialized]
        private const int kKeySize = 32; // 256 bit
        [JsonProperty]
        internal ICollection<User> Users = new HashSet<User>();
        #endregion
    }


}

