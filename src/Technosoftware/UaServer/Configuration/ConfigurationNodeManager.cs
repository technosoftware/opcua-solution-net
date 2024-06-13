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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Opc.Ua;
using Opc.Ua.Security.Certificates;

using Technosoftware.UaServer.Diagnostics;
#endregion

namespace Technosoftware.UaServer.Configuration
{
    /// <summary>
    /// The Server Configuration Node Manager.
    /// </summary>
    public class ConfigurationNodeManager : DiagnosticsNodeManager
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the configuration and diagnostics manager.
        /// </summary>
        public ConfigurationNodeManager(
            IUaServerData server,
            ApplicationConfiguration configuration
            )
            :
            base(server, configuration)
        {
            rejectedStorePath_ = configuration.SecurityConfiguration.RejectedCertificateStore.StorePath;
            certificateGroups_ = new List<ServerCertificateGroup>();
            configuration_ = configuration;
            // TODO: configure cert groups in configuration
            var defaultApplicationGroup = new ServerCertificateGroup {
                NodeId = Opc.Ua.ObjectIds.ServerConfiguration_CertificateGroups_DefaultApplicationGroup,
                BrowseName = Opc.Ua.BrowseNames.DefaultApplicationGroup,
                CertificateTypes = new NodeId[] { ObjectTypeIds.RsaSha256ApplicationCertificateType },
                ApplicationCertificate = configuration.SecurityConfiguration.ApplicationCertificate,
                IssuerStorePath = configuration.SecurityConfiguration.TrustedIssuerCertificates.StorePath,
                TrustedStorePath = configuration.SecurityConfiguration.TrustedPeerCertificates.StorePath
            };
            certificateGroups_.Add(defaultApplicationGroup);
        }
        #endregion

        #region IUaNodeManager Members
        /// <summary>
        /// Replaces the generic node with a node specific to the model.
        /// </summary>
        protected override NodeState AddBehaviourToPredefinedNode(
            ISystemContext context,
            NodeState predefinedNode)
        {
            var passiveNode = predefinedNode as BaseObjectState;

            if (passiveNode != null)
            {
                NodeId typeId = passiveNode.TypeDefinitionId;
                if (IsNodeIdInNamespace(typeId) && typeId.IdType == IdType.Numeric)
                {
                    switch ((uint)typeId.Identifier)
                    {

                        case ObjectTypes.ServerConfigurationType:
                        {
                            var activeNode = new ServerConfigurationState(passiveNode.Parent);

                            activeNode.GetCertificates = new GetCertificatesMethodState(activeNode);

                            activeNode.Create(context, passiveNode);

                            serverConfigurationNode_ = activeNode;

                            // replace the node in the parent.
                            if (passiveNode.Parent != null)
                            {
                                passiveNode.Parent.ReplaceChild(context, activeNode);
                            }
                            else
                            {
                                NodeState serverNode = FindNodeInAddressSpace(ObjectIds.Server);
                                serverNode?.ReplaceChild(context, activeNode);
                            }
                            // remove the reference to server node because it is set as parent
                            activeNode.RemoveReference(ReferenceTypeIds.HasComponent, true, ObjectIds.Server);

                            return activeNode;
                        }

                        case ObjectTypes.CertificateGroupFolderType:
                        {
                            var activeNode = new CertificateGroupFolderState(passiveNode.Parent);
                            activeNode.Create(context, passiveNode);

                            // delete unsupported groups
                            if (certificateGroups_.All(group => group.BrowseName != activeNode.DefaultHttpsGroup?.BrowseName))
                            {
                                activeNode.DefaultHttpsGroup = null;
                            }
                            if (certificateGroups_.All(group => group.BrowseName != activeNode.DefaultUserTokenGroup?.BrowseName))
                            {
                                activeNode.DefaultUserTokenGroup = null;
                            }
                            if (certificateGroups_.All(group => group.BrowseName != activeNode.DefaultApplicationGroup?.BrowseName))
                            {
                                activeNode.DefaultApplicationGroup = null;
                            }

                            // replace the node in the parent.
                            if (passiveNode.Parent != null)
                            {
                                passiveNode.Parent.ReplaceChild(context, activeNode);
                            }
                            return activeNode;
                        }

                        case ObjectTypes.CertificateGroupType:
                        {
                            ServerCertificateGroup result = certificateGroups_.FirstOrDefault(group => group.NodeId == passiveNode.NodeId);
                            if (result != null)
                            {
                                var activeNode = new CertificateGroupState(passiveNode.Parent);
                                activeNode.Create(context, passiveNode);

                                result.NodeId = activeNode.NodeId;
                                result.Node = activeNode;

                                // replace the node in the parent.
                                if (passiveNode.Parent != null)
                                {
                                    passiveNode.Parent.ReplaceChild(context, activeNode);
                                }
                                return activeNode;
                            }
                        }
                        break;
                    }
                }
            }
            return base.AddBehaviourToPredefinedNode(context, predefinedNode);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates the configuration node for the server.
        /// </summary>
        public void CreateServerConfiguration(
            UaServerContext systemContext,
            ApplicationConfiguration configuration)
        {
            // setup server configuration node
            serverConfigurationNode_.ServerCapabilities.Value = configuration.ServerConfiguration.ServerCapabilities.ToArray();
            serverConfigurationNode_.ServerCapabilities.ValueRank = ValueRanks.OneDimension;
            serverConfigurationNode_.ServerCapabilities.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            serverConfigurationNode_.SupportedPrivateKeyFormats.Value = configuration.ServerConfiguration.SupportedPrivateKeyFormats.ToArray();
            serverConfigurationNode_.SupportedPrivateKeyFormats.ValueRank = ValueRanks.OneDimension;
            serverConfigurationNode_.SupportedPrivateKeyFormats.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            serverConfigurationNode_.MaxTrustListSize.Value = (uint)configuration.ServerConfiguration.MaxTrustListSize;
            serverConfigurationNode_.MulticastDnsEnabled.Value = configuration.ServerConfiguration.MultiCastDnsEnabled;

            serverConfigurationNode_.UpdateCertificate.OnCall = new UpdateCertificateMethodStateMethodCallHandler(UpdateCertificate);
            serverConfigurationNode_.CreateSigningRequest.OnCall = new CreateSigningRequestMethodStateMethodCallHandler(CreateSigningRequest);
            serverConfigurationNode_.ApplyChanges.OnCallMethod = new GenericMethodCalledEventHandler(ApplyChanges);
            serverConfigurationNode_.GetRejectedList.OnCall = new GetRejectedListMethodStateMethodCallHandler(GetRejectedList);
            serverConfigurationNode_.GetCertificates.OnCall = new GetCertificatesMethodStateMethodCallHandler(GetCertificates);

            serverConfigurationNode_.ClearChangeMasks(systemContext, true);

            // setup certificate group trust list handlers
            foreach (ServerCertificateGroup certGroup in certificateGroups_)
            {
                certGroup.Node.CertificateTypes.Value =
                    certGroup.CertificateTypes;
                certGroup.Node.TrustList.Handle = new TrustList(
                    certGroup.Node.TrustList,
                    certGroup.TrustedStorePath,
                    certGroup.IssuerStorePath,
                    new TrustList.SecureAccess(HasApplicationSecureAdminAccess),
                    new TrustList.SecureAccess(HasApplicationSecureAdminAccess));
                certGroup.Node.ClearChangeMasks(systemContext, true);
            }

            // find ServerNamespaces node and subscribe to StateChanged
            var serverNamespacesNode = FindPredefinedNode(ObjectIds.Server_Namespaces, typeof(NamespacesState)) as NamespacesState;

            if (serverNamespacesNode != null)
            {
                serverNamespacesNode.StateChanged += ServerNamespacesChanged;
            }
        }

        /// <summary>
        /// Gets and returns the <see cref="NamespaceMetadataState"/> node associated with the specified NamespaceUri
        /// </summary>
        /// <param name="namespaceUri">The Url of the namespace</param>
        /// <returns></returns>
        public NamespaceMetadataState GetNamespaceMetadataState(string namespaceUri)
        {
            if (namespaceUri == null)
            {
                return null;
            }

            if (namespaceMetadataStates_.ContainsKey(namespaceUri))
            {
                return namespaceMetadataStates_[namespaceUri];
            }

            NamespaceMetadataState namespaceMetadataState = FindNamespaceMetadataState(namespaceUri);

            lock (Lock)
            {
                // remember the result for faster access.
                namespaceMetadataStates_[namespaceUri] = namespaceMetadataState;
            }

            return namespaceMetadataState;
        }

        /// <summary>
        /// Gets or creates the <see cref="NamespaceMetadataState"/> node for the specified NamespaceUri.
        /// </summary>
        /// <param name="namespaceUri">The Url of the namespace</param>
        /// <returns></returns>
        public NamespaceMetadataState CreateNamespaceMetadataState(string namespaceUri)
        {
            NamespaceMetadataState namespaceMetadataState = FindNamespaceMetadataState(namespaceUri);

            if (namespaceMetadataState == null)
            {
                // find ServerNamespaces node
                var serverNamespacesNode = FindPredefinedNode(ObjectIds.Server_Namespaces, typeof(NamespacesState)) as NamespacesState;
                if (serverNamespacesNode == null)
                {
                    Utils.LogError("Cannot create NamespaceMetadataState for namespace '{0}'.", namespaceUri);
                    return null;
                }

                // create the NamespaceMetadata node
                namespaceMetadataState = new NamespaceMetadataState(serverNamespacesNode);
                namespaceMetadataState.BrowseName = new QualifiedName(namespaceUri, NamespaceIndex);
                namespaceMetadataState.Create(SystemContext, null, namespaceMetadataState.BrowseName, null, true);
                namespaceMetadataState.DisplayName = namespaceUri;
                namespaceMetadataState.SymbolicName = namespaceUri;
                namespaceMetadataState.NamespaceUri.Value = namespaceUri;

                // add node as child of ServerNamespaces and in predefined nodes
                serverNamespacesNode.AddChild(namespaceMetadataState);
                serverNamespacesNode.ClearChangeMasks(ServerData.DefaultSystemContext, true);
                AddPredefinedNode(SystemContext, namespaceMetadataState);
            }

            return namespaceMetadataState;
        }

        /// <summary>
        /// Determine if the impersonated user has admin access.
        /// </summary>
        /// <param name="context">An interface to an object that describes how access the system containing the data.</param>
        /// <exception cref="ServiceResultException"/>
        /// <seealso cref="StatusCodes.BadUserAccessDenied"/>
        public void HasApplicationSecureAdminAccess(ISystemContext context)
        {
            HasApplicationSecureAdminAccess(context, "");
        }

        /// <summary>
        /// Determine if the impersonated user has admin access.
        /// </summary>
        /// <param name="context">An interface to an object that describes how access the system containing the data.</param>
        /// <param name="_"></param>
        /// <exception cref="ServiceResultException"/>
        /// <seealso cref="StatusCodes.BadUserAccessDenied"/>
        public void HasApplicationSecureAdminAccess(ISystemContext context, string _)
        {
            var operationContext = (context as SystemContext)?.OperationContext as UaServerOperationContext;
            if (operationContext != null)
            {
                if (operationContext.ChannelContext?.EndpointDescription?.SecurityMode != MessageSecurityMode.SignAndEncrypt)
                {
                    throw new ServiceResultException(StatusCodes.BadUserAccessDenied, "Access to this item is only allowed with MessageSecurityMode SignAndEncrypt.");
                }
                IUserIdentity identity = operationContext.UserIdentity;
                // allow access to system configuration only with Role SecurityAdmin
                if (identity == null || identity.TokenType == UserTokenType.Anonymous ||
                    !identity.GrantedRoleIds.Contains(ObjectIds.WellKnownRole_SecurityAdmin))
                {
                    throw new ServiceResultException(StatusCodes.BadUserAccessDenied, "Security Admin Role required to access this item.");
                }

            }
        }
        #endregion

        #region Private Methods
        private ServiceResult UpdateCertificate(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId certificateGroupId,
            NodeId certificateTypeId,
            byte[] certificate,
            byte[][] issuerCertificates,
            string privateKeyFormat,
            byte[] privateKey,
            ref bool applyChangesRequired)
        {
            HasApplicationSecureAdminAccess(context);

            object[] inputArguments = new object[] { certificateGroupId, certificateTypeId, certificate, issuerCertificates, privateKeyFormat, privateKey };
            X509Certificate2 newCert = null;

            ServerData.ReportCertificateUpdateRequestedAuditEvent(context, objectId, method, inputArguments);
            try
            {
                if (certificate == null)
                {
                    throw new ArgumentNullException(nameof(certificate));
                }

                privateKeyFormat = privateKeyFormat?.ToUpper();
                if (!(String.IsNullOrEmpty(privateKeyFormat) || privateKeyFormat == "PEM" || privateKeyFormat == "PFX"))
                {
                    throw new ServiceResultException(StatusCodes.BadNotSupported, "The private key format is not supported.");
                }

                ServerCertificateGroup certificateGroup = VerifyGroupAndTypeId(certificateGroupId, certificateTypeId);
                certificateGroup.UpdateCertificate = null;

                var newIssuerCollection = new X509Certificate2Collection();

                try
                {
                    // build issuer chain
                    if (issuerCertificates != null)
                    {
                        foreach (var issuerRawCert in issuerCertificates)
                        {
                            var newIssuerCert = new X509Certificate2(issuerRawCert);
                            newIssuerCollection.Add(newIssuerCert);
                        }
                    }

                    newCert = new X509Certificate2(certificate);
                }
                catch
                {
                    throw new ServiceResultException(StatusCodes.BadCertificateInvalid, "Certificate data is invalid.");
                }

                // validate new subject matches the previous subject,
                // otherwise application may not be able to find it after restart
                // TODO: An issuer may modify the subject of an issued certificate,
                // but then the configuration must be updated too!
                // NOTE: not a strict requirement here for ASN.1 byte compare 
                if (!X509Utils.CompareDistinguishedName(certificateGroup.ApplicationCertificate.Certificate.Subject, newCert.Subject))
                {
                    throw new ServiceResultException(StatusCodes.BadSecurityChecksFailed, "Subject Name of new certificate doesn't match the application.");
                }

                // self signed
                bool selfSigned = X509Utils.IsSelfSigned(newCert);
                if (selfSigned && newIssuerCollection.Count != 0)
                {
                    throw new ServiceResultException(StatusCodes.BadCertificateInvalid, "Issuer list not empty for self signed certificate.");
                }

                if (!selfSigned)
                {
                    try
                    {
                        // verify cert with issuer chain
                        var certValidator = new CertificateValidator();
                        var issuerStore = new CertificateTrustList();
                        var issuerCollection = new CertificateIdentifierCollection();
                        foreach (X509Certificate2 issuerCert in newIssuerCollection)
                        {
                            issuerCollection.Add(new CertificateIdentifier(issuerCert));
                        }
                        issuerStore.TrustedCertificates = issuerCollection;
                        certValidator.Update(issuerStore, issuerStore, null);
                        certValidator.Validate(newCert);
                    }
                    catch (Exception ex)
                    {
                        throw new ServiceResultException(StatusCodes.BadSecurityChecksFailed, "Failed to verify integrity of the new certificate and the issuer list.", ex);
                    }
                }

                var updateCertificate = new UpdateCertificateData();
                try
                {
                    ICertificatePasswordProvider passwordProvider = configuration_.SecurityConfiguration.CertificatePasswordProvider;
                    switch (privateKeyFormat)
                    {
                        case null:
                        case "":
                        {
                            X509Certificate2 certWithPrivateKey = certificateGroup.ApplicationCertificate.LoadPrivateKeyEx(passwordProvider).Result;
                            var exportableKey = X509Utils.CreateCopyWithPrivateKey(certWithPrivateKey, false);
                            updateCertificate.CertificateWithPrivateKey = CertificateFactory.CreateCertificateWithPrivateKey(newCert, exportableKey);
                            break;
                        }
                        case "PFX":
                        {
                            X509Certificate2 certWithPrivateKey = X509Utils.CreateCertificateFromPKCS12(privateKey, passwordProvider?.GetPassword(certificateGroup.ApplicationCertificate), true);
                            updateCertificate.CertificateWithPrivateKey = CertificateFactory.CreateCertificateWithPrivateKey(newCert, certWithPrivateKey);
                            break;
                        }
                        case "PEM":
                        {
                            updateCertificate.CertificateWithPrivateKey = CertificateFactory.CreateCertificateWithPEMPrivateKey(newCert, privateKey, passwordProvider?.GetPassword(certificateGroup.ApplicationCertificate));
                            break;
                        }
                    }
                    updateCertificate.IssuerCollection = newIssuerCollection;
                    updateCertificate.SessionId = context.SessionId;
                }
                catch
                {
                    throw new ServiceResultException(StatusCodes.BadSecurityChecksFailed, "Failed to verify integrity of the new certificate and the private key.");
                }

                certificateGroup.UpdateCertificate = updateCertificate;
                applyChangesRequired = true;

                if (updateCertificate != null)
                {
                    try
                    {
                        using (ICertificateStore appStore = certificateGroup.ApplicationCertificate.OpenStore())
                        {
                            Utils.LogCertificate(Utils.TraceMasks.Security, "Delete application certificate: ", certificateGroup.ApplicationCertificate.Certificate);
                            appStore.Delete(certificateGroup.ApplicationCertificate.Thumbprint).Wait();
                            Utils.LogCertificate(Utils.TraceMasks.Security, "Add new application certificate: ", updateCertificate.CertificateWithPrivateKey);
                            ICertificatePasswordProvider passwordProvider = configuration_.SecurityConfiguration.CertificatePasswordProvider;
                            appStore.Add(updateCertificate.CertificateWithPrivateKey, passwordProvider?.GetPassword(certificateGroup.ApplicationCertificate)).Wait();
                            // keep only track of cert without private key
                            var certOnly = new X509Certificate2(updateCertificate.CertificateWithPrivateKey.RawData);
                            updateCertificate.CertificateWithPrivateKey.Dispose();
                            updateCertificate.CertificateWithPrivateKey = certOnly;
                        }
                        using (ICertificateStore issuerStore = CertificateStoreIdentifier.OpenStore(certificateGroup.IssuerStorePath))
                        {
                            foreach (X509Certificate2 issuer in updateCertificate.IssuerCollection)
                            {
                                try
                                {
                                    Utils.LogCertificate(Utils.TraceMasks.Security, "Add new issuer certificate: ", issuer);
                                    issuerStore.Add(issuer).Wait();
                                }
                                catch (ArgumentException)
                                {
                                    // ignore error if issuer cert already exists
                                }
                            }
                        }

                        ServerData.ReportCertificateUpdatedAuditEvent(context, objectId, method, inputArguments, certificateGroupId, certificateTypeId);
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(Utils.TraceMasks.Security, ServiceResult.BuildExceptionTrace(ex));
                        throw new ServiceResultException(StatusCodes.BadSecurityChecksFailed, "Failed to update certificate.", ex);
                    }
                }
            }
            catch (Exception e)
            {
                // report the failure of UpdateCertificate via an audit event
                ServerData.ReportCertificateUpdatedAuditEvent(context, objectId, method, inputArguments, certificateGroupId, certificateTypeId, e);
                // Raise audit certificate event 
                ServerData.ReportAuditCertificateEvent(newCert, e);
                throw;
            }

            return ServiceResult.Good;
        }

        private ServiceResult CreateSigningRequest(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId certificateGroupId,
            NodeId certificateTypeId,
            string subjectName,
            bool regeneratePrivateKey,
            byte[] nonce,
            ref byte[] certificateRequest)
        {
            HasApplicationSecureAdminAccess(context);

            ServerCertificateGroup certificateGroup = VerifyGroupAndTypeId(certificateGroupId, certificateTypeId);

            if (!String.IsNullOrEmpty(subjectName))
            {
                throw new ArgumentNullException(nameof(subjectName));
            }

            // TODO: implement regeneratePrivateKey
            // TODO: use nonce for generating the private key

            ICertificatePasswordProvider passwordProvider = configuration_.SecurityConfiguration.CertificatePasswordProvider;
            X509Certificate2 certWithPrivateKey = certificateGroup.ApplicationCertificate.LoadPrivateKeyEx(passwordProvider).Result;
            Utils.LogCertificate(Utils.TraceMasks.Security, "Create signing request: ", certWithPrivateKey);
            certificateRequest = CertificateFactory.CreateSigningRequest(certWithPrivateKey, X509Utils.GetDomainsFromCertificate(certWithPrivateKey));
            return ServiceResult.Good;
        }

        private ServiceResult ApplyChanges(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {
            HasApplicationSecureAdminAccess(context);

            var disconnectSessions = false;

            foreach (ServerCertificateGroup certificateGroup in certificateGroups_)
            {
                try
                {
                    UpdateCertificateData updateCertificate = certificateGroup.UpdateCertificate;
                    if (updateCertificate != null)
                    {
                        disconnectSessions = true;
                        Utils.LogCertificate((int)Utils.TraceMasks.Security, "Apply Changes for certificate: ",
                            updateCertificate.CertificateWithPrivateKey);
                    }
                }
                finally
                {
                    certificateGroup.UpdateCertificate = null;
                }
            }

            if (disconnectSessions)
            {
                Task.Run(async () => {
                    Utils.LogInfo((int)Utils.TraceMasks.Security, "Apply Changes for application certificate update.");
                    // give the client some time to receive the response
                    // before the certificate update may disconnect all sessions
                    await Task.Delay(1000).ConfigureAwait(false);
                    await configuration_.CertificateValidator.UpdateCertificate(configuration_.SecurityConfiguration).ConfigureAwait(false);
                }
                );
            }

            return StatusCodes.Good;
        }

        private ServiceResult GetRejectedList(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            ref byte[][] certificates)
        {
            HasApplicationSecureAdminAccess(context);

            using (ICertificateStore store = CertificateStoreIdentifier.OpenStore(rejectedStorePath_))
            {
                X509Certificate2Collection collection = store.Enumerate().Result;
                var rawList = new List<byte[]>();
                foreach (X509Certificate2 cert in collection)
                {
                    rawList.Add(cert.RawData);
                }
                certificates = rawList.ToArray();
            }

            return StatusCodes.Good;
        }

        private ServiceResult GetCertificates(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            NodeId certificateGroupId,
            ref NodeId[] certificateTypeIds,
            ref byte[][] certificates)
        {
            HasApplicationSecureAdminAccess(context);

            ServerCertificateGroup certificateGroup = certificateGroups_.FirstOrDefault(group => Utils.IsEqual(group.NodeId, certificateGroupId));
            if (certificateGroup == null)
            {
                throw new ServiceResultException(StatusCodes.BadInvalidArgument, "Certificate group invalid.");
            }

            NodeId certificateTypeId = certificateGroup.CertificateTypes.FirstOrDefault();

            //TODO support multiple Application Instance Certificates
            if (certificateTypeId != null)
            {
                certificateTypeIds = new NodeId[1] {certificateTypeId };
                certificates = new byte[1][];
                certificates[0] = certificateGroup.ApplicationCertificate.Certificate.GetRawCertData();
            }
            else
            {
                certificateTypeIds = new NodeId[0];
                certificates = new byte[0][];
            }

            return ServiceResult.Good;
        }


        private ServerCertificateGroup VerifyGroupAndTypeId(
            NodeId certificateGroupId,
            NodeId certificateTypeId
            )
        {
            // verify typeid must be set
            if (NodeId.IsNull(certificateTypeId))
            {
                throw new ServiceResultException(StatusCodes.BadInvalidArgument, "Certificate type not specified.");
            }

            // verify requested certificate group
            if (NodeId.IsNull(certificateGroupId))
            {
                certificateGroupId = ObjectIds.ServerConfiguration_CertificateGroups_DefaultApplicationGroup;
            }

            ServerCertificateGroup certificateGroup = certificateGroups_.FirstOrDefault(group => Utils.IsEqual(group.NodeId, certificateGroupId));
            if (certificateGroup == null)
            {
                throw new ServiceResultException(StatusCodes.BadInvalidArgument, "Certificate group invalid.");
            }

            // verify certificate type
            var foundCertType = certificateGroup.CertificateTypes.Any(t => Utils.IsEqual(t, certificateTypeId));
            if (!foundCertType)
            {
                throw new ServiceResultException(StatusCodes.BadInvalidArgument, "Certificate type not valid for certificate group.");
            }

            return certificateGroup;
        }

        /// <summary>
        /// Finds the <see cref="NamespaceMetadataState"/> node for the specified NamespaceUri.
        /// </summary>
        /// <param name="namespaceUri"></param>
        /// <returns></returns>
        private NamespaceMetadataState FindNamespaceMetadataState(string namespaceUri)
        {
            try
            {
                // find ServerNamespaces node
                var serverNamespacesNode = FindPredefinedNode(ObjectIds.Server_Namespaces, typeof(NamespacesState)) as NamespacesState;
                if (serverNamespacesNode == null)
                {
                    Utils.LogError("Cannot find ObjectIds.Server_Namespaces node.");
                    return null;
                }

                IList<BaseInstanceState> serverNamespacesChildren = new List<BaseInstanceState>();
                serverNamespacesNode.GetChildren(SystemContext, serverNamespacesChildren);

                foreach (BaseInstanceState namespacesReference in serverNamespacesChildren)
                {
                    // Find NamespaceMetadata node of NamespaceUri in Namespaces children
                    var namespaceMetadata = namespacesReference as NamespaceMetadataState;

                    if (namespaceMetadata == null)
                    {
                        continue;
                    }

                    if (namespaceMetadata.NamespaceUri.Value == namespaceUri)
                    {
                        return namespaceMetadata;
                    }
                    else
                    {
                        continue;
                    }
                }

                IList<IReference> serverNamespacesReferencs = new List<IReference>();
                serverNamespacesNode.GetReferences(SystemContext, serverNamespacesReferencs);

                foreach (IReference serverNamespacesReference in serverNamespacesReferencs)
                {
                    if (serverNamespacesReference.IsInverse == false)
                    {
                        // Find NamespaceMetadata node of NamespaceUri in Namespaces references
                        var nameSpaceNodeId = ExpandedNodeId.ToNodeId(serverNamespacesReference.TargetId, ServerData.NamespaceUris);
                        var namespaceMetadata = FindNodeInAddressSpace(nameSpaceNodeId) as NamespaceMetadataState;

                        if (namespaceMetadata == null)
                        {
                            continue;
                        }

                        if (namespaceMetadata.NamespaceUri.Value == namespaceUri)
                        {
                            return namespaceMetadata;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex, "Error searching NamespaceMetadata for namespaceUri {0}.", namespaceUri);
                return null;
            }
        }

        /// <summary>
        /// Clear NamespaceMetadata nodes cache in case nodes are added or deleted
        /// </summary>
        private void ServerNamespacesChanged(ISystemContext context, NodeState node, NodeStateChangeMasks changes)
        {
            if ((changes & NodeStateChangeMasks.Children) != 0 ||
                (changes & NodeStateChangeMasks.References) != 0)
            {
                try
                {
                    lock (Lock)
                    {
                        namespaceMetadataStates_.Clear();
                    }
                }
                catch
                {
                    // ignore errors
                }
            }
        }
        #endregion

        #region Private Fields
        private class UpdateCertificateData
        {
            public NodeId SessionId;
            public X509Certificate2 CertificateWithPrivateKey;
            public X509Certificate2Collection IssuerCollection;
        }

        private class ServerCertificateGroup
        {
            public string BrowseName;
            public NodeId NodeId;
            public CertificateGroupState Node;
            public NodeId[] CertificateTypes;
            public CertificateIdentifier ApplicationCertificate;
            public string IssuerStorePath;
            public string TrustedStorePath;
            public UpdateCertificateData UpdateCertificate;
        }

        private ServerConfigurationState serverConfigurationNode_;
        private readonly ApplicationConfiguration configuration_;
        private readonly IList<ServerCertificateGroup> certificateGroups_;
        private readonly string rejectedStorePath_;
        private readonly Dictionary<string, NamespaceMetadataState> namespaceMetadataStates_ = new Dictionary<string, NamespaceMetadataState>();
        #endregion
    }
}
