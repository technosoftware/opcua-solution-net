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
using System.IO;
using Newtonsoft.Json;
#endregion

namespace Technosoftware.UaServer.UserDatabase
{
    /// <summary>
    /// A user database with JSON storage.
    /// </summary>
    /// <remarks>
    /// This db is good for testing but not for production use.
    /// </remarks>
    public class JsonUserDatabase : LinqUserDatabase
    {
        #region Constructors
        /// <summary>
        /// Create a JSON database.
        /// </summary>
        public JsonUserDatabase(string fileName)
        {
            m_fileName = fileName;
        }

        /// <summary>
        /// Load the JSON application database.
        /// </summary>
        static public JsonUserDatabase Load(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            try
            {
                if (File.Exists(fileName))
                {
                    string json = File.ReadAllText(fileName);
                    JsonUserDatabase db = JsonConvert.DeserializeObject<JsonUserDatabase>(json);
                    db.FileName = fileName;
                    return db;
                }
            }
            catch
            {

            }
            return new JsonUserDatabase(fileName);
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Save the complete database.
        /// </summary>
        public override void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(m_fileName, json);
        }

        /// <summary>
        /// Get or set the filename.
        /// </summary>
        [JsonIgnore]
        public string FileName { get { return m_fileName; } private set { m_fileName = value; } }
        #endregion

        #region Private Fields
        [JsonIgnore]
        string m_fileName;
        #endregion
    }
}
