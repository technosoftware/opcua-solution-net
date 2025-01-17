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
using System.Threading;

using Opc.Ua;

using Technosoftware.UaPubSub;
#endregion

namespace SampleCompany.SamplePublisher
{
    class PublishedValuesWrites : IDisposable
    {
        #region Constants
        // It should match the namespace index from configuration file
        public const ushort NamespaceIndexSimple = 2;
        public const ushort NamespaceIndexAllTypes = 3;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pubSubApplication"></param>
        public PublishedValuesWrites(UaPubSubApplication uaPubSubApplication)
        {
            publishedDataSets_ = uaPubSubApplication.UaPubSubConfigurator.PubSubConfiguration.PublishedDataSets;
            dataStore_ = uaPubSubApplication.DataStore;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            updateValuesTimer_.Dispose();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize PublisherData with information from configuration and start timer to update data 
        /// </summary>
        public void Start()
        {
            if (publishedDataSets_ != null)
            {
                // Remember the fields to be updated 
                foreach (var publishedDataSet in publishedDataSets_)
                {
                    switch (publishedDataSet.Name)
                    {
                        case DataSetNameSimple:
                            simpleFields_.AddRange(publishedDataSet.DataSetMetaData.Fields);
                            break;
                        case DataSetNameAllTypes:
                            allTypesFields_.AddRange(publishedDataSet.DataSetMetaData.Fields);
                            break;
                    }
                }
            }

            try
            {
                LoadInitialData();
            }
            catch (Exception e)
            {
                Utils.Trace(Utils.TraceMasks.Error, "LoadInitialData wrong field: {0}", e.StackTrace);
            }

            updateValuesTimer_ = new Timer(UpdateValues, null, 1000, 1000);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load initial demo data
        /// </summary>
        private void LoadInitialData()
        {
            #region DataSet 'Simple' fill with data
            WriteFieldData("BoolToggle", NamespaceIndexSimple, new DataValue(new Variant(false), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Int32", NamespaceIndexSimple, new DataValue(new Variant(0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Int32Fast", NamespaceIndexSimple, new DataValue(new Variant(0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("DateTime", NamespaceIndexSimple, new DataValue(new Variant(DateTime.UtcNow), StatusCodes.Good, DateTime.UtcNow));
            #endregion

            #region DataSet 'AllTypes' fill with data
            WriteFieldData("BoolToggle", NamespaceIndexAllTypes, new DataValue(new Variant(true), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Byte", NamespaceIndexAllTypes, new DataValue(new Variant((byte)0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Int16", NamespaceIndexAllTypes, new DataValue(new Variant((Int16)0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Int32", NamespaceIndexAllTypes, new DataValue(new Variant(0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("SByte", NamespaceIndexAllTypes, new DataValue(new Variant((sbyte)0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("UInt16", NamespaceIndexAllTypes, new DataValue(new Variant((UInt16)0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("UInt32", NamespaceIndexAllTypes, new DataValue(new Variant((UInt32)0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("UInt64", NamespaceIndexAllTypes, new DataValue(new Variant((UInt64)0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Float", NamespaceIndexAllTypes, new DataValue(new Variant((float)0F), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Double", NamespaceIndexAllTypes, new DataValue(new Variant((double)0.0), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("String", NamespaceIndexAllTypes, new DataValue(new Variant(aviationAlphabet_[0]), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("ByteString", NamespaceIndexAllTypes, new DataValue(new Variant(new byte[] { 1, 2, 3 }), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("Guid", NamespaceIndexAllTypes, new DataValue(new Variant(Guid.NewGuid()), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("DateTime", NamespaceIndexAllTypes, new DataValue(new Variant(DateTime.UtcNow), StatusCodes.Good, DateTime.UtcNow));
            WriteFieldData("UInt32Array", NamespaceIndexAllTypes, new DataValue(new Variant(new UInt32[] { 1, 2, 3 }), StatusCodes.Good, DateTime.UtcNow));
            #endregion
        }

        /// <summary>
        /// Write (update) field data
        /// </summary>
        /// <param name="metaDatafieldName"></param>
        /// <param name="dataValue"></param>
        private void WriteFieldData(string metaDatafieldName, ushort namespaceIndex, DataValue dataValue)
        {
            dataStore_.WritePublishedDataItem(new NodeId(metaDatafieldName, namespaceIndex), Attributes.Value, dataValue);
        }

        /// <summary>
        /// Simulate value changes in dynamic nodes
        /// </summary>
        /// <param name="state"></param>
        private void UpdateValues(object state)
        {
            try
            {
                lock (lock_)
                {
                    foreach (var variable in simpleFields_)
                    {
                        switch (variable.Name)
                        {
                            case "BoolToggle":
                                boolToogleCount_++;
                                if (boolToogleCount_ >= BoolToogleLimit)
                                {
                                    boolToogleCount_ = 0;
                                    IncrementValue(variable, NamespaceIndexSimple);
                                }
                                break;
                            case "Int32":
                                IncrementValue(variable, NamespaceIndexSimple, SimpleInt32Limit);
                                break;
                            case "Int32Fast":
                                IncrementValue(variable, NamespaceIndexSimple, SimpleInt32Limit, 100);
                                break;
                            case "DateTime":
                                IncrementValue(variable, NamespaceIndexSimple);
                                break;
                        }
                    }

                    foreach (var variable in allTypesFields_)
                    {
                        IncrementValue(variable, NamespaceIndexAllTypes);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected error doing simulation.");
            }
        }

        /// <summary>
        /// Increment value
        /// maxAllowedValue - maximum incremented value before reset value to initial value
        /// step - the increment value
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="namespaceIndex"></param>
        /// <param name="maxAllowedValue"></param>
        /// <param name="step"></param>
        private void IncrementValue(FieldMetaData variable, ushort namespaceIndex, long maxAllowedValue = Int32.MaxValue, int step = 0)
        {
            // Read value to be incremented
            var dataValue = dataStore_.ReadPublishedDataItem(new NodeId(variable.Name, namespaceIndex), Attributes.Value);
            if (dataValue.Value == null)
            {
                return;
            }

            var valueUpdated = false;

            var expectedType = TypeInfo.GetBuiltInType(variable.DataType);
            switch (expectedType)
            {
                case BuiltInType.Boolean:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var boolValue = Convert.ToBoolean(dataValue.Value);
                        dataValue.Value = !boolValue;
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.Byte:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var byteValue = Convert.ToByte(dataValue.Value);
                        dataValue.Value = (byte)(byteValue + 1);
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.Int16:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var int16Value = Convert.ToInt16(dataValue.Value);
                        int intIdentifier = int16Value;
                        Interlocked.CompareExchange(ref intIdentifier, 0, Int16.MaxValue);
                        dataValue.Value = (Int16)Interlocked.Increment(ref intIdentifier);
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.Int32:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var int32Value = Convert.ToInt32(dataValue.Value);
                        if (step > 0)
                        {
                            int32Value += (step - 1);
                        }
                        if (int32Value > maxAllowedValue)
                        {
                            int32Value = 0;
                        }
                        dataValue.Value = Interlocked.Increment(ref int32Value);
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.SByte:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var sbyteValue = Convert.ToSByte(dataValue.Value);
                        int intIdentifier = sbyteValue;
                        Interlocked.CompareExchange(ref intIdentifier, 0, SByte.MaxValue);
                        dataValue.Value = (SByte)Interlocked.Increment(ref intIdentifier);
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.UInt16:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var uint16Value = Convert.ToUInt16(dataValue.Value);
                        int intIdentifier = uint16Value;
                        Interlocked.CompareExchange(ref intIdentifier, 0, UInt16.MaxValue);
                        dataValue.Value = (UInt16)Interlocked.Increment(ref intIdentifier);
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.UInt32:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var uint32Value = Convert.ToUInt32(dataValue.Value);
                        long longIdentifier = uint32Value;
                        Interlocked.CompareExchange(ref longIdentifier, 0, UInt32.MaxValue);
                        dataValue.Value = (UInt32)Interlocked.Increment(ref longIdentifier);
                        valueUpdated = true;
                    }
                    else if (variable.ValueRank == ValueRanks.OneDimension)
                    {
                        var values = dataValue.Value as uint[];
                        if (values != null)
                        {
                            for (var i = 0; i < values.Length; i++)
                            {
                                var uint32Value = values[i];
                                long longIdentifier = uint32Value;
                                Interlocked.CompareExchange(ref longIdentifier, 0, UInt32.MaxValue);
                                values[i] = (UInt32)Interlocked.Increment(ref longIdentifier);
                            }
                            valueUpdated = true;
                        }
                    }
                    break;
                case BuiltInType.UInt64:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var uint64Value = Convert.ToUInt64(dataValue.Value);
                        float longIdentifier = uint64Value + 1;
                        Interlocked.CompareExchange(ref longIdentifier, 0, UInt64.MaxValue);
                        dataValue.Value = (UInt64)longIdentifier;
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.Float:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var floatValue = Convert.ToSingle(dataValue.Value);
                        Interlocked.CompareExchange(ref floatValue, 0, float.MaxValue);
                        dataValue.Value = floatValue + 1;
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.Double:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        var doubleValue = Convert.ToDouble(dataValue.Value);
                        Interlocked.CompareExchange(ref doubleValue, 0, double.MaxValue);
                        dataValue.Value = doubleValue + 1;
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.DateTime:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        dataValue.Value = DateTime.UtcNow;
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.Guid:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        dataValue.Value = Guid.NewGuid();
                        valueUpdated = true;
                    }
                    break;
                case BuiltInType.String:
                    if (variable.ValueRank == ValueRanks.Scalar)
                    {
                        aviationAlphabetIndex_ = (aviationAlphabetIndex_ + 1) % aviationAlphabet_.Length;
                        dataValue.Value = aviationAlphabet_[aviationAlphabetIndex_];
                        valueUpdated = true;
                    }
                    break;
            }

            if (valueUpdated)
            {
                // Save new updated value to data store
                WriteFieldData(variable.Name, namespaceIndex, dataValue);
            }
        }
        #endregion

        #region Private Fields
        private const string DataSetNameSimple = "Simple";
        private const string DataSetNameAllTypes = "AllTypes";

        // simulate for BoolToogle changes to 3 seconds
        private int boolToogleCount_;
        private const int BoolToogleLimit = 2;
        private const int SimpleInt32Limit = 10000;

        private FieldMetaDataCollection simpleFields_ = new FieldMetaDataCollection();
        private FieldMetaDataCollection allTypesFields_ = new FieldMetaDataCollection();

        private PublishedDataSetDataTypeCollection publishedDataSets_;
        private IUaPubSubDataStore dataStore_;
        private Timer updateValuesTimer_;
        string[] aviationAlphabet_ = new string[] {
            "Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India",
            "Juliet", "Kilo", "Lima", "Mike",  "November",  "Oscar", "Papa", "Quebec", "Romeo",
            "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "X-Ray", "Yankee", "Zulu"
        };
        int aviationAlphabetIndex_;
        private object lock_ = new object();
        #endregion

    }
}
