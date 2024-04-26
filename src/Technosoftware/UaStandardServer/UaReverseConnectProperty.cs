#region Copyright (c) 2011-2024 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2024 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2024 Technosoftware GmbH. All rights reserved

#region Using Directives
using Opc.Ua;
using System;
#endregion

namespace Technosoftware.UaStandardServer
{
    /// <summary>
    /// Describes the properties of a server reverse connection.
    /// </summary>
    public class UaReverseConnectProperty
    {
        /// <summary>
        /// Initialize a reverse connect server property.
        /// </summary>
        /// <param name="clientUrl">The Url of the reverse connect client.</param>
        /// <param name="timeout">The timeout to use for a reverse connect attempt.</param>
        /// <param name="maxSessionCount">The maximum number of sessions allowed to the client.</param>
        /// <param name="configEntry">If this is an application configuration entry.</param>
        /// <param name="enabled">If the connection is enabled.</param>
        public UaReverseConnectProperty(
            Uri clientUrl,
            int timeout,
            int maxSessionCount,
            bool configEntry,
            bool enabled = true)
        {
            ClientUrl = clientUrl;
            Timeout = timeout > 0 ? timeout : UaStandardServer.DefaultReverseConnectTimeout;
            MaxSessionCount = maxSessionCount;
            ConfigEntry = configEntry;
            Enabled = enabled;
        }

        /// <summary>
        /// The Url of the reverse connect client.
        /// </summary>
        public Uri ClientUrl { get; private set; }

        /// <summary>
        /// The timeout to use for a reverse connect attempt.
        /// </summary>
        public int Timeout { get; private set; }

        /// <summary>
        /// If this is an application configuration entry.
        /// </summary>
        public bool ConfigEntry { get; private set; }

        /// <summary>
        /// The service result of the last connection attempt.
        /// </summary>
        public ServiceResult ServiceResult { get; set; }

        /// <summary>
        /// The maximum number of sessions allowed to the client.
        /// </summary>
        public int MaxSessionCount { get; set; }

        /// <summary>
        /// If the connection is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The time when the connection was rejected.
        /// </summary>
        public DateTime RejectTime { get; set; }

        /// <summary>
        /// The last state of the reverse connection.
        /// </summary>
        public UaReverseConnectState LastState { get; set; } = UaReverseConnectState.Closed;
    }
}
