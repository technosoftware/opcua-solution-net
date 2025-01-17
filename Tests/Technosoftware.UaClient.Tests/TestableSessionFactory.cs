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
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Opc.Ua;

using Technosoftware.UaStandardServer.Tests;
#endregion

namespace Technosoftware.UaClient.Tests
{
    /// <summary>
    /// Object that creates instances of an Opc.Ua.Client.Session object.
    /// </summary>
    public class TestableSessionFactory : DefaultSessionFactory
    {
        /// <summary>
        /// The default instance of the factory.
        /// </summary>
        public new static readonly TestableSessionFactory Instance = new TestableSessionFactory();

        /// <summary>
        /// Force use of the default instance.
        /// </summary>
        protected TestableSessionFactory()
        {
        }

        #region ISessionFactory Members
        /// <inheritdoc/>
        public override Task<IUaSession> CreateAsync(
            ApplicationConfiguration configuration,
            ConfiguredEndpoint endpoint,
            bool updateBeforeConnect,
            string sessionName,
            uint sessionTimeout,
            IUserIdentity identity,
            IList<string> preferredLocales,
            CancellationToken ct = default)
        {
            return CreateAsync(configuration, endpoint, updateBeforeConnect, false, sessionName, sessionTimeout, identity, preferredLocales, ct);
        }

        /// <inheritdoc/>
        public async override Task<IUaSession> CreateAsync(
            ApplicationConfiguration configuration,
            ConfiguredEndpoint endpoint,
            bool updateBeforeConnect,
            bool checkDomain,
            string sessionName,
            uint sessionTimeout,
            IUserIdentity identity,
            IList<string> preferredLocales,
            CancellationToken ct = default)
        {
            return await Session.CreateAsync(this, configuration, (ITransportWaitingConnection)null, endpoint,
                updateBeforeConnect, checkDomain, sessionName, sessionTimeout,
                identity, preferredLocales, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async override Task<IUaSession> CreateAsync(
            ApplicationConfiguration configuration,
            ITransportWaitingConnection connection,
            ConfiguredEndpoint endpoint,
            bool updateBeforeConnect,
            bool checkDomain,
            string sessionName,
            uint sessionTimeout,
            IUserIdentity identity,
            IList<string> preferredLocales,
            CancellationToken ct = default)
        {
            return await Session.CreateAsync(this, configuration, connection, endpoint,
                updateBeforeConnect, checkDomain, sessionName, sessionTimeout,
                identity, preferredLocales, ct
                ).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async override Task<IUaSession> CreateAsync(
            ApplicationConfiguration configuration,
            ReverseConnectManager reverseConnectManager,
            ConfiguredEndpoint endpoint,
            bool updateBeforeConnect,
            bool checkDomain,
            string sessionName,
            uint sessionTimeout,
            IUserIdentity userIdentity,
            IList<string> preferredLocales,
            CancellationToken ct = default
            )
        {
            if (reverseConnectManager == null)
            {
                return await this.CreateAsync(configuration, endpoint, updateBeforeConnect,
                    checkDomain, sessionName, sessionTimeout, userIdentity, preferredLocales, ct).ConfigureAwait(false);
            }

            ITransportWaitingConnection connection;
            do
            {
                connection = await reverseConnectManager.WaitForConnectionAsync(
                    endpoint.EndpointUrl,
                    endpoint.ReverseConnect?.ServerUri,
                    ct).ConfigureAwait(false);

                if (updateBeforeConnect)
                {
                    await endpoint.UpdateFromServerAsync(
                        endpoint.EndpointUrl, connection,
                        endpoint.Description.SecurityMode,
                        endpoint.Description.SecurityPolicyUri,
                        ct).ConfigureAwait(false);
                    updateBeforeConnect = false;
                    connection = null;
                }
            } while (connection == null);

            return await CreateAsync(
                configuration,
                connection,
                endpoint,
                false,
                checkDomain,
                sessionName,
                sessionTimeout,
                userIdentity,
                preferredLocales,
                ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override IUaSession Create(
           ApplicationConfiguration configuration,
           ITransportChannel channel,
           ConfiguredEndpoint endpoint,
           X509Certificate2 clientCertificate,
           EndpointDescriptionCollection availableEndpoints = null,
           StringCollection discoveryProfileUris = null)
        {
            return Session.Create(this, configuration, channel, endpoint, clientCertificate, availableEndpoints, discoveryProfileUris);
        }
        #endregion

        #region ISessionInstantiator Members
        /// <inheritdoc/>
        public override Session Create(
            ISessionChannel channel,
            ApplicationConfiguration configuration,
            ConfiguredEndpoint endpoint)
        {
            return new TestableSession(channel, configuration, endpoint);
        }

        /// <inheritdoc/>
        public override Session Create(
            ITransportChannel channel,
            ApplicationConfiguration configuration,
            ConfiguredEndpoint endpoint,
            X509Certificate2 clientCertificate,
            EndpointDescriptionCollection availableEndpoints = null,
            StringCollection discoveryProfileUris = null)
        {
            return new TestableSession(channel, configuration, endpoint, clientCertificate, availableEndpoints, discoveryProfileUris);
        }
        #endregion
    }
}
