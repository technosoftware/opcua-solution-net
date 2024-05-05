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

using Opc.Ua;
#endregion

namespace Technosoftware.UaPubSub.Transport
{
    /// <summary>
    /// Entity responsible to trigger DataSetMetaData messages as configured for a <see cref="DataSetWriterDataType"/>.
    /// </summary>
    public class MqttMetadataPublisher
    {
        #region Private Fields
        private readonly IMqttPubSubConnection parentConnection_;
        private readonly WriterGroupDataType writerGroup_;
        private readonly DataSetWriterDataType dataSetWriter_;

        // the component that triggers the publish messages
        private readonly IntervalRunner intervalRunner_;
        #endregion

        #region Constructor
        /// <summary>
        /// Create new instance of <see cref="MqttMetadataPublisher"/>.
        /// </summary>
        internal MqttMetadataPublisher(IMqttPubSubConnection parentConnection, WriterGroupDataType writerGroup, DataSetWriterDataType dataSetWriter,
            double metaDataUpdateTime)
        {
            parentConnection_ = parentConnection;
            writerGroup_ = writerGroup;
            dataSetWriter_ = dataSetWriter;
            intervalRunner_ = new IntervalRunner(dataSetWriter.DataSetWriterId, metaDataUpdateTime, CanPublish, PublishMessage);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the publisher and makes it ready to send data.
        /// </summary>
        public void Start()
        {
            intervalRunner_.Start();
            Utils.Trace("The MqttMetadataPublisher for DataSetWriterId '{0}' was started.",
                dataSetWriter_.DataSetWriterId);
        }

        /// <summary>
        /// Stop the publishing thread.
        /// </summary>
        public virtual void Stop()
        {
            intervalRunner_.Stop();

            Utils.Trace("The MqttMetadataPublisher for DataSetWriterId '{0}' was stopped.",
                dataSetWriter_.DataSetWriterId);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Decide if the DataSetWriter can publish metadata
        /// </summary>
        /// <returns></returns>
        private bool CanPublish()
        {
            return parentConnection_.CanPublishMetaData(writerGroup_, dataSetWriter_);
        }

        /// <summary>
        /// Generate and publish the dataset MetaData message
        /// </summary>
        private void PublishMessage()
        {
            try
            {
                UaNetworkMessage metaDataNetworkMessage = parentConnection_.CreateDataSetMetaDataNetworkMessage(writerGroup_, dataSetWriter_);
                if (metaDataNetworkMessage != null)
                {
                    var success = parentConnection_.PublishNetworkMessage(metaDataNetworkMessage);
                    Utils.Trace(
                        "MqttMetadataPublisher Publish DataSetMetaData, DataSetWriterId:{0}; success = {1}",
                        dataSetWriter_.DataSetWriterId, success);
                }
            }
            catch (Exception e)
            {
                // Unexpected exception in PublishMessages
                Utils.Trace(e, "MqttMetadataPublisher.PublishMessages");
            }
        }
        #endregion

        /// <summary>
        /// Holds state of MetaData
        /// </summary>
        public class MetaDataState
        {
            #region Constructor

            /// <summary>
            /// Create new instance of <see cref="MetaDataState"/>
            /// </summary>
            /// <param name="dataSetWriter"></param>
            public MetaDataState(DataSetWriterDataType dataSetWriter)
            {
                DataSetWriter = dataSetWriter;
                LastSendTime = DateTime.MinValue;

                var transport =
                    ExtensionObject.ToEncodeable(DataSetWriter.TransportSettings)
                        as BrokerDataSetWriterTransportDataType;

                MetaDataUpdateTime = transport?.MetaDataUpdateTime ?? 0;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The DataSetWriter associated with this MetadataState object
            /// </summary>
            public DataSetWriterDataType DataSetWriter { get; set; }

            /// <summary>
            /// Holds the last metadata that was sent
            /// </summary>
            public DataSetMetaDataType LastMetaData { get; set; }

            /// <summary>
            /// Holds the Utc DateTime for the last metadata sent
            /// </summary>
            public DateTime LastSendTime { get; set; }

            /// <summary>
            /// The configured interval when the metadata shall be sent
            /// </summary>
            public double MetaDataUpdateTime { get; set; }

            #endregion

            /// <summary>
            /// Get the next publish interval
            /// </summary>
            /// <returns></returns>
            public double GetNextPublishInterval()
            {
                return Math.Max(0, MetaDataUpdateTime - DateTime.UtcNow.Subtract(LastSendTime).TotalMilliseconds);
            }
        }
    }
}
