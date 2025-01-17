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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

using Opc.Ua;

using Technosoftware.UaPubSub;
using Technosoftware.UaPubSub.Configuration;
using Technosoftware.UaPubSub.Transport;
#endregion

namespace Technosoftware.UaPubSub.Tests.Transport
{

    [TestFixture(Description = "Tests for UdpPubSubConnection class - Publisher ")]
    public partial class UdpPubSubConnectionTests
    {
        [Test(Description = "Validate unicast PublishNetworkMessage"), Order(1)]
#if !CUSTOM_TESTS
        [Ignore("A network interface controller is necessary in order to run correctly.")]
#endif
        public void ValidateUdpPubSubConnectionNetworkMessagePublishUnicast()
        {
           //Arrange 
            var localhost = GetFirstNic();
            Assert.IsNotNull(localhost, "localhost is null");
            Assert.IsNotNull(localhost.Address, "localhost.Address is null");

            //create publisher configuration object with modified port
            string configurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            PubSubConfigurationDataType publisherConfiguration = UaPubSubConfigurationHelper.LoadConfiguration(configurationFile);
            Assert.IsNotNull(publisherConfiguration, "publisherConfiguration is null");
            Assert.Greater(publisherConfiguration.Connections.Count, 1, "publisherConfiguration.Connection should be > 0");

            IPAddress unicastIPAddress = localhost.Address;
            Assert.IsNotNull(unicastIPAddress, "unicastIPAddress is null");

            NetworkAddressUrlDataType publisherAddress = new NetworkAddressUrlDataType();
            publisherAddress.Url = Utils.Format(UdpUrlFormat, Utils.UriSchemeOpcUdp, unicastIPAddress.ToString());
            publisherConfiguration.Connections.First().Address = new ExtensionObject(publisherAddress);

            //create publisher UaPubSubApplication with changed configuration settings
            UaPubSubApplication publisherApplication = UaPubSubApplication.Create(publisherConfiguration);
            Assert.IsNotNull(publisherApplication, "publisherApplication is null");

            UdpPubSubConnection publisherConnection = publisherApplication.PubSubConnections.First() as UdpPubSubConnection;
            Assert.IsNotNull(publisherConnection, "publisherConnection is null");

            // will signal that the uadp message was received from local ip
            shutdownEvent_ = new ManualResetEvent(false);

            //setup uadp client for receiving from multicast (simulate a subscriber unicast)
            UdpClient udpUnicastClient = new UdpClientUnicast(localhost.Address, DiscoveryPortNo);
            Assert.IsNotNull(udpUnicastClient, "udpUnicastClient is null");
            udpUnicastClient.BeginReceive(new AsyncCallback(OnReceive), udpUnicastClient);

            // prepare a network message
            WriterGroupDataType writerGroup0 = publisherConnection.PubSubConnectionConfiguration.WriterGroups.First();
            Assert.IsNotNull(writerGroup0, "writerGroup0 is null");

            System.Collections.Generic.IList<UaNetworkMessage> networkMessages = publisherConnection.CreateNetworkMessages(writerGroup0, new WriterGroupPublishState());
            Assert.IsNotNull(networkMessages, "connection.CreateNetworkMessages shall not return null");

            //Act
           publisherConnection.Start();

            if (networkMessages != null)
            {
                foreach (UaNetworkMessage uaNetworkMessage in networkMessages)
                {
                    if (uaNetworkMessage != null)
                    {
                        publisherConnection.PublishNetworkMessage(uaNetworkMessage);
                    }
                }
            }

            //Assert
            bool noMessageReceived = false;
            if (!shutdownEvent_.WaitOne(EstimatedPublishingTime))
            {
                noMessageReceived = true;
            }

            publisherConnection.Stop();
            udpUnicastClient.Close();
            udpUnicastClient.Dispose();

            if (noMessageReceived == true)
            {
                Assert.Fail("The UDP message was not received");
            }
        }

        [Test(Description = "Validate broadcast PublishNetworkMessage"), Order(2)]
#if !CUSTOM_TESTS
        [Ignore("A network interface controller is necessary in order to run correctly.")]
#endif
        public void ValidateUdpPubSubConnectionNetworkMessagePublishBroadcast()
        {
            //Arrange 
            var localhost = GetFirstNic();
            Assert.IsNotNull(localhost, "localhost is null");
            Assert.IsNotNull(localhost.Address, "localhost.Address is null");

            //create publisher configuration object with modified port
            string configurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            PubSubConfigurationDataType publisherConfiguration = UaPubSubConfigurationHelper.LoadConfiguration(configurationFile);
            Assert.IsNotNull(publisherConfiguration, "publisherConfiguration is null");
            Assert.Greater(publisherConfiguration.Connections.Count, 1, "publisherConfiguration.Connection should be > 0");

            IPAddress broadcastIPAddress = GetFirstNicLastIPByteChanged(255);
            Assert.IsNotNull(broadcastIPAddress, "broadcastIPAddress is null");

            NetworkAddressUrlDataType publisherAddress = new NetworkAddressUrlDataType();
            publisherAddress.Url = Utils.Format(UdpUrlFormat, Utils.UriSchemeOpcUdp, broadcastIPAddress.ToString());
            publisherConfiguration.Connections.First().Address = new ExtensionObject(publisherAddress);

            //create publisher UaPubSubApplication with changed configuration settings
            UaPubSubApplication publisherApplication = UaPubSubApplication.Create(publisherConfiguration);
            Assert.IsNotNull(publisherApplication, "publisherApplication is null");

            UdpPubSubConnection publisherConnection = publisherApplication.PubSubConnections.First() as UdpPubSubConnection;
            Assert.IsNotNull(publisherConnection, "publisherConnection is null");

            // will signal that the uadp message was received from local ip
            shutdownEvent_ = new ManualResetEvent(false);

            //setup uadp client for receiving from broadcast (simulate a subscriber broadcast)
            UdpClient udpBroadcastClient = new UdpClientBroadcast(localhost.Address, DiscoveryPortNo, UsedInContext.Subscriber);
            udpBroadcastClient.BeginReceive(new AsyncCallback(OnReceive), udpBroadcastClient);

            // prepare a network message
            WriterGroupDataType writerGroup0 = publisherConnection.PubSubConnectionConfiguration.WriterGroups.First();
            IList<UaNetworkMessage> networkMessages = publisherConnection.CreateNetworkMessages(writerGroup0, new WriterGroupPublishState());
            Assert.IsNotNull(networkMessages, "connection.CreateNetworkMessages shall not return null");

            //Act  
            publisherConnection.Start();

            if (networkMessages != null)
            {
                foreach (UaNetworkMessage uaNetworkMessage in networkMessages)
                {
                    if (uaNetworkMessage != null)
                    {
                        publisherConnection.PublishNetworkMessage(uaNetworkMessage);
                    }
                }
            }

            //Assert
            bool noMessageReceived = false;
            if (!shutdownEvent_.WaitOne(EstimatedPublishingTime))
            {
                noMessageReceived = true;
            }

            publisherConnection.Stop();
            udpBroadcastClient.Close();
            udpBroadcastClient.Dispose();

            if (noMessageReceived)
            {
                Assert.Fail("The UDP message was not received");
            }
        }

        [Test(Description = "Validate multicast PublishNetworkMessage"), Order(3)]
#if !CUSTOM_TESTS
        [Ignore("A network interface controller is necessary in order to run correctly.")]
#endif
        public void ValidateUdpPubSubConnectionNetworkMessagePublishMulticast()
        {
            //Arrange 
            var localhost = GetFirstNic();
            Assert.IsNotNull(localhost, "localhost is null");
            Assert.IsNotNull(localhost.Address, "localhost.Address is null");

            //create publisher configuration object with modified port
            string configurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            PubSubConfigurationDataType publisherConfiguration = UaPubSubConfigurationHelper.LoadConfiguration(configurationFile);
            Assert.IsNotNull(publisherConfiguration, "publisherConfiguration is null");
            Assert.Greater(publisherConfiguration.Connections.Count, 1, "publisherConfiguration.Connection should be > 0");

            IPAddress[] multicastIPAddresses = Dns.GetHostAddresses(UdpMulticastIp);
            IPAddress multicastIPAddress = multicastIPAddresses.First();
            Assert.IsNotNull(multicastIPAddress, "multicastIPAddress is null");

            NetworkAddressUrlDataType publisherAddress = new NetworkAddressUrlDataType();
            publisherAddress.Url = Utils.Format(UdpUrlFormat, Utils.UriSchemeOpcUdp, multicastIPAddress.ToString());
            publisherConfiguration.Connections.First().Address = new ExtensionObject(publisherAddress);

            //create publisher UaPubSubApplication with changed configuration settings
            UaPubSubApplication publisherApplication = UaPubSubApplication.Create(publisherConfiguration);
            Assert.IsNotNull(publisherApplication, "publisherApplication is null");

            UdpPubSubConnection publisherConnection = publisherApplication.PubSubConnections.First() as UdpPubSubConnection;
            Assert.IsNotNull(publisherConnection, "publisherConnection is null");

            // will signal that the uadp message was received from local ip
            shutdownEvent_ = new ManualResetEvent(false);

            //setup uadp client for receiving from multicast (simulate a subscriber multicast)
            UdpClient udpMulticastClient = new UdpClientMulticast(localhost.Address, multicastIPAddress, DiscoveryPortNo);
            udpMulticastClient.BeginReceive(new AsyncCallback(OnReceive), udpMulticastClient);

            // prepare a network message
            WriterGroupDataType writerGroup0 = publisherConnection.PubSubConnectionConfiguration.WriterGroups.First();
            IList<UaNetworkMessage> networkMessages = publisherConnection.CreateNetworkMessages(writerGroup0, new WriterGroupPublishState());
            Assert.IsNotNull(networkMessages, "connection.CreateNetworkMessages shall not return null");

            //Act  
            publisherConnection.Start();

            if (networkMessages != null)
            {
                foreach (UaNetworkMessage uaNetworkMessage in networkMessages)
                {
                    if (uaNetworkMessage != null)
                    {
                        publisherConnection.PublishNetworkMessage(uaNetworkMessage);
                    }
                }
            }

            //Assert
            bool noMessageReceived = false;
            if (!shutdownEvent_.WaitOne(EstimatedPublishingTime))
            {
                noMessageReceived = true;
            }

            publisherConnection.Stop();
            udpMulticastClient.Close();
            udpMulticastClient.Dispose();

            if (noMessageReceived)
            {
                Assert.Fail("The UDP message was not received");
            }
        }

        [Test(Description = "Validate discovery request PublishNetworkMessage for a DataSetMetaData"), Order(4)]
#if !CUSTOM_TESTS
        [Ignore("A network interface controller is necessary in order to run correctly.")]
#endif
        public void ValidateUdpPubSubConnectionNetworkMessageDiscoveryPublish_DataSetMetadata()
        {
            //Arrange 
            var localhost = GetFirstNic();
            Assert.IsNotNull(localhost, "localhost is null");
            Assert.IsNotNull(localhost.Address, "localhost.Address is null");

            //create publisher configuration object with modified port
            string configurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            PubSubConfigurationDataType publisherConfiguration = UaPubSubConfigurationHelper.LoadConfiguration(configurationFile);
            Assert.IsNotNull(publisherConfiguration, "publisherConfiguration is null");
            Assert.Greater(publisherConfiguration.Connections.Count, 1, "publisherConfiguration.Connection should be > 0");

            //discovery IP address 224.0.2.14
            IPAddress[] multicastIPAddresses =  Dns.GetHostAddresses(UdpDiscoveryIp);
            IPAddress multicastIPAddress = multicastIPAddresses.First();
            Assert.IsNotNull(multicastIPAddress, "multicastIPAddress is null");

            NetworkAddressUrlDataType publisherAddress = new NetworkAddressUrlDataType();
            publisherAddress.Url = Utils.Format(UdpUrlFormat, Utils.UriSchemeOpcUdp, multicastIPAddress.ToString());
            publisherConfiguration.Connections[0].Address = new ExtensionObject(publisherAddress);

            //create publisher UaPubSubApplication with changed configuration settings
            UaPubSubApplication publisherApplication = UaPubSubApplication.Create(publisherConfiguration);
            Assert.IsNotNull(publisherApplication, "publisherApplication is null");
                        
            UdpPubSubConnection publisherConnection = publisherApplication.PubSubConnections.First() as UdpPubSubConnection;
            Assert.IsNotNull(publisherConnection, "publisherConnection is null");

            // will signal that the uadp message was received from local ip
            shutdownEvent_ = new ManualResetEvent(false);

            //setup uadp client for receiving from multicast (simulate a subscriber multicast)
            UdpClient udpMulticastClient = new UdpClientMulticast(localhost.Address, multicastIPAddress, DiscoveryPortNo);
            udpMulticastClient.BeginReceive(new AsyncCallback(OnReceive), udpMulticastClient);

            // prepare a network message
            WriterGroupDataType writerGroup0 = publisherConnection.PubSubConnectionConfiguration.WriterGroups.First();
            List<UInt16> dataSetWriterIds = new List<UInt16>();
            foreach (DataSetWriterDataType dataSetWriterDataType in writerGroup0.DataSetWriters)
            {
                dataSetWriterIds.Add(dataSetWriterDataType.DataSetWriterId);
            }
            IList<UaNetworkMessage> networkMessages = publisherConnection.CreateDataSetMetaDataNetworkMessages(dataSetWriterIds.ToArray());
            Assert.IsNotNull(networkMessages, "connection.CreateNetworkMessages shall not return null");

            //Act  
            publisherConnection.Start();

            if (networkMessages != null)
            {
                foreach (UaNetworkMessage uaNetworkMessage in networkMessages)
                {
                    if (uaNetworkMessage != null)
                    {
                        publisherConnection.PublishNetworkMessage(uaNetworkMessage);
                    }
                }
            }

            //Assert
            bool noMessageReceived = false;
            if (!shutdownEvent_.WaitOne(EstimatedPublishingTime))
            {
                noMessageReceived = true;
            }

            publisherConnection.Stop();
            udpMulticastClient.Close();
            udpMulticastClient.Dispose();
            
            if (noMessageReceived)
            {
                Assert.Fail("The UDP message was not received");
            }
        }


        [Test(Description = "Validate discovery DataSetWriterConfigurationMessage response"), Order(4)]
#if !CUSTOM_TESTS
        [Ignore("A network interface controller is necessary in order to run correctly.")]
#endif
        public void ValidateUdpPubSubConnectionNetworkMessageDiscoveryPublish_DataSetWriterConfiguration()
        {
            //Arrange 
            var localhost = GetFirstNic();
            Assert.IsNotNull(localhost, "localhost is null");
            Assert.IsNotNull(localhost.Address, "localhost.Address is null");

            //create publisher configuration object with modified port
            string configurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            PubSubConfigurationDataType publisherConfiguration = UaPubSubConfigurationHelper.LoadConfiguration(configurationFile);
            Assert.IsNotNull(publisherConfiguration, "publisherConfiguration is null");
            Assert.Greater(publisherConfiguration.Connections.Count, 1, "publisherConfiguration.Connection should be > 0");

            //discovery IP address 224.0.2.14
            IPAddress[] multicastIPAddresses = Dns.GetHostAddresses(UdpDiscoveryIp);
            IPAddress multicastIPAddress = multicastIPAddresses.First();
            Assert.IsNotNull(multicastIPAddress, "multicastIPAddress is null");

            NetworkAddressUrlDataType publisherAddress = new NetworkAddressUrlDataType();
            publisherAddress.Url = Utils.Format(UdpUrlFormat, Utils.UriSchemeOpcUdp, multicastIPAddress.ToString());
            publisherConfiguration.Connections[0].Address = new ExtensionObject(publisherAddress);

            //create publisher UaPubSubApplication with changed configuration settings
            UaPubSubApplication publisherApplication = UaPubSubApplication.Create(publisherConfiguration);
            Assert.IsNotNull(publisherApplication, "publisherApplication is null");

            UdpPubSubConnection publisherConnection = publisherApplication.PubSubConnections.First() as UdpPubSubConnection;
            Assert.IsNotNull(publisherConnection, "publisherConnection is null");

            // will signal that the uadp message was received from local ip
            shutdownEvent_ = new ManualResetEvent(false);

            //setup uadp client for receiving from multicast (simulate a subscriber multicast)
            UdpClient udpMulticastClient = new UdpClientMulticast(localhost.Address, multicastIPAddress, DiscoveryPortNo);
            udpMulticastClient.BeginReceive(new AsyncCallback(OnReceive), udpMulticastClient);

            // prepare a network message
            WriterGroupDataType writerGroup0 = publisherConnection.PubSubConnectionConfiguration.WriterGroups.First();
            List<UInt16> dataSetWriterIds = new List<UInt16>();
            foreach (DataSetWriterDataType dataSetWriterDataType in writerGroup0.DataSetWriters)
            {
                dataSetWriterIds.Add(dataSetWriterDataType.DataSetWriterId);
            }
            UaNetworkMessage networkMessage = publisherConnection.CreateDataSetWriterCofigurationMessage(dataSetWriterIds.ToArray()).First();
            Assert.IsNotNull(networkMessage, "connection.CreateDataSetWriterCofigurationMessages shall not return null");

            //Act  
            publisherConnection.Start();

            if (networkMessage != null)
            {
                publisherConnection.PublishNetworkMessage(networkMessage);
            }

            //Assert
            bool noMessageReceived = false;
            if (!shutdownEvent_.WaitOne(EstimatedPublishingTime))
            {
                noMessageReceived = true;
            }

            publisherConnection.Stop();
            udpMulticastClient.Close();
            udpMulticastClient.Dispose();

            if (noMessageReceived)
            {
                Assert.Fail("The UDP message was not received");
            }
        }

        [Test(Description = "Validate discovery request PublishNetworkMessage for PublisherEndpoints"), Order(4)]
#if !CUSTOM_TESTS
        [Ignore("A network interface controller is necessary in order to run correctly.")]
#endif
        public void ValidateUdpPubSubConnectionNetworkMessageDiscoveryPublish_PublisherEndpoints()
        {
            //Arrange 
            var localhost = GetFirstNic();
            Assert.IsNotNull(localhost, "localhost is null");
            Assert.IsNotNull(localhost.Address, "localhost.Address is null");

            //create publisher configuration object with modified port
            string configurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            PubSubConfigurationDataType publisherConfiguration = UaPubSubConfigurationHelper.LoadConfiguration(configurationFile);
            Assert.IsNotNull(publisherConfiguration, "publisherConfiguration is null");
            Assert.Greater(publisherConfiguration.Connections.Count, 1, "publisherConfiguration.Connection should be > 0");

            //discovery IP address 224.0.2.14
            IPAddress[] multicastIPAddresses = Dns.GetHostAddresses(UdpDiscoveryIp);
            IPAddress multicastIPAddress = multicastIPAddresses.First();
            Assert.IsNotNull(multicastIPAddress, "multicastIPAddress is null");

            NetworkAddressUrlDataType publisherAddress = new NetworkAddressUrlDataType();
            publisherAddress.Url = Utils.Format(UdpUrlFormat, Utils.UriSchemeOpcUdp, multicastIPAddress.ToString());
            publisherConfiguration.Connections[0].Address = new ExtensionObject(publisherAddress);

            //create publisher UaPubSubApplication with changed configuration settings
            UaPubSubApplication publisherApplication = UaPubSubApplication.Create(publisherConfiguration);
            Assert.IsNotNull(publisherApplication, "publisherApplication is null");

            UdpPubSubConnection publisherConnection = publisherApplication.PubSubConnections.First() as UdpPubSubConnection;
            Assert.IsNotNull(publisherConnection, "publisherConnection is null");

            // will signal that the uadp message was received from local ip
            shutdownEvent_ = new ManualResetEvent(false);

            //setup uadp client for receiving from multicast (simulate a subscriber multicast)
            UdpClient udpMulticastClient = new UdpClientMulticast(localhost.Address, multicastIPAddress, DiscoveryPortNo);
            udpMulticastClient.BeginReceive(new AsyncCallback(OnReceive), udpMulticastClient);

            List<EndpointDescription> endpointDescriptions = new List<EndpointDescription>()
            {
                new EndpointDescription() {
                    EndpointUrl = "opc.tcp://server1:4840/Test",
                    SecurityMode = MessageSecurityMode.None,
                    SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#None",
                    Server = new ApplicationDescription() { ApplicationName = "Test security mode None", ApplicationUri = "urn:localhost:Server" }
                },
                new EndpointDescription()
                {
                    EndpointUrl = "opc.tcp://server1:4840/Test",
                    SecurityMode = MessageSecurityMode.Sign,
                    SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256",
                    Server = new ApplicationDescription() { ApplicationName = "Test security mode Sign", ApplicationUri = "urn:localhost:Server" }
                },
                new EndpointDescription()
                {
                    EndpointUrl = "opc.tcp://server1:4840/Test",
                    SecurityMode = MessageSecurityMode.SignAndEncrypt,
                    SecurityPolicyUri = "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256",
                    Server = new ApplicationDescription() { ApplicationName = "Test security mode SignAndEncrypt", ApplicationUri = "urn:localhost:Server" }
                }
            };

            UaNetworkMessage uaNetworkMessage = publisherConnection.CreatePublisherEndpointsNetworkMessage(endpointDescriptions.ToArray(), 
                StatusCodes.Good, publisherConnection.PubSubConnectionConfiguration.PublisherId.Value);
            Assert.IsNotNull(uaNetworkMessage, "uaNetworkMessage shall not return null");

            //Act  
            publisherConnection.Start();

            publisherConnection.PublishNetworkMessage(uaNetworkMessage);

            // Assert
            bool noMessageReceived = false;
            if (!shutdownEvent_.WaitOne(EstimatedPublishingTime))
            {
                noMessageReceived = true;
            }
           
            publisherConnection.Stop();
            udpMulticastClient.Close();
            udpMulticastClient.Dispose();

            if (noMessageReceived)
            {
                Assert.Fail("The UDP message was not received");
            }
        }

        /// <summary>
        /// Handle Receive event for an UADP channel
        /// </summary>
        /// <param name="result"></param>
        private void OnReceive(IAsyncResult result)
        {
            try
            {
                // this is what had been passed into BeginReceive as the second parameter:
                UdpClient socket = result.AsyncState as UdpClient;
                // points towards whoever had sent the message:
                IPEndPoint source = new IPEndPoint(0, 0);
                // get the actual message and fill out the source:
                socket?.EndReceive(result, ref source);

                if (IsHostAddress(source.Address.ToString()))
                {
                    //signal that uadp message was received from local ip
                    shutdownEvent_.Set();
                    return;
                }

                // schedule the next receive operation once reading is done:
                socket?.BeginReceive(new AsyncCallback(OnReceive), socket);
            }
            catch (Exception ex)
            {
                Assert.Warn(Utils.Format("OnReceive() failed due to the following reason: {0}", ex.Message));
            }
        }

    }
}
