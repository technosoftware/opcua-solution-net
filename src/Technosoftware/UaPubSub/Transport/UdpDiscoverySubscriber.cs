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
using System.Net.Sockets;
using System.Threading.Tasks;

using Opc.Ua;

using Technosoftware.UaPubSub.Encoding;
#endregion

namespace Technosoftware.UaPubSub.Transport
{
    /// <summary>
    /// Class responsible to manage the UDP Discovery Request/Response messages for a <see cref="UdpPubSubConnection"/> entity as a subscriber.
    /// </summary>
    internal class UdpDiscoverySubscriber : UdpDiscovery
    {
        #region  Private Fields
        private const int kInitialRequestInterval = 5000;

        // The list that will store the WriterIds that shall be included in a DataSetMetaData Request message
        private readonly List<UInt16> metadataWriterIdsToSend_;

        // the component that triggers the publish request messages
        private readonly IntervalRunner intervalRunner_;
        #endregion

        #region Constructor
        /// <summary>
        /// Create new instance of <see cref="UdpDiscoverySubscriber"/>
        /// </summary>
        /// <param name="udpConnection"></param>
        public UdpDiscoverySubscriber(UdpPubSubConnection udpConnection) : base(udpConnection)
        {
            metadataWriterIdsToSend_ = new List<ushort>();

            intervalRunner_ = new IntervalRunner(udpConnection.PubSubConnectionConfiguration.Name,
                kInitialRequestInterval, CanPublish, RequestDiscoveryMessages);

        }
        #endregion

        #region Start/Stop Method Overrides

        /// <summary>
        /// Implementation of StartAsync for the subscriber Discovery
        /// </summary>
        /// <param name="messageContext">The <see cref="IServiceMessageContext"/> object that should be used in encode/decode messages</param>
        /// <returns></returns>
        public override async Task StartAsync(IServiceMessageContext messageContext)
        {
            await base.StartAsync(messageContext).ConfigureAwait(false);

            intervalRunner_.Start();
        }

        /// <summary>
        /// Stop the UdpDiscovery process for Subscriber
        /// </summary>
        /// <returns></returns>
        public override async Task StopAsync()
        {
            await base.StopAsync().ConfigureAwait(false);

            intervalRunner_.Stop();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enqueue the specified DataSetWriterId for DataSetInformation to be requested 
        /// </summary>
        /// <param name="writerId"></param>
        public void AddWriterIdForDataSetMetadata(UInt16 writerId)
        {
            lock (lock_)
            {
                if (!metadataWriterIdsToSend_.Contains(writerId))
                {
                    metadataWriterIdsToSend_.Add(writerId);
                }
            }
        }

        /// <summary>
        /// Removes the specified DataSetWriterId for DataSetInformation to be requested 
        /// </summary>
        /// <param name="writerId"></param>
        public void RemoveWriterIdForDataSetMetadata(UInt16 writerId)
        {
            lock (lock_)
            {
                if (metadataWriterIdsToSend_.Contains(writerId))
                {
                    metadataWriterIdsToSend_.Remove(writerId);
                }
            }
        }
        /// <summary>
        /// Send a discovery Request for DataSetWriterConfiguration
        /// </summary>
        public void SendDiscoveryRequestDataSetWriterConfiguration()
        {
            var dataSetWriterIds = udpConnection_.PubSubConnectionConfiguration.ReaderGroups?
                .SelectMany(group => group.DataSetReaders)?
                .Select(group => group.DataSetWriterId)?
                .ToArray();

            var discoveryRequestDataSetWriterConfiguration = new UadpNetworkMessage(UADPNetworkMessageDiscoveryType.DataSetWriterConfiguration) {
                DataSetWriterIds = dataSetWriterIds,
                PublisherId = udpConnection_.PubSubConnectionConfiguration.PublisherId.Value,
            };

            var bytes = discoveryRequestDataSetWriterConfiguration.Encode(MessageContext);

            // send the Discovery request message to all open UADPClient 
            foreach (UdpClient udpClient in discoveryUdpClients_)
            {
                try
                {
                    Utils.Trace("UdpDiscoverySubscriber.SendDiscoveryRequestDataSetWriterConfiguration message");
                    udpClient.Send(bytes, bytes.Length, DiscoveryNetworkAddressEndPoint);
                }
                catch (Exception ex)
                {
                    Utils.Trace(ex, "UdpDiscoverySubscriber.SendDiscoveryRequestDataSetWriterConfiguration");
                }
            }

            // double the time between requests
            intervalRunner_.Interval = intervalRunner_.Interval * 2;
        }

        /// <summary>
        /// Updates the dataset writer configuration
        /// </summary>
        /// <param name="writerConfig">the configuration</param>
        public void UpdateDataSetWriterConfiguration(WriterGroupDataType writerConfig)
        {
            WriterGroupDataType writerGroup = udpConnection_.PubSubConnectionConfiguration.WriterGroups?
                .Find(x => x.WriterGroupId == writerConfig.WriterGroupId);
            if (writerGroup != null)
            {
                var index = udpConnection_.PubSubConnectionConfiguration.WriterGroups.IndexOf(writerGroup);
                udpConnection_.PubSubConnectionConfiguration.WriterGroups[index] = writerConfig;
            }
        }

        /// <summary>
        /// Send a discovery Request for PublisherEndpoints
        /// </summary>
        public void SendDiscoveryRequestPublisherEndpoints()
        {
            var discoveryRequestPublisherEndpoints = new UadpNetworkMessage(UADPNetworkMessageDiscoveryType.PublisherEndpoint);
            discoveryRequestPublisherEndpoints.PublisherId = udpConnection_.PubSubConnectionConfiguration.PublisherId.Value;

            var bytes = discoveryRequestPublisherEndpoints.Encode(MessageContext);

            // send the PublisherEndpoints DiscoveryRequest message to all open UdpClients
            foreach (var udpClient in discoveryUdpClients_)
            {
                try
                {
                    Utils.Trace("UdpDiscoverySubscriber.SendDiscoveryRequestPublisherEndpoints message for PublisherId: {0}",
                        discoveryRequestPublisherEndpoints.PublisherId);

                    udpClient.Send(bytes, bytes.Length, DiscoveryNetworkAddressEndPoint);
                }
                catch (Exception ex)
                {
                    Utils.Trace(ex, "UdpDiscoverySubscriber.SendDiscoveryRequestPublisherEndpoints");
                }
            }

            // double the time between requests
            intervalRunner_.Interval *= 2;
        }


        /// <summary>
        /// Create and Send the DiscoveryRequest messages for DataSetMetaData
        /// </summary>
        public void SendDiscoveryRequestDataSetMetaData()
        {
            UInt16[] dataSetWriterIds = null;
            lock (lock_)
            {
                dataSetWriterIds = metadataWriterIdsToSend_.ToArray();
                metadataWriterIdsToSend_.Clear();
            }

            if (dataSetWriterIds == null || dataSetWriterIds.Length == 0)
            {
                return;
            }

            // create the DataSetMetaData DiscoveryRequest message
            var discoveryRequestMetaDataMessage = new UadpNetworkMessage(UADPNetworkMessageDiscoveryType.DataSetMetaData) {
                DataSetWriterIds = dataSetWriterIds,
                PublisherId = udpConnection_.PubSubConnectionConfiguration.PublisherId.Value,
            };

            var bytes = discoveryRequestMetaDataMessage.Encode(MessageContext);

            // send the DataSetMetaData DiscoveryRequest message to all open UDPClient 
            foreach (var udpClient in discoveryUdpClients_)
            {
                try
                {
                    Utils.Trace("UdpDiscoverySubscriber.SendDiscoveryRequestDataSetMetaData Before sending message for DataSetWriterIds:{0}",
                        String.Join(", ", dataSetWriterIds));

                    udpClient.Send(bytes, bytes.Length, DiscoveryNetworkAddressEndPoint);
                }
                catch (Exception ex)
                {
                    Utils.Trace(ex, "UdpDiscoverySubscriber.SendDiscoveryRequestDataSetMetaData");
                }
            }

            // double the time between requests
            intervalRunner_.Interval = intervalRunner_.Interval * 2;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Decide if there is anything to publish
        /// </summary>
        /// <returns></returns>
        private bool CanPublish()
        {
            lock (lock_)
            {
                if (metadataWriterIdsToSend_.Count == 0)
                {
                    // reset the interval for publisher if there is nothing to send
                    intervalRunner_.Interval = kInitialRequestInterval;
                }

                return metadataWriterIdsToSend_.Count > 0;
            }
        }

        /// <summary>
        /// Joint task to request discovery messages
        /// </summary>
        private void RequestDiscoveryMessages()
        {
            SendDiscoveryRequestDataSetMetaData();
            SendDiscoveryRequestDataSetWriterConfiguration();
        }
        #endregion
    }
}
