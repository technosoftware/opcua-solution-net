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

using Opc.Ua;
#endregion

namespace Technosoftware.UaPubSub.PublishedData
{
    /// <summary>
    /// Class specialized in collecting published data 
    /// </summary>
    public class DataCollector
    {
        #region Private Fields
        private readonly Dictionary<string, PublishedDataSetDataType> publishedDataSetsByName_;
        private readonly IUaPubSubDataStore dataStore_;
        #endregion

        #region Constructor
        /// <summary>
        /// Create new instance of <see cref="DataCollector"/>.
        /// </summary>
        /// <param name="dataStore">Reference to the <see cref="IUaPubSubDataStore"/> that will be used to collect data.</param>
        public DataCollector(IUaPubSubDataStore dataStore)
        {
            dataStore_ = dataStore;
            publishedDataSetsByName_ = new Dictionary<string, PublishedDataSetDataType>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validates a <see cref="PublishedDataSetDataType"/> configuration object.
        /// </summary>
        /// <param name="publishedDataSet">The <see cref="PublishedDataSetDataType"/> that is to be validated.</param>
        /// <returns>true if configuration is correct.</returns>
        public bool ValidatePublishedDataSet(PublishedDataSetDataType publishedDataSet)
        {
            if (publishedDataSet == null)
            {
                throw new ArgumentException(nameof(publishedDataSet));
            }
            if (publishedDataSet.DataSetMetaData == null)
            {
                Utils.Trace(Utils.TraceMasks.Error, "The DataSetMetaData field is null.");
                return false;
            }
            if (ExtensionObject.ToEncodeable(publishedDataSet.DataSetSource) is PublishedDataItemsDataType publishedDataItems && publishedDataItems.PublishedData != null)
            {
                if (publishedDataItems.PublishedData.Count != publishedDataSet.DataSetMetaData.Fields.Count)
                {
                    Utils.Trace(Utils.TraceMasks.Error, "The DataSetSource.Count is different from DataSetMetaData.Fields.Count.");
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Register a publishedDataSet
        /// </summary>
        /// <param name="publishedDataSet"></param>
        public void AddPublishedDataSet(PublishedDataSetDataType publishedDataSet)
        {
            if (publishedDataSet == null)
            {
                throw new ArgumentException(nameof(publishedDataSet));
            }
            // validate publishedDataSet
            if (ValidatePublishedDataSet(publishedDataSet))
            {
                publishedDataSetsByName_[publishedDataSet.Name] = publishedDataSet;
            }
            else
            {
                Utils.Trace(Utils.TraceMasks.Error, "The PublishedDataSet {0} was not registered because it is not configured properly.",
                    publishedDataSet.Name);
            }
        }

        /// <summary>
        /// Remove a registered a publishedDataSet
        /// </summary>
        /// <param name="publishedDataSet"></param>
        public void RemovePublishedDataSet(PublishedDataSetDataType publishedDataSet)
        {
            if (publishedDataSet == null)
            {
                throw new ArgumentException(nameof(publishedDataSet));
            }
            publishedDataSetsByName_.Remove(publishedDataSet.Name);
        }

        /// <summary>
        ///  Create and return a DataSet object created from its dataSetName
        /// </summary>
        /// <param name="dataSetName"></param>
        /// <returns></returns>
        public DataSet CollectData(string dataSetName)
        {
            PublishedDataSetDataType publishedDataSet = GetPublishedDataSet(dataSetName);

            if (publishedDataSet != null)
            {
                dataStore_.UpdateMetaData(publishedDataSet);

                if (publishedDataSet.DataSetSource != null)
                {
                    var dataSet = new DataSet(dataSetName);
                    dataSet.DataSetMetaData = publishedDataSet.DataSetMetaData;


                    if (ExtensionObject.ToEncodeable(publishedDataSet.DataSetSource) is PublishedDataItemsDataType publishedDataItems && publishedDataItems.PublishedData != null && publishedDataItems.PublishedData.Count > 0)
                    {
                        dataSet.Fields = new Field[publishedDataItems.PublishedData.Count];
                        for (var i = 0; i < publishedDataItems.PublishedData.Count; i++)
                        {
                            try
                            {
                                PublishedVariableDataType publishedVariable = publishedDataItems.PublishedData[i];
                                dataSet.Fields[i] = new Field();

                                // set FieldMetaData property
                                dataSet.Fields[i].FieldMetaData = publishedDataSet.DataSetMetaData.Fields[i];

                                // retrieve value from DataStore 
                                DataValue dataValue = null;

                                if (publishedVariable.PublishedVariable != null)
                                {
                                    dataValue = dataStore_.ReadPublishedDataItem(publishedVariable.PublishedVariable, publishedVariable.AttributeId);
                                }

                                if (dataValue == null)
                                {
                                    //try to get the dataValue from ExtensionFields
                                    /*If an entry of the PublishedData references one of the ExtensionFields, the substituteValue shall contain the 
                                    * QualifiedName of the ExtensionFields entry. 
                                    * All other fields of this PublishedVariableDataType array element shall be null*/
                                    var extensionFieldName = publishedVariable.SubstituteValue.Value as QualifiedName;
                                    if (extensionFieldName != null)
                                    {
                                        Opc.Ua.KeyValuePair extensionField = publishedDataSet.ExtensionFields.Find(x => x.Key == extensionFieldName);
                                        if (extensionField != null)
                                        {
                                            dataValue = new DataValue(extensionField.Value);
                                        }
                                    }
                                    if (dataValue == null)
                                    {
                                        dataValue = new DataValue(StatusCodes.Bad, DateTime.UtcNow);
                                    }
                                }
                                else
                                {
                                    dataValue = Utils.Clone(dataValue) as DataValue;

                                    //check StatusCode and return SubstituteValue if possible
                                    if (dataValue.StatusCode == StatusCodes.Bad && publishedVariable.SubstituteValue != Variant.Null)
                                    {
                                        dataValue.Value = publishedVariable.SubstituteValue.Value;
                                        dataValue.StatusCode = StatusCodes.UncertainSubstituteValue;
                                    }
                                }

                                dataValue.ServerTimestamp = DateTime.UtcNow;

                                #region FieldMetaData -> MaxStringLength size validation

                                Field field = dataSet.Fields[i];
                                Variant variant = dataValue.WrappedValue;

                                bool shouldBringToConstraints(uint givenStrlen)
                                {
                                    return field.FieldMetaData.MaxStringLength > 0 &&
                                        givenStrlen > field.FieldMetaData.MaxStringLength;
                                }

                                switch ((BuiltInType)field.FieldMetaData.BuiltInType)
                                {
                                    case BuiltInType.String:
                                        if (field.FieldMetaData.ValueRank == ValueRanks.Scalar)
                                        {
                                            if (variant.Value is string strFieldValue && shouldBringToConstraints((uint)strFieldValue.Length))
                                            {
                                                variant.Value = strFieldValue.Substring(0, (int)field.FieldMetaData.MaxStringLength);
                                                dataValue.Value = variant;
                                            }
                                        }
                                        else if (field.FieldMetaData.ValueRank == ValueRanks.OneDimension)
                                        {
                                            var valueArray = variant.Value as string[];
                                            if (valueArray != null)
                                            {
                                                for (var idx = 0; idx < valueArray.Length; idx++)
                                                {
                                                    if (shouldBringToConstraints((uint)valueArray[idx].Length))
                                                    {
                                                        valueArray[idx] = valueArray[idx].Substring(0, (int)field.FieldMetaData.MaxStringLength);
                                                    }
                                                }
                                            }
                                            dataValue.Value = valueArray;
                                        }
                                        break;
                                    case BuiltInType.ByteString:
                                        if (field.FieldMetaData.ValueRank == ValueRanks.Scalar)
                                        {
                                            if (variant.Value is byte[] byteStringFieldValue && shouldBringToConstraints((uint)byteStringFieldValue.Length))
                                            {
                                                var byteArray = (byte[])byteStringFieldValue.Clone();
                                                Array.Resize(ref byteArray, (int)field.FieldMetaData.MaxStringLength);
                                                variant.Value = byteArray;
                                                dataValue.Value = variant;
                                            }
                                        }
                                        else if (field.FieldMetaData.ValueRank == ValueRanks.OneDimension)
                                        {
                                            var valueArray = variant.Value as byte[][];
                                            if (valueArray != null)
                                            {
                                                for (var idx = 0; idx < valueArray.Length; idx++)
                                                {
                                                    if (shouldBringToConstraints((uint)valueArray[idx].Length))
                                                    {
                                                        var byteArray = (byte[])valueArray[idx].Clone();
                                                        Array.Resize(ref byteArray, (int)field.FieldMetaData.MaxStringLength);
                                                        valueArray[idx] = byteArray;
                                                    }
                                                }
                                            }
                                            dataValue.Value = valueArray;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                #endregion

                                dataSet.Fields[i].Value = dataValue;
                            }
                            catch (Exception ex)
                            {
                                dataSet.Fields[i].Value = new DataValue(StatusCodes.Bad, DateTime.UtcNow);
                                Utils.Trace(Utils.TraceMasks.Information, "DataCollector.CollectData for dataset {0} field {1} resulted in ex {2}",
                                    dataSetName, i, ex);
                            }
                        }
                        return dataSet;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get The <see cref="PublishedDataSetDataType"/> for a DataSetName
        /// </summary>
        /// <param name="dataSetName"></param>
        /// <returns></returns>
        public PublishedDataSetDataType GetPublishedDataSet(string dataSetName)
        {
            return dataSetName == null
                ? throw new ArgumentException(nameof(dataSetName))
                : publishedDataSetsByName_.TryGetValue(dataSetName, out PublishedDataSetDataType value) ? value : null;
        }

        #endregion
    }
}
