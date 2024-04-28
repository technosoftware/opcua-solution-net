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

using System.Collections.Generic;
using System.Xml;

using Opc.Ua;

#endregion

namespace Technosoftware.UaServer.Configuration
{
    /// <summary>
    /// Privileged identity which can access the system configuration.
    /// </summary>
    public class SystemConfigurationIdentity : RoleBasedIdentity
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Create a user identity with the privilege
        /// to modify the system configuration.
        /// </summary>
        /// <param name="identity">The user identity.</param>
        public SystemConfigurationIdentity(IUserIdentity identity)
        :base(identity, new List<Role> {Role.SecurityAdmin, Role.ConfigureAdmin }){
        }
        #endregion
    }
}
