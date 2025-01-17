#region Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Opc.Ua;
#endregion

namespace Technosoftware.UaStandardServer.Tests
{
    /// <summary>
    /// Extensions for server fixture.
    /// </summary>
    public static class ServerFixtureUtils
    {
        public const double DefaultSessionTimeout = 120000;
        public const uint DefaultMaxResponseMessageSize = 128 * 1024;
        public const int MinTestPort = 50000;
        public const int MaxTestPort = 65000;

        #region Public Methods
        /// <summary>
        /// Create and Activate a session without security.
        /// </summary>
        /// <remarks>
        /// The request header is used to call services directly,
        /// without establishing a session with a client.
        /// </remarks>
        /// <param name="server">The server to connect to.</param>
        /// <param name="sessionName">A session name.</param>
        /// <returns>The request header for the session.</returns>
        public static RequestHeader CreateAndActivateSession(
            this SessionServerBase server,
            string sessionName,
            bool useSecurity = false,
            UserIdentityToken identityToken = null,
            double sessionTimeout = DefaultSessionTimeout,
            uint maxResponseMessageSize = DefaultMaxResponseMessageSize)
        {
            // Find TCP endpoint
            var endpoints = server.GetEndpoints();
            var endpoint = endpoints.FirstOrDefault(e =>
                e.TransportProfileUri.Equals(Profiles.UaTcpTransport, StringComparison.Ordinal) ||
                e.TransportProfileUri.Equals(Profiles.HttpsBinaryTransport, StringComparison.Ordinal));

            if (endpoint == null)
            {
                throw new Exception("Unsupported transport profile.");
            }

            // fake profiles
            if (useSecurity)
            {
                endpoint.SecurityMode = MessageSecurityMode.Sign;
                endpoint.SecurityPolicyUri = SecurityPolicies.Basic256Sha256;
            }
            else
            {
                endpoint.SecurityMode = MessageSecurityMode.None;
                endpoint.SecurityPolicyUri = SecurityPolicies.None;
            }

            var context = new SecureChannelContext(
                sessionName,
                endpoint,
                RequestEncoding.Binary);

            // set security context
            SecureChannelContext.Current = context;
            var requestHeader = new RequestHeader();

            // Create session
            var response = server.CreateSession(
                requestHeader,
                null, null, null,
                sessionName,
                null, null, sessionTimeout, maxResponseMessageSize,
                out var sessionId, out var authenticationToken, out sessionTimeout,
                out var serverNonce, out var serverCertificate, out var endpointDescriptions,
                out var serverSoftwareCertificates, out var signatureData, out var maxRequestMessageSize);
            ValidateResponse(response);

            // Activate session
            requestHeader.AuthenticationToken = authenticationToken;
            response = server.ActivateSession(requestHeader, signatureData,
                new SignedSoftwareCertificateCollection(), new StringCollection(),
                (identityToken != null) ? new ExtensionObject(identityToken) : null, null,
                out serverNonce, out var results, out var diagnosticInfos);
            ValidateResponse(response);

            return requestHeader;
        }

        /// <summary>
        /// Close a session.
        /// </summary>
        /// <param name="server">The server where the session is active.</param>
        /// <param name="requestHeader">The request header of the session.</param>
        public static void CloseSession(this SessionServerBase server, RequestHeader requestHeader)
        {
            // close session
            var response = server.CloseSession(requestHeader, true);
            ValidateResponse(response);
        }

        /// <summary>
        /// Validate the response of a service call.
        /// </summary>
        /// <param name="header">The response header of the service call.</param>
        public static void ValidateResponse(ResponseHeader header)
        {
            if (header == null)
            {
                throw new ServiceResultException(StatusCodes.BadUnknownResponse, "Null header in response.");
            }

            if (StatusCode.IsBad(header.ServiceResult))
            {
                throw new ServiceResultException(new ServiceResult(header.ServiceResult, header.ServiceDiagnostics, header.StringTable));
            }
        }

        /// <summary>
        /// Validate the response of a service call and validate the number of items returned.
        /// </summary>
        /// <remarks>
        /// On the client the generated code already validates the response but the
        /// check is duplicated here to catch also issues when running tests within
        /// the shared client/server context.
        /// </remarks>
        /// <param name="header">The response header of the service call.</param>
        /// <param name="response">The list of returned values by the service call.</param>
        /// <param name="request">The list of requests passed to the service call.</param>
        public static void ValidateResponse(ResponseHeader header, IList response, IList request)
        {
            ValidateResponse(header);

            if (response is DiagnosticInfoCollection)
            {
                throw new ArgumentException("Must call ValidateDiagnosticInfos() for DiagnosticInfoCollections.", nameof(response));
            }

            if (response == null || response.Count != request.Count)
            {
                throw new ServiceResultException(StatusCodes.BadUnexpectedError, "The server returned a list without the expected number of elements.");
            }
        }

        /// <summary>
        /// Validate the diagnostic response of a service call.
        /// </summary>
        /// <param name="response">The diagnostic info response.</param>
        /// <param name="request">The request items of the service call.</param>
        public static void ValidateDiagnosticInfos(DiagnosticInfoCollection response, IList request, StringCollection stringTable)
        {
            // returning an empty list for diagnostic info arrays is allowed.
            if (response != null && response.Count != 0)
            {
                if (response.Count != request.Count)
            {
                throw new ServiceResultException(StatusCodes.BadUnexpectedError,
                    "The server forgot to fill in the DiagnosticInfos array correctly when returning an operation level error.");
            }

                // now validate the string table
                if (stringTable != null)
                {
                    for (int ii = 0; ii < response.Count; ii++)
                    {
                        if (response[ii] is DiagnosticInfo diagnosticInfo && !diagnosticInfo.IsNullDiagnosticInfo)
                        {
                            if (diagnosticInfo.NamespaceUri >= stringTable.Count ||
                                diagnosticInfo.SymbolicId >= stringTable.Count ||
                                diagnosticInfo.Locale >= stringTable.Count ||
                                diagnosticInfo.LocalizedText >= stringTable.Count)
                            {
                                throw new ServiceResultException(StatusCodes.BadUnexpectedError,
                                    "The server forgot to fill in string table for the DiagnosticInfos array correctly when returning an operation level error.");
                            }
                            var serviceResult = new ServiceResult(StatusCodes.Good, ii, response, stringTable);
                            Utils.LogInfo("DiagnosticInfo: {0}", serviceResult.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a browse description from a node id collection.
        /// </summary>
        /// <param name="nodeIdCollection">The node id collection.</param>
        /// <param name="template">The template for the browse description for each node id.</param>
        public static BrowseDescriptionCollection CreateBrowseDescriptionCollectionFromNodeId(
            NodeIdCollection nodeIdCollection,
            BrowseDescription template)
        {
            var browseDescriptionCollection = new BrowseDescriptionCollection();
            foreach (var nodeId in nodeIdCollection)
            {
                BrowseDescription browseDescription = (BrowseDescription)template.MemberwiseClone();
                browseDescription.NodeId = nodeId;
                browseDescriptionCollection.Add(browseDescription);
            }
            return browseDescriptionCollection;
        }

        /// <summary>
        /// Create the continuation point collection from the browse result
        /// collection for the BrowseNext service.
        /// </summary>
        /// <param name="browseResultCollection">The browse result collection to use.</param>
        /// <returns>The collection of continuation points for the BrowseNext service.</returns>
        public static ByteStringCollection PrepareBrowseNext(BrowseResultCollection browseResultCollection)
        {
            var continuationPoints = new ByteStringCollection();
            foreach (var browseResult in browseResultCollection)
            {
                if (browseResult.ContinuationPoint != null)
                {
                    continuationPoints.Add(browseResult.ContinuationPoint);
                }
            }
            return continuationPoints;
        }

        /// <summary>
        /// A dictionary of all node attributes.
        /// </summary>
        public static readonly ReadOnlyDictionary<uint, DataValue> AttributesIds = new ReadOnlyDictionary<uint, DataValue>(
            new SortedDictionary<uint, DataValue> {
                { Attributes.NodeId, null },
                { Attributes.NodeClass, null },
                { Attributes.BrowseName, null },
                { Attributes.DisplayName, null },
                { Attributes.Description, null },
                { Attributes.WriteMask, null },
                { Attributes.UserWriteMask, null },
                { Attributes.DataType, null },
                { Attributes.ValueRank, null },
                { Attributes.ArrayDimensions, null },
                { Attributes.AccessLevel, null },
                { Attributes.UserAccessLevel, null },
                { Attributes.Historizing, null },
                { Attributes.MinimumSamplingInterval, null },
                { Attributes.EventNotifier, null },
                { Attributes.Executable, null },
                { Attributes.UserExecutable, null },
                { Attributes.IsAbstract, null },
                { Attributes.InverseName, null },
                { Attributes.Symmetric, null },
                { Attributes.ContainsNoLoops, null },
                { Attributes.DataTypeDefinition, null },
                { Attributes.RolePermissions, null },
                { Attributes.UserRolePermissions, null },
                { Attributes.AccessRestrictions, null },
                { Attributes.AccessLevelEx, null }
            });

        /// <summary>
        /// Get free IP Port.
        /// </summary>
        public static int GetNextFreeIPPort()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            using (var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endpoint);
                if (socket.LocalEndPoint is IPEndPoint ep)
                {
                    return ep.Port;
                }
            }
            return 0;
        }
        #endregion
    }
}
