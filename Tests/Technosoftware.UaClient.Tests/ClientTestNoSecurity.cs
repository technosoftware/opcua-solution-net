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
using System.Threading.Tasks;

using NUnit.Framework;

using Opc.Ua;
#endregion

namespace Technosoftware.UaClient.Tests
{
    /// <summary>
    /// Client tests which require security None and are otherwise skipped,
    /// starts the server with additional security None profile.
    /// </summary>
    [TestFixture, Category("Client")]
    [SetCulture("en-us"), SetUICulture("en-us")]
    [TestFixtureSource(nameof(FixtureArgs))]

    public class ClientTestNoSecurity
    {
        private ClientTest _clientTest { get; set; }

        public static readonly object[] FixtureArgs = {
            new object [] { Utils.UriSchemeOpcTcp},
            // https protocol security None is not supported
            // new object [] { Utils.UriSchemeHttps},
            // new object [] { Utils.UriSchemeOpcHttps},
        };

        public ClientTestNoSecurity()
        {
            _clientTest = new ClientTest(Utils.UriSchemeOpcTcp);
        }

        public ClientTestNoSecurity(string uriScheme)
        {
            _clientTest = new ClientTest(uriScheme);
        }

        #region Test Setup
        /// <summary>
        /// Set up a Server and a Client instance.
        /// </summary>
        [OneTimeSetUp]
        public Task OneTimeSetUp()
        {
            _clientTest.SupportsExternalServerUrl = true;
            return _clientTest.OneTimeSetUpAsync(null, true);
        }

        /// <summary>
        /// Tear down the Server and the Client.
        /// </summary>
        [OneTimeTearDown]
        public Task OneTimeTearDownAsync()
        {
            return _clientTest.OneTimeTearDownAsync();
        }

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public Task SetUp()
        {
            return _clientTest.SetUp();
        }

        /// <summary>
        /// Test teardown.
        /// </summary>
        [TearDown]
        public Task TearDown()
        {
            return _clientTest.TearDown();
        }
        #endregion

        [Test, Order(230)]
        public Task ReconnectJWTSecurityNone()
        {
            return _clientTest.ReconnectJWT(SecurityPolicies.None);
        }

        [Test, Order(220)]
        public Task ConnectJWT()
        {
            return _clientTest.ConnectJWT(SecurityPolicies.None);
        }

        /// <summary>
        /// Open a session on a channel, then reconnect (activate)
        /// the same session on a new channel with saved session secrets
        /// </summary>
        [Test, Order(260)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public Task ReconnectSessionOnAlternateChannelWithSavedSessionSecretsSecurityNone(bool anonymous, bool asyncReconnect)
        {
            return _clientTest.ReconnectSessionOnAlternateChannelWithSavedSessionSecrets(SecurityPolicies.None, anonymous, asyncReconnect);
        }

        [Theory, Order(400)]
        public Task BrowseFullAddressSpaceSecurityNone(bool operationLimits)
        {
            return _clientTest.BrowseFullAddressSpace(SecurityPolicies.None, operationLimits);
        }

        [Test, Order(201)]
        public Task ConnectAndCloseAsyncNoSecurity()
        {
            return _clientTest.ConnectAndCloseAsync(SecurityPolicies.None);
        }
    }
}
