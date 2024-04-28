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

using Opc.Ua;
#endregion

namespace Technosoftware.UaServer.UserDatabase
{
    /// <summary>
    /// An abstract interface to the user database which stores logins with associated Roles
    /// </summary>
    public interface IUaUserDatabase
    {
        /// <summary>
        /// Initialize User Database
        /// </summary>
        void Initialize();
        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="userName">the username</param>
        /// <param name="password">the password</param>
        /// <param name="roles">the role of the new user</param>
        /// <returns>true if registered successfull</returns>
        bool CreateUser(string userName, string password, IEnumerable<Role> roles);
        /// <summary>
        /// Delete existing user
        /// </summary>
        /// <param name="userName">the user to delete</param>
        /// <returns>true if deleted successfully</returns>
        bool DeleteUser(string userName);
        /// <summary>
        /// checks if the provided credentials fit a user
        /// </summary>
        /// <param name="userName">the username</param>
        /// <param name="password">the password</param>
        /// <returns>true if userName + PW combination is correct</returns>
        bool CheckCredentials(string userName, string password);
        /// <summary>
        /// returns the Role of the provided user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>the Role of the provided users</returns>
        /// <exception cref="ArgumentException">When the user is not found</exception>
        IEnumerable<Role> GetUserRoles(string userName);
        /// <summary>
        /// changes the password of an existing users
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>true if change was successfull</returns>
        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }
}
