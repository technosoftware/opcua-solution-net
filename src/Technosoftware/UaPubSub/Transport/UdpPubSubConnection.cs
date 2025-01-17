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
using System.Threading.Tasks;

using Opc.Ua;

using Technosoftware.UaPubSub.Configuration;
using Technosoftware.UaPubSub.Encoding;
using Technosoftware.UaPubSub.PublishedData;
#endregion

namespace Technosoftware.UaPubSub.Transport
{
    /// <summary>
    /// UADP implementation of <see cref="UaPubSubConnection"/> class.
    /// </summary>
    internal class UdpPubSubConnection : UaPubSubConnection, IUadpDiscoveryMessages
    {
        #region Private Fields
        private List<UdpClient> publisherUdpClients_ = new List<UdpClient>();
        private List<UdpClient> subscriberUdpClients_ = new List<UdpClient>();
        private UdpDiscoverySubscriber udpDiscoverySubscriber_;
        private UdpDiscoveryPublisher udpDiscoveryPublisher_;
        private static int sequenceNumber_ = 0;
        private static int dataSetSequenceNumber_ = 0;
        #endregion

        #region Constructor
        /// <summary>
        ///  Create new instance of <see cref="UdpPubSubConnection"/> from <see cref="PubSubConnectionDataType"/> configuration data
        /// </summary>
        public UdpPubSubConnection(UaPubSubApplication uaPubSubApplication, PubSubConnectionDataType pubSubConnectionDataType)
            : base(uaPubSubApplication, pubSubConnectionDataType)
        {
            transportProtocol_ = TransportProtocol.UDP;

            Utils.Trace("UdpPubSubConnection with name '{0}' was created.", pubSubConnectionDataType.Name);

            Initialize();
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set the event handler
        /// </summary>
        public GetPublisherEndpointsEventHandler GetPublisherEndpoints { get; set; }

        /// <summary>
        /// Get the NetworkInterface name from configured <see cref="PubSubConnectionDataType"/>.Address.
        /// </summary>
        public string NetworkInterfaceName { get; set; }

        /// <summary>
        /// Get the <see cref="IPEndPoint"/> from configured <see cref="PubSubConnectionDataType"/>.Address.
        /// </summary>
        public IPEndPoint NetworkAddressEndPoint { get; private set; }

        /// <summary>
        /// Get the port from configured <see cref="PubSubConnectionDataType"/>.Address
        /// </summary>
        public int Port { get; private set; }
        #endregion

        #region UaPubSubConnection - Overrides
        /// <summary>
        /// Perform specific Start tasks
        /// </summary>
        protected override async Task InternalStart()
        {
            //cleanup all existing UdpClient previously open
            await InternalStop().ConfigureAwait(false);

            if (NetworkAddressEndPoint == null)
            {
                return;
            }

            //publisher initialization    
            if (Publishers.Count > 0)
            {
                lock (Lock)
                {
                    publisherUdpClients_ = UdpClientCreator.GetUdpClients(UsedInContext.Publisher, NetworkInterfaceName, NetworkAddressEndPoint);
                }

                udpDiscoveryPublisher_ = new UdpDiscoveryPublisher(this);
                await udpDiscoveryPublisher_.StartAsync(MessageContext).ConfigureAwait(false);
            }

            //subscriber initialization   
            if (GetAllDataSetReaders().Count > 0)
            {
                lock (Lock)
                {
                    subscriberUdpClients_ = UdpClientCreator.GetUdpClients(UsedInContext.Subscriber, NetworkInterfaceName, NetworkAddressEndPoint);

                    foreach (UdpClient subscriberUdpClient in subscriberUdpClients_)
                    {
                        try
                        {
                            subscriberUdpClient.BeginReceive(new AsyncCallback(OnUadpReceive), subscriberUdpClient);
                        }
                        catch (Exception ex)
                        {
                            Utils.Trace(Utils.TraceMasks.Information, "UdpClient '{0}' Cannot receive data. Exception: {1}",
                              subscriberUdpClient.Client.LocalEndPoint, ex.Message);
                        }
                    }
                }

                // initialize the discovery channel
                udpDiscoverySubscriber_ = new UdpDiscoverySubscriber(this);
                await udpDiscoverySubscriber_.StartAsync(MessageContext).ConfigureAwait(false);

                // add handler to metaDataReceived event
                this.Application.MetaDataReceivedEvent += MetaDataReceived;
                this.Application.DataSetWriterConfigurationReceivedEvent += DataSetWriterConfigurationReceived;
            }
        }

        /// <summary>
        /// Perform specific Stop tasks
        /// </summary>
        protected override async Task InternalStop()
        {
            lock (Lock)
            {
                foreach (var list in new List<List<UdpClient>>() { publisherUdpClients_, subscriberUdpClients_ })
                {
                    if (list != null && list.Count > 0)
                    {
                        foreach (var udpClient in list)
                        {
                            udpClient.Close();
                            udpClient.Dispose();
                        }
                        list.Clear();
                    }
                }
            }

            if (udpDiscoveryPublisher_ != null)
            {
                await udpDiscoveryPublisher_.StopAsync().ConfigureAwait(false);
            }

            if (udpDiscoverySubscriber_ != null)
            {
                await udpDiscoverySubscriber_.StopAsync().ConfigureAwait(false);

                // remove handler to metaDataReceived event
                this.Application.MetaDataReceivedEvent -= MetaDataReceived;
            }
        }

        /// <summary>
        /// Create the list of network messages built from the provided writerGroupConfiguration
        /// </summary>
        public override IList<UaNetworkMessage> CreateNetworkMessages(WriterGroupDataType writerGroupConfiguration, WriterGroupPublishState state)
        {
            var messageSettings = ExtensionObject.ToEncodeable(writerGroupConfiguration.MessageSettings)
                as UadpWriterGroupMessageDataType;
            if (messageSettings == null)
            {
                //Wrong configuration of writer group MessageSettings
                return null;
            }
            var transportSettings = ExtensionObject.ToEncodeable(writerGroupConfiguration.TransportSettings)
                as DatagramWriterGroupTransportDataType;

            if (transportSettings == null)
            {
                //Wrong configuration of writer group TransportSettings
                return null;
            }
            var networkMessages = new List<UaNetworkMessage>();

            //Create list of dataSet messages to be sent
            var dataSetMessages = new List<UadpDataSetMessage>();
            foreach (DataSetWriterDataType dataSetWriter in writerGroupConfiguration.DataSetWriters)
            {
                //check if dataSetWriter enabled
                if (dataSetWriter.Enabled)
                {
                    DataSet dataSet = CreateDataSet(dataSetWriter, state);

                    if (dataSet != null)
                    {
                        var hasMetaDataChanged = state.HasMetaDataChanged(dataSetWriter, dataSet.DataSetMetaData);

                        if (hasMetaDataChanged)
                        {
                            // add metadata network message
                            networkMessages.Add(new UadpNetworkMessage(writerGroupConfiguration, dataSet.DataSetMetaData) {
                                PublisherId = PubSubConnectionConfiguration.PublisherId.Value,
                                DataSetWriterId = dataSetWriter.DataSetWriterId
                            });
                        }

                        var dataSetMessageSettings = ExtensionObject.ToEncodeable(dataSetWriter.MessageSettings) as
                                UadpDataSetWriterMessageDataType;
                        // check MessageSettings to see how to encode DataSet
                        if (dataSetMessageSettings != null)
                        {
                            var uadpDataSetMessage = new UadpDataSetMessage(dataSet);
                            uadpDataSetMessage.DataSetWriterId = dataSetWriter.DataSetWriterId;
                            uadpDataSetMessage.SetMessageContentMask((UadpDataSetMessageContentMask)dataSetMessageSettings.DataSetMessageContentMask);
                            uadpDataSetMessage.SetFieldContentMask((DataSetFieldContentMask)dataSetWriter.DataSetFieldContentMask);
                            uadpDataSetMessage.SequenceNumber = (ushort)(Utils.IncrementIdentifier(ref dataSetSequenceNumber_) % UInt16.MaxValue);
                            uadpDataSetMessage.ConfiguredSize = dataSetMessageSettings.ConfiguredSize;
                            uadpDataSetMessage.DataSetOffset = dataSetMessageSettings.DataSetOffset;
                            uadpDataSetMessage.Timestamp = DateTime.UtcNow;
                            uadpDataSetMessage.Status = StatusCodes.Good;
                            dataSetMessages.Add(uadpDataSetMessage);

                            state.OnMessagePublished(dataSetWriter, dataSet);
                        }
                    }
                }
            }

            //cancel send if no dataset message
            if (dataSetMessages.Count == 0)
            {
                return networkMessages;
            }

            var uadpNetworkMessage = new UadpNetworkMessage(writerGroupConfiguration, dataSetMessages);
            uadpNetworkMessage.SetNetworkMessageContentMask((UadpNetworkMessageContentMask)messageSettings.NetworkMessageContentMask);
            uadpNetworkMessage.WriterGroupId = writerGroupConfiguration.WriterGroupId;
            // Network message header
            uadpNetworkMessage.PublisherId = PubSubConnectionConfiguration.PublisherId.Value;
            uadpNetworkMessage.SequenceNumber = (ushort)(Utils.IncrementIdentifier(ref sequenceNumber_) % UInt16.MaxValue);

            // Writer group header
            uadpNetworkMessage.GroupVersion = messageSettings.GroupVersion;
            uadpNetworkMessage.NetworkMessageNumber = 1; //only one network message per publish

            networkMessages.Add(uadpNetworkMessage);


            return networkMessages;
        }

        /// <summary>
        /// Create and return the list of DataSetMetaData response messages 
        /// </summary>
        /// <param name="dataSetWriterIds"></param>
        /// <returns></returns>
        public IList<UaNetworkMessage> CreateDataSetMetaDataNetworkMessages(UInt16[] dataSetWriterIds)
        {
            var networkMessages = new List<UaNetworkMessage>();
            var writers = GetWriterGroupsDataType();

            foreach (var dataSetWriterId in dataSetWriterIds)
            {
                DataSetWriterDataType writer = writers.Where(w => w.DataSetWriterId == dataSetWriterId).FirstOrDefault();
                if (writer != null)
                {
                    WriterGroupDataType writerGroup = PubSubConnectionConfiguration.WriterGroups.Where(wg => wg.DataSetWriters.Contains(writer)).FirstOrDefault();
                    if (writerGroup != null)
                    {
                        DataSetMetaDataType metaData = Application.DataCollector.GetPublishedDataSet(writer.DataSetName)?.DataSetMetaData;
                        if (metaData != null)
                        {
                            var networkMessage = new UadpNetworkMessage(writerGroup, metaData);
                            networkMessage.PublisherId = PubSubConnectionConfiguration.PublisherId.Value;
                            networkMessage.DataSetWriterId = dataSetWriterId;

                            networkMessages.Add(networkMessage);
                        }
                    }
                }
            }

            return networkMessages;
        }

        /// <summary>
        /// Create and return the list of DataSetWriterConfiguration response message
        /// </summary>
        /// <param name="dataSetWriterIds">DatasetWriter ids</param>
        /// <returns></returns>
        public IList<UaNetworkMessage> CreateDataSetWriterCofigurationMessage(UInt16[] dataSetWriterIds)
        {
            var networkMessages = new List<UaNetworkMessage>();

            IList<DataSetWriterConfigurationResponse> responses = GetDataSetWriterDiscoveryResponses(dataSetWriterIds);

            foreach (DataSetWriterConfigurationResponse response in responses)
            {
                var networkMessage = new UadpNetworkMessage(response.DataSetWriterIds,
                    response.DataSetWriterConfig,
                    response.StatusCodes);

                networkMessage.PublisherId = PubSubConnectionConfiguration.PublisherId.Value;
                networkMessage.MessageStatusCodes.ToList().AddRange(response.StatusCodes);
                networkMessages.Add(networkMessage);
            }

            return networkMessages;
        }

        /// <summary>
        /// Publish the network message
        /// </summary>
        public override bool PublishNetworkMessage(UaNetworkMessage networkMessage)
        {
            if (networkMessage == null || publisherUdpClients_ == null || publisherUdpClients_.Count == 0)
            {
                return false;
            }

            try
            {
                lock (Lock)
                {
                    if (publisherUdpClients_ != null && publisherUdpClients_.Count > 0)
                    {
                        // Get encoded bytes
                        var bytes = networkMessage.Encode(MessageContext);

                        foreach (var udpClient in publisherUdpClients_)
                        {
                            try
                            {
                                udpClient.Send(bytes, bytes.Length, NetworkAddressEndPoint);

                                Utils.Trace("UdpPubSubConnection.PublishNetworkMessage bytes:{0}, endpoint:{1}", bytes.Length, NetworkAddressEndPoint);
                            }
                            catch (Exception ex)
                            {
                                Utils.Trace(ex, "UdpPubSubConnection.PublishNetworkMessage");
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Trace(ex, "UdpPubSubConnection.PublishNetworkMessage");
                return false;
            }

            return false;
        }

        /// <summary>
        /// Always returns true since UDP is a connectionless protocol
        /// </summary>
        public override bool AreClientsConnected()
        {
            return true;
        }

        #region IUadpDiscoveryMessages interface methods

        /// <summary>
        /// Set GetPublisherEndpoints callback used by the subscriber to receive PublisherEndpoints data from publisher
        /// </summary>
        /// <param name="getPubliherEndpoints"></param>
        public void GetPublisherEndpointsCallback(GetPublisherEndpointsEventHandler getPubliherEndpoints)
        {
            if (udpDiscoveryPublisher_ != null)
            {
                udpDiscoveryPublisher_.GetPublisherEndpoints = getPubliherEndpoints;
            }
        }

        /// <summary>
        /// Set GetDataSetWriterConfiguration callback used by the subscriber to receive DataSetWriter ids from publisher
        /// </summary>
        /// <param name="getDataSetWriterIds"></param>
        public void GetDataSetWriterConfigurationCallback(GetDataSetWriterIdsEventHandler getDataSetWriterIds)
        {
            if (udpDiscoveryPublisher_ != null)
            {
                udpDiscoveryPublisher_.GetDataSetWriterIds = getDataSetWriterIds;
            }
        }

        /// <summary>
        /// Create and return the list of EndpointDescription response messages
        /// To be used only by UADP Discovery response messages
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="publisherProvideEndpointsStatusCode"></param>
        /// <param name="publisherId"></param>
        /// <returns></returns>
        public UaNetworkMessage CreatePublisherEndpointsNetworkMessage(EndpointDescription[] endpoints,
            StatusCode publisherProvideEndpointsStatusCode, object publisherId)
        {
            if (PubSubConnectionConfiguration != null &&
                PubSubConnectionConfiguration.TransportProfileUri == Profiles.PubSubUdpUadpTransport)
            {
                var networkMessage = new UadpNetworkMessage(endpoints, publisherProvideEndpointsStatusCode);
                networkMessage.PublisherId = publisherId;

                return networkMessage;
            }

            return null;
        }

        /// <summary>
        /// Request UADP Discovery Publisher endpoints only
        /// </summary>
        public void RequestPublisherEndpoints()
        {
            if (PubSubConnectionConfiguration != null &&
                PubSubConnectionConfiguration.TransportProfileUri == Profiles.PubSubUdpUadpTransport &&
                udpDiscoverySubscriber_ != null)
            {
                // send discovery request publisher endpoints here for now
                udpDiscoverySubscriber_.SendDiscoveryRequestPublisherEndpoints();
            }
        }

        /// <summary>
        /// Request UADP Discovery DataSetWriterConfiguration messages
        /// </summary>
        public void RequestDataSetWriterConfiguration()
        {
            if (PubSubConnectionConfiguration != null &&
                PubSubConnectionConfiguration.TransportProfileUri == Profiles.PubSubUdpUadpTransport &&
                udpDiscoverySubscriber_ != null)
            {
                udpDiscoverySubscriber_.SendDiscoveryRequestDataSetWriterConfiguration();
            }
        }

        /// <summary>
        /// Request DataSetMetaData
        /// </summary>
        public void RequestDataSetMetaData()
        {
            if (udpDiscoverySubscriber_ != null)
            {
                udpDiscoverySubscriber_.SendDiscoveryRequestDataSetMetaData();
            }
        }
        #endregion
        #endregion

        #region Private methods
        /// <summary>
        /// Initialize Connection properties from connection configuration object
        /// </summary>
        private void Initialize()
        {
            var networkAddressUrlState = ExtensionObject.ToEncodeable(PubSubConnectionConfiguration.Address)
                       as NetworkAddressUrlDataType;
            if (networkAddressUrlState == null)
            {
                Utils.Trace(Utils.TraceMasks.Error, "The configuration for connection {0} has invalid Address configuration.",
                          PubSubConnectionConfiguration.Name);
                return;
            }
            // set properties
            NetworkInterfaceName = networkAddressUrlState.NetworkInterface;
            NetworkAddressEndPoint = UdpClientCreator.GetEndPoint(networkAddressUrlState.Url);

            if (NetworkAddressEndPoint == null)
            {
                Utils.Trace(Utils.TraceMasks.Error, "The configuration for connection {0} with Url:'{1}' resulted in an invalid endpoint.",
                          PubSubConnectionConfiguration.Name, networkAddressUrlState.Url);
            }
        }

        /// <summary>
        /// Process the bytes received from UADP channel as Subscriber
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        private void ProcessReceivedMessage(byte[] message, IPEndPoint source)
        {
            Utils.Trace(Utils.TraceMasks.Information, "UdpPubSubConnection.ProcessReceivedMessage from source={0}", source);

            List<DataSetReaderDataType> dataSetReaders = GetOperationalDataSetReaders();
            var dataSetReadersToDecode = new List<DataSetReaderDataType>();

            foreach (DataSetReaderDataType dataSetReader in dataSetReaders)
            {
                // check if dataSetReaders have metadata information
                if (!ConfigurationVersionUtils.IsUsable(dataSetReader.DataSetMetaData))
                {
                    // check if it is possible to request the metadata information
                    if (dataSetReader.DataSetWriterId != 0)
                    {
                        udpDiscoverySubscriber_.AddWriterIdForDataSetMetadata(dataSetReader.DataSetWriterId);
                    }
                }
                else
                {
                    dataSetReadersToDecode.Add(dataSetReader);
                }
            }

            var networkMessage = new UadpNetworkMessage();
            networkMessage.DataSetDecodeErrorOccurred += NetworkMessage_DataSetDecodeErrorOccurred;
            networkMessage.Decode(MessageContext, message, dataSetReadersToDecode);
            networkMessage.DataSetDecodeErrorOccurred -= NetworkMessage_DataSetDecodeErrorOccurred;

            // Process the decoded network message 
            ProcessDecodedNetworkMessage(networkMessage, source.ToString());
        }


        /// <summary>
        /// Handle Receive event for an UADP channel on Subscriber Side
        /// </summary>
        /// <param name="result"></param>
        private void OnUadpReceive(IAsyncResult result)
        {
            lock (Lock)
            {
                if (subscriberUdpClients_ == null || subscriberUdpClients_.Count == 0)
                {
                    return;
                }
            }

            // this is what had been passed into BeginReceive as the second parameter:
            var socket = result.AsyncState as UdpClient;

            if (socket == null)
            {
                return;
            }

            // points towards whoever had sent the message:
            var source = new IPEndPoint(0, 0);
            // get the actual message and fill out the source:
            try
            {
                var message = socket.EndReceive(result, ref source);

                if (message != null)
                {
                    Utils.Trace("OnUadpReceive received message with length {0} from {1}", message.Length, source.Address);

                    if (message.Length > 1)
                    {
                        // raise RawData received event
                        var rawDataReceivedEventArgs = new RawDataReceivedEventArgs() {
                            Message = message,
                            Source = source.Address.ToString(),
                            TransportProtocol = this.TransportProtocol,
                            MessageMapping = MessageMapping.Uadp,
                            PubSubConnectionConfiguration = PubSubConnectionConfiguration
                        };

                        // trigger notification for received raw data
                        Application.RaiseRawDataReceivedEvent(rawDataReceivedEventArgs);

                        // check if the RawData message is marked as handled
                        if (rawDataReceivedEventArgs.Handled)
                        {
                            Utils.Trace("UdpConnection message from source={0} is marked as handled and will not be decoded.", rawDataReceivedEventArgs.Source);
                            return;
                        }

                        // call on a new thread
                        Task.Run(() => {
                            ProcessReceivedMessage(message, source);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Trace(ex, "OnUadpReceive from {0}", source.Address);
            }

            try
            {
                // schedule the next receive operation once reading is done:
                socket.BeginReceive(new AsyncCallback(OnUadpReceive), socket);
            }
            catch (Exception ex)
            {
                Utils.Trace(Utils.TraceMasks.Information, "OnUadpReceive BeginReceive threw Exception {0}", ex.Message);

                lock (Lock)
                {
                    Renew(socket);
                }
            }
        }

        /// <summary>
        /// Re initializes the socket 
        /// </summary>
        /// <param name="socket">The socket which should be reinitialized</param>
        private void Renew(UdpClient socket)
        {
            UdpClient newsocket = null;

            if (socket is UdpClientMulticast mcastSocket)
            {
                newsocket = new UdpClientMulticast(mcastSocket.Address, mcastSocket.MulticastAddress, mcastSocket.Port);
            }
            else if (socket is UdpClientBroadcast bcastSocket)
            {
                newsocket = new UdpClientBroadcast(bcastSocket.Address, bcastSocket.Port, bcastSocket.PubSubContext);
            }
            else if (socket is UdpClientUnicast ucastSocket)
            {
                newsocket = new UdpClientUnicast(ucastSocket.Address, ucastSocket.Port);
            }
            subscriberUdpClients_.Remove(socket);
            subscriberUdpClients_.Add(newsocket);
            socket.Close();
            socket.Dispose();

            if (newsocket != null)
            {
                newsocket.BeginReceive(new AsyncCallback(OnUadpReceive), newsocket);
            }
        }

        /// <summary>
        /// Resets SequenceNumber 
        /// </summary>
        internal static void ResetSequenceNumber()
        {
            sequenceNumber_ = 0;
            dataSetSequenceNumber_ = 0;
        }

        /// <summary>
        /// Handle <see cref="UaPubSubApplication.MetaDataReceivedEvent"/> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetaDataReceived(object sender, SubscribedDataEventArgs e)
        {
            if (udpDiscoverySubscriber_ != null && e.NetworkMessage.DataSetWriterId != null)
            {
                udpDiscoverySubscriber_.RemoveWriterIdForDataSetMetadata(e.NetworkMessage.DataSetWriterId.Value);
            }
        }


        /// <summary>
        /// Handler for DatasetWriterConfigurationReceived event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataSetWriterConfigurationReceived(object sender, DataSetWriterConfigurationEventArgs e)
        {
            lock (Lock)
            {
                WriterGroupDataType config = e.DataSetWriterConfiguration;
                if (e.DataSetWriterConfiguration != null)
                {
                    udpDiscoverySubscriber_.UpdateDataSetWriterConfiguration(config);
                }
            }
        }

        /// <summary>
        /// Handle <see cref="UaNetworkMessage.DataSetDecodeErrorOccurred"/> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkMessage_DataSetDecodeErrorOccurred(object sender, DataSetDecodeErrorEventArgs e)
        {
            if (e.DecodeErrorReason == DataSetDecodeErrorReason.MetadataMajorVersion)
            {
                // Resend metadata request
                // check if it is possible to request the metadata information
                if (e.DataSetReader.DataSetWriterId != 0)
                {
                    udpDiscoverySubscriber_.AddWriterIdForDataSetMetadata(e.DataSetReader.DataSetWriterId);
                }
            }
        }
        #endregion
    }
}
