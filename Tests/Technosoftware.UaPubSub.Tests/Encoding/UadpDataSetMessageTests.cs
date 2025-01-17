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
using System.Linq;
using System.IO;

using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

using Opc.Ua;
using Technosoftware.UaPubSub.PublishedData;
using Technosoftware.UaPubSub.Encoding;
#endregion

namespace Technosoftware.UaPubSub.Tests.Encoding
{
    [TestFixture(Description = "Tests for Encoding/Decoding of UadpDataSeMessage objects")]
    public class UadpDataSetMessageTests
    {
        private string publisherConfigurationFileName_ = Path.Combine("Configuration", "PublisherConfiguration.xml");
        private string subscriberConfigurationFileName_ = Path.Combine("Configuration", "SubscriberConfiguration.xml");

        private PubSubConfigurationDataType publisherConfiguration_;
        private UaPubSubApplication publisherApplication_;
        private WriterGroupDataType firstWriterGroup_;
        private IUaPubSubConnection firstPublisherConnection_;

        private PubSubConfigurationDataType subscriberConfiguration_;
        private UaPubSubApplication subscriberApplication_;
        private ReaderGroupDataType firstReaderGroup_;
        private DataSetReaderDataType firstDataSetReaderType_;

        private const ushort kNamespaceIndexSimple = 2;

        /// <summary>
        /// just for test match the DataSet1->DataSetWriterId
        /// </summary>
        private const ushort kTestDataSetWriterId = 1;
        private const ushort kMessageContentMask = 0x3f;

        [OneTimeSetUp()]
        public void MyTestInitialize()
        {
            // Create a publisher application
            // todo refactor to use the MessagesHelper create configuration
            string publisherConfigurationFile = Utils.GetAbsoluteFilePath(publisherConfigurationFileName_, true, true, false);
            publisherApplication_ = UaPubSubApplication.Create(publisherConfigurationFile);
            Assert.IsNotNull(publisherApplication_, "publisherApplication_ should not be null");

            // Get the publisher configuration
            publisherConfiguration_ = publisherApplication_.UaPubSubConfigurator.PubSubConfiguration;
            Assert.IsNotNull(publisherConfiguration_, "publisherConfiguration_ should not be null");

            // Get first connection
            Assert.IsNotNull(publisherConfiguration_.Connections, "publisherConfiguration_.Connections should not be null");
            Assert.IsNotEmpty(publisherConfiguration_.Connections, "publisherConfiguration_.Connections should not be empty");
            firstPublisherConnection_ = publisherApplication_.PubSubConnections[0];
            Assert.IsNotNull(firstPublisherConnection_, "firstPublisherConnection_ should not be null");

            // Read the first writer group
            Assert.IsNotEmpty(publisherConfiguration_.Connections[0].WriterGroups, "pubSubConfigConnection.WriterGroups should not be empty");
            firstWriterGroup_ = publisherConfiguration_.Connections[0].WriterGroups[0];
            Assert.IsNotNull(firstWriterGroup_, "firstWriterGroup_ should not be null");

            Assert.IsNotNull(publisherConfiguration_.PublishedDataSets, "publisherConfiguration_.PublishedDataSets should not be null");
            Assert.IsNotEmpty(publisherConfiguration_.PublishedDataSets, "publisherConfiguration_.PublishedDataSets should not be empty");

            // Create a subscriber application
            string subscriberConfigurationFile = Utils.GetAbsoluteFilePath(subscriberConfigurationFileName_, true, true, false);
            subscriberApplication_ = UaPubSubApplication.Create(subscriberConfigurationFile);
            Assert.IsNotNull(subscriberApplication_, "subscriberApplication_ should not be null");

            // Get the subscriber configuration
            subscriberConfiguration_ = subscriberApplication_.UaPubSubConfigurator.PubSubConfiguration;
            Assert.IsNotNull(subscriberConfiguration_, "subscriberConfiguration_ should not be null");

            // Read the first reader group
            firstReaderGroup_ = subscriberConfiguration_.Connections[0].ReaderGroups[0];
            Assert.IsNotNull(firstWriterGroup_, "firstReaderGroup_ should not be null");

            firstDataSetReaderType_ = GetFirstDataSetReader();
        }

        [Test(Description = "Validate dataset message mask with Variant data type;" +
                            "Change the Uadp dataset message mask into the [0,63] range that covers all options(properties)")]
        public void ValidateDataSetMessageMask(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            // change network message mask
            for (uint dataSetMessageContentMask = 0; dataSetMessageContentMask < kMessageContentMask; dataSetMessageContentMask++)
            {
                uadpDataSetMessage.SetMessageContentMask((UadpDataSetMessageContentMask)dataSetMessageContentMask);

                // Assert
                CompareEncodeDecode(uadpDataSetMessage);
            }
        }

        [Test(Description = "Validate TimeStamp")]
        public void ValidateDataSetTimeStamp(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.Timestamp);
            uadpDataSetMessage.Timestamp = DateTime.UtcNow;

            // Assert
            CompareEncodeDecode(uadpDataSetMessage);
        }

        [Test(Description = "Validate PicoSeconds")]
        public void ValidatePicoSeconds(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.PicoSeconds);
            uadpDataSetMessage.PicoSeconds = 10;

            // Assert
            CompareEncodeDecode(uadpDataSetMessage);
        }

        [Test(Description = "Validate Status")]
        public void ValidateStatus(
            [Values(UadpDataSetMessageContentMask.None, UadpDataSetMessageContentMask.Timestamp,
                UadpDataSetMessageContentMask.MajorVersion, UadpDataSetMessageContentMask.MinorVersion,
                UadpDataSetMessageContentMask.SequenceNumber,
                UadpDataSetMessageContentMask.MajorVersion | UadpDataSetMessageContentMask.MinorVersion,
                UadpDataSetMessageContentMask.MajorVersion | UadpDataSetMessageContentMask.MinorVersion | UadpDataSetMessageContentMask.SequenceNumber)]
            UadpDataSetMessageContentMask messageContentMask,
            [Values(StatusCodes.Good, StatusCodes.UncertainDataSubNormal, StatusCodes.BadAggregateListMismatch, StatusCodes.BadUnknownResponse,
            StatusCodes.Bad, StatusCodes.BadAggregateConfigurationRejected, StatusCodes.BadAggregateInvalidInputs, StatusCodes.BadAlreadyExists)]
            uint statusCode
            )
        {
            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(DataSetFieldContentMask.None);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(messageContentMask | UadpDataSetMessageContentMask.Status);
            uadpDataSetMessage.Status = statusCode;

            // Assert
            CompareEncodeDecode(uadpDataSetMessage);
        }

        [Test(Description = "Validate MajorVersion and MinorVersion with Equal values")]
        public void ValidateMajorVersionEqMinorVersionEq(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            const int versionValue = 2;

            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.MajorVersion | UadpDataSetMessageContentMask.MinorVersion);
            uadpDataSetMessage.MetaDataVersion.MajorVersion = versionValue;
            uadpDataSetMessage.MetaDataVersion.MinorVersion = versionValue * 10;

            IServiceMessageContext messageContextEncode = new ServiceMessageContext();
            byte[] bytes;
            var memoryStream = new MemoryStream();
            using (BinaryEncoder encoder = new BinaryEncoder(memoryStream, messageContextEncode, true))
            {
                uadpDataSetMessage.Encode(encoder);
                _ = encoder.Close();
                bytes = ReadBytes(memoryStream);
            }

            UadpDataSetMessage uaDataSetMessageDecoded = new UadpDataSetMessage();
            BinaryDecoder decoder = new BinaryDecoder(bytes, messageContextEncode);

            // Make sure the reader MajorVersion and MinorVersion are the same with the ones on the dataset message
            DataSetReaderDataType reader = (DataSetReaderDataType)firstDataSetReaderType_.MemberwiseClone();
            reader.DataSetMetaData.ConfigurationVersion.MajorVersion = versionValue;
            reader.DataSetMetaData.ConfigurationVersion.MinorVersion = versionValue * 10;

            // workaround
            uaDataSetMessageDecoded.DataSetWriterId = kTestDataSetWriterId;
            uaDataSetMessageDecoded.DecodePossibleDataSetReader(decoder, reader);
            decoder.Dispose();

            // Assert
            Assert.AreEqual(DataSetDecodeErrorReason.NoError, uaDataSetMessageDecoded.DecodeErrorReason);
            Assert.AreEqual(false, uaDataSetMessageDecoded.IsMetadataMajorVersionChange);
            Assert.AreNotEqual(null, uaDataSetMessageDecoded.DataSet);
            // compare uadpDataSetMessage with uaDataSetMessageDecoded
            CompareUadpDataSetMessages(uadpDataSetMessage, uaDataSetMessageDecoded);
        }

        [Test(Description = "Validate MajorVersion equal and MinorVersion differ")]
        public void ValidateMajorVersionEqMinorVersionDiffer(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            const int VersionValue = 2;

            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.MajorVersion | UadpDataSetMessageContentMask.MinorVersion);
            uadpDataSetMessage.MetaDataVersion.MajorVersion = VersionValue;
            uadpDataSetMessage.MetaDataVersion.MinorVersion = VersionValue * 10;

            IServiceMessageContext messageContextEncode = new ServiceMessageContext();
            byte[] bytes;
            var memoryStream = new MemoryStream();
            using (BinaryEncoder encoder = new BinaryEncoder(memoryStream, messageContextEncode, true))
            {
                uadpDataSetMessage.Encode(encoder);
                _ = encoder.Close();
                bytes = ReadBytes(memoryStream);
            }

            UadpDataSetMessage uaDataSetMessageDecoded = new UadpDataSetMessage();
            BinaryDecoder decoder = new BinaryDecoder(bytes, messageContextEncode);

            // Make sure the reader MajorVersion is same with the ones on the dataset message
            // and MinorVersion differ
            DataSetReaderDataType reader = (DataSetReaderDataType)firstDataSetReaderType_.MemberwiseClone();
            reader.DataSetMetaData.ConfigurationVersion.MajorVersion = uadpDataSetMessage.MetaDataVersion.MajorVersion;
            reader.DataSetMetaData.ConfigurationVersion.MinorVersion = uadpDataSetMessage.MetaDataVersion.MinorVersion + 1;

            // workaround
            uaDataSetMessageDecoded.DataSetWriterId = kTestDataSetWriterId;
            uaDataSetMessageDecoded.DecodePossibleDataSetReader(decoder, reader);
            decoder.Dispose();

            // Assert
            Assert.AreEqual(DataSetDecodeErrorReason.NoError, uaDataSetMessageDecoded.DecodeErrorReason);
            Assert.AreEqual(false, uaDataSetMessageDecoded.IsMetadataMajorVersionChange);
            Assert.AreNotEqual(null, uaDataSetMessageDecoded.DataSet);
            // compare uadpDataSetMessage with uaDataSetMessageDecoded
            CompareUadpDataSetMessages(uadpDataSetMessage, uaDataSetMessageDecoded);
        }

        [Test(Description = "Validate MajorVersion differ and MinorVersion are equal")]
        public void ValidateMajorVersionDiffMinorVersionEq(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            const int VersionValue = 2;

            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.MajorVersion | UadpDataSetMessageContentMask.MinorVersion);
            uadpDataSetMessage.MetaDataVersion.MajorVersion = VersionValue;
            uadpDataSetMessage.MetaDataVersion.MinorVersion = VersionValue * 10;

            IServiceMessageContext messageContextEncode = new ServiceMessageContext();
            byte[] bytes;
            var memoryStream = new MemoryStream();
            using (BinaryEncoder encoder = new BinaryEncoder(memoryStream, messageContextEncode, true))
            {
                uadpDataSetMessage.Encode(encoder);
                _ = encoder.Close();
                bytes = ReadBytes(memoryStream);
            }

            UadpDataSetMessage uaDataSetMessageDecoded = new UadpDataSetMessage();
            BinaryDecoder decoder = new BinaryDecoder(bytes, messageContextEncode);

            // Make sure the reader MajorVersion differ and MinorVersion are equal
            DataSetReaderDataType reader = (DataSetReaderDataType)firstDataSetReaderType_.MemberwiseClone();
            reader.DataSetMetaData.ConfigurationVersion.MajorVersion = uadpDataSetMessage.MetaDataVersion.MajorVersion + 1;
            reader.DataSetMetaData.ConfigurationVersion.MinorVersion = uadpDataSetMessage.MetaDataVersion.MinorVersion;

            // workaround
            uaDataSetMessageDecoded.DataSetWriterId = kTestDataSetWriterId;
            uaDataSetMessageDecoded.DecodePossibleDataSetReader(decoder, reader);
            decoder.Dispose();

            // Assert
            Assert.AreEqual(DataSetDecodeErrorReason.MetadataMajorVersion, uaDataSetMessageDecoded.DecodeErrorReason);
            Assert.AreEqual(true, uaDataSetMessageDecoded.IsMetadataMajorVersionChange);
            Assert.AreEqual(null, uaDataSetMessageDecoded.DataSet);
        }

        [Test(Description = "Validate MajorVersion differ and MinorVersion differ")]
        public void ValidateMajorVersionDiffMinorVersionDiff(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            const int versionValue = 2;

            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.MajorVersion | UadpDataSetMessageContentMask.MinorVersion);
            uadpDataSetMessage.MetaDataVersion.MajorVersion = versionValue;
            uadpDataSetMessage.MetaDataVersion.MinorVersion = versionValue * 10;

            IServiceMessageContext messageContextEncode = new ServiceMessageContext();
            byte[] bytes;
            var memoryStream = new MemoryStream();
            using (BinaryEncoder encoder = new BinaryEncoder(memoryStream, messageContextEncode, true))
            {
                uadpDataSetMessage.Encode(encoder);
                _ = encoder.Close();
                bytes = ReadBytes(memoryStream);
            }

            UadpDataSetMessage uaDataSetMessageDecoded = new UadpDataSetMessage();
            BinaryDecoder decoder = new BinaryDecoder(bytes, messageContextEncode);

            // Make sure the reader MajorVersion differ and MinorVersion differ
            DataSetReaderDataType reader = (DataSetReaderDataType)firstDataSetReaderType_.MemberwiseClone();
            reader.DataSetMetaData.ConfigurationVersion.MajorVersion = uadpDataSetMessage.MetaDataVersion.MajorVersion + 1;
            reader.DataSetMetaData.ConfigurationVersion.MinorVersion = uadpDataSetMessage.MetaDataVersion.MinorVersion + 1;

            // workaround
            uaDataSetMessageDecoded.DataSetWriterId = kTestDataSetWriterId;
            uaDataSetMessageDecoded.DecodePossibleDataSetReader(decoder, reader);
            decoder.Dispose();

            // Assert
            Assert.AreEqual(DataSetDecodeErrorReason.MetadataMajorVersion, uaDataSetMessageDecoded.DecodeErrorReason);
            Assert.AreEqual(true, uaDataSetMessageDecoded.IsMetadataMajorVersionChange);
            Assert.AreEqual(null, uaDataSetMessageDecoded.DataSet);
        }

        [Test(Description = "Validate SequenceNumber")]
        public void ValidateSequenceNumber(
            [Values(DataSetFieldContentMask.None, DataSetFieldContentMask.RawData, // list here all possible DataSetFieldContentMask
            DataSetFieldContentMask.ServerPicoSeconds, DataSetFieldContentMask.ServerTimestamp, DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.SourceTimestamp, DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.StatusCode,
            DataSetFieldContentMask.ServerPicoSeconds| DataSetFieldContentMask.ServerTimestamp| DataSetFieldContentMask.SourcePicoSeconds| DataSetFieldContentMask.SourceTimestamp| DataSetFieldContentMask.StatusCode
            )]
            DataSetFieldContentMask dataSetFieldContentMask)
        {
            // Arrange
            UadpDataSetMessage uadpDataSetMessage = GetFirstDataSetMessage(dataSetFieldContentMask);

            // Act  
            uadpDataSetMessage.SetMessageContentMask(UadpDataSetMessageContentMask.SequenceNumber);
            uadpDataSetMessage.SequenceNumber = 1000;

            // Assert
            CompareEncodeDecode(uadpDataSetMessage);
        }

        #region Private Methods        

        /// <summary>
        /// Load Variant data type into datasets
        /// </summary>
        private void LoadData()
        {
            Assert.IsNotNull(publisherApplication_, "publisherApplication_ should not be null");

            #region DataSet Simple
            // DataSet 'Simple' fill with data
            DataValue booleanValue = new DataValue(new Variant(true), StatusCodes.Good);
            publisherApplication_.DataStore.WritePublishedDataItem(new NodeId("BoolToggle", kNamespaceIndexSimple), Attributes.Value, booleanValue);
            DataValue scalarInt32XValue = new DataValue(new Variant(100), StatusCodes.Good);
            publisherApplication_.DataStore.WritePublishedDataItem(new NodeId("Int32", kNamespaceIndexSimple), Attributes.Value, scalarInt32XValue);
            DataValue scalarInt32YValue = new DataValue(new Variant(50), StatusCodes.Good);
            publisherApplication_.DataStore.WritePublishedDataItem(new NodeId("Int32Fast", kNamespaceIndexSimple), Attributes.Value, scalarInt32YValue);
            DataValue dateTimeValue = new DataValue(new Variant(DateTime.UtcNow), StatusCodes.Good);
            publisherApplication_.DataStore.WritePublishedDataItem(new NodeId("DateTime", kNamespaceIndexSimple), Attributes.Value, dateTimeValue);
            #endregion
        }

        /// <summary>
        /// Get first DataSetReaders from configuration
        /// </summary>
        /// <returns></returns>
        private DataSetReaderDataType GetFirstDataSetReader()
        {
            // Read the first configured ReaderGroup
            Assert.IsNotNull(firstReaderGroup_, "firstReaderGroup_ should not be null");
            Assert.IsNotEmpty(firstReaderGroup_.DataSetReaders, "firstReaderGroup_.DataSetReaders should not be empty");
            Assert.IsNotNull(firstReaderGroup_.DataSetReaders[0], "firstReaderGroup_.DataSetReaders[0] should not be null");

            return firstReaderGroup_.DataSetReaders[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldContentMask"> a DataSetFieldContentMask specifying what type of encoding is chosen for field values
        /// If none of the flags are set, the fields are represented as Variant.
        /// If the RawData flag is set, the fields are represented as RawData and all other bits are ignored.
        /// If one of the bits StatusCode, SourceTimestamp, ServerTimestamp, SourcePicoSeconds, ServerPicoSeconds is set, 
        ///    the fields are represented as DataValue.
        /// </param>
        /// <returns></returns>
        private UadpDataSetMessage GetFirstDataSetMessage(DataSetFieldContentMask fieldContentMask)
        {
            LoadData();

            // set the configurable field content mask to allow only Variant data type
            foreach (DataSetWriterDataType dataSetWriter in firstWriterGroup_.DataSetWriters)
            {
                // 00 The DataSet fields are encoded as Variant data type
                // The Variant can contain a StatusCode instead of the expected DataType if the status of the field is Bad.
                // The Variant can contain a DataValue with the value and the statusCode if the status of the field is Uncertain.
                dataSetWriter.DataSetFieldContentMask = (uint)fieldContentMask;
            }

            var networkMessages = firstPublisherConnection_.CreateNetworkMessages(firstWriterGroup_, new WriterGroupPublishState());
            // filter out the metadata message
            networkMessages = (from m in networkMessages
                               where !m.IsMetaDataMessage
                               select m).ToList();
            Assert.IsNotNull(networkMessages, "connection.CreateNetworkMessages shall not return null");
            Assert.AreEqual(1, networkMessages.Count, "connection.CreateNetworkMessages shall return only one network message");

            UadpNetworkMessage uaNetworkMessage = networkMessages[0] as UadpNetworkMessage;

            Assert.IsNotNull(uaNetworkMessage, "networkMessageEncode should not be null");

            // read first dataset message
            UaDataSetMessage[] uadpDataSetMessages = uaNetworkMessage.DataSetMessages.ToArray();
            Assert.IsNotEmpty(uadpDataSetMessages, "uadpDataSetMessages collection should not be empty");

            UaDataSetMessage uadpDataSetMessage = uadpDataSetMessages[0];
            Assert.IsNotNull(uadpDataSetMessage, "uadpDataSetMessage should not be null");

            return uadpDataSetMessage as UadpDataSetMessage;
        }

        /// <summary>
        /// Compare encoded/decoded dataset messages
        /// </summary>
        /// <param name="uadpDataSetMessage"></param>
        /// <returns></returns>
        private void CompareEncodeDecode(UadpDataSetMessage uadpDataSetMessage)
        {
            IServiceMessageContext messageContextEncode = new ServiceMessageContext();
            byte[] bytes;
            var memoryStream = new MemoryStream();
            using (BinaryEncoder encoder = new BinaryEncoder(memoryStream, messageContextEncode, true))
            {
                uadpDataSetMessage.Encode(encoder);
                _ = encoder.Close();
                bytes = ReadBytes(memoryStream);
            }

            UadpDataSetMessage uaDataSetMessageDecoded = new UadpDataSetMessage();
            BinaryDecoder decoder = new BinaryDecoder(bytes, messageContextEncode);

            // workaround
            uaDataSetMessageDecoded.DataSetWriterId = kTestDataSetWriterId;
            uaDataSetMessageDecoded.DecodePossibleDataSetReader(decoder, firstDataSetReaderType_);
            decoder.Dispose();

            // compare uadpDataSetMessage with uaDataSetMessageDecoded
            CompareUadpDataSetMessages(uadpDataSetMessage, uaDataSetMessageDecoded);
        }


        /// <summary>
        /// Compare dataset messages options 
        /// </summary>
        /// <param name="uadpDataSetMessageEncode"></param>
        /// <param name="uadpDataSetMessageDecoded"></param>
        /// <returns></returns>
        private void CompareUadpDataSetMessages(UadpDataSetMessage uadpDataSetMessageEncode, UadpDataSetMessage uadpDataSetMessageDecoded)
        {
            DataSet dataSetDecoded = uadpDataSetMessageDecoded.DataSet;
            UadpDataSetMessageContentMask dataSetMessageContentMask = uadpDataSetMessageEncode.DataSetMessageContentMask;

            Assert.AreEqual(uadpDataSetMessageEncode.DataSetFlags1, uadpDataSetMessageDecoded.DataSetFlags1,
                    "DataSetMessages DataSetFlags1 do not match:");
            Assert.AreEqual(uadpDataSetMessageEncode.DataSetFlags2, uadpDataSetMessageDecoded.DataSetFlags2,
                   "DataSetMessages DataSetFlags2 do not match:");

            if ((dataSetMessageContentMask & UadpDataSetMessageContentMask.Timestamp) ==
                UadpDataSetMessageContentMask.Timestamp)
            {
                Assert.AreEqual(uadpDataSetMessageEncode.Timestamp, uadpDataSetMessageDecoded.Timestamp,
                    "DataSetMessages TimeStamp do not match:");
            }

            if ((dataSetMessageContentMask & UadpDataSetMessageContentMask.PicoSeconds) ==
                UadpDataSetMessageContentMask.PicoSeconds)
            {
                Assert.AreEqual(uadpDataSetMessageEncode.PicoSeconds, uadpDataSetMessageDecoded.PicoSeconds,
                    "DataSetMessages PicoSeconds do not match:");
            }

            if ((dataSetMessageContentMask & UadpDataSetMessageContentMask.Status) ==
                UadpDataSetMessageContentMask.Status)
            {
                Assert.AreEqual(uadpDataSetMessageEncode.Status, uadpDataSetMessageDecoded.Status,
                    "DataSetMessages Status do not match:");
            }

            if ((dataSetMessageContentMask & UadpDataSetMessageContentMask.MajorVersion) ==
                UadpDataSetMessageContentMask.MajorVersion)
            {
                Assert.AreEqual(uadpDataSetMessageEncode.MetaDataVersion.MajorVersion, uadpDataSetMessageDecoded.MetaDataVersion.MajorVersion,
                    "DataSetMessages ConfigurationMajorVersion do not match:");
            }

            if ((dataSetMessageContentMask & UadpDataSetMessageContentMask.MinorVersion) ==
                UadpDataSetMessageContentMask.MinorVersion)
            {
                Assert.AreEqual(uadpDataSetMessageEncode.MetaDataVersion.MinorVersion, uadpDataSetMessageDecoded.MetaDataVersion.MinorVersion,
                    "DataSetMessages ConfigurationMajorVersion do not match:");
            }

            // check also the payload data
            Assert.AreEqual(uadpDataSetMessageEncode.DataSet.Fields.Length, dataSetDecoded.Fields.Length,
                "DataSetMessages DataSet fields size do not match:");

            for (int index = 0; index < uadpDataSetMessageEncode.DataSet.Fields.Length; index++)
            {
                Field dataSetFieldEncoded = uadpDataSetMessageEncode.DataSet.Fields[index];
                Field dataSetFieldDecoded = dataSetDecoded.Fields[index];

                Assert.IsNotNull(dataSetFieldEncoded.Value, "DataSetFieldEncoded.Value is null");
                Assert.IsNotNull(dataSetFieldDecoded.Value, "DataSetFieldDecoded.Value is null");
                object encodedValue = dataSetFieldEncoded.Value.Value;
                object decodedValue = dataSetFieldDecoded.Value.Value;

                Assert.AreEqual(encodedValue, decodedValue,
                    "DataSetMessages Field.Value does not match value field at position: {0} {1}|{2}", index, encodedValue, decodedValue);
            }
        }

        /// <summary>
        /// Read All bytes from a given stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private byte[] ReadBytes(Stream stream)
        {
            stream.Position = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
        #endregion
    }
}
