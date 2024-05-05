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

using Technosoftware.UaPubSub.PublishedData;
#endregion

namespace Technosoftware.UaPubSub
{
    /// <summary>
    /// The publishing state for a writer group.
    /// </summary>
    public class WriterGroupPublishState
    {
        /// <summary>
        /// Hold the DataSet State
        /// </summary>
        private class DataSetState
        {
            public uint MessageCount;
            public DataSet LastDataSet;

            public ConfigurationVersionDataType ConfigurationVersion;
            public DateTime LastMetaDataUpdate;
        }

        /// <summary>
        /// The DataSetStates indexed by dataset writer group id.
        /// </summary>
        private readonly Dictionary<ushort, DataSetState> dataSetStates_;

        #region Constructor
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public WriterGroupPublishState()
        {
            dataSetStates_ = new Dictionary<ushort, DataSetState>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns TRUE if the next DataSetMessage is a delta frame.
        /// </summary>
        public bool IsDeltaFrame(DataSetWriterDataType writer, out uint sequenceNumber)
        {
            lock (dataSetStates_)
            {
                DataSetState state = GetState(writer);
                sequenceNumber = state.MessageCount + 1;

                if (state.MessageCount % writer.KeyFrameCount != 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns TRUE if the next DataSetMessage is a delta frame.
        /// </summary>
        public bool HasMetaDataChanged(DataSetWriterDataType writer, DataSetMetaDataType metadata)
        {
            if (metadata == null)
            {
                return false;
            }

            lock (dataSetStates_)
            {
                DataSetState state = GetState(writer);

                ConfigurationVersionDataType version = state.ConfigurationVersion;
                // no matter what the TransportSettings.MetaDataUpdateTime is the ConfigurationVersion is checked
                if (version == null)
                {
                    // keep a copy of ConfigurationVersion
                    state.ConfigurationVersion = metadata.ConfigurationVersion.Clone() as ConfigurationVersionDataType;
                    state.LastMetaDataUpdate = DateTime.UtcNow;
                    return true;
                }

                if (version.MajorVersion != metadata.ConfigurationVersion.MajorVersion ||
                    version.MinorVersion != metadata.ConfigurationVersion.MinorVersion)
                {
                    // keep a copy of ConfigurationVersion
                    state.ConfigurationVersion = metadata.ConfigurationVersion.Clone() as ConfigurationVersionDataType;
                    state.LastMetaDataUpdate = DateTime.UtcNow;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the DataSet has changed and null
        /// </summary>
        public DataSet ExcludeUnchangedFields(DataSetWriterDataType writer, DataSet dataset)
        {
            lock (dataSetStates_)
            {
                DataSetState state = GetState(writer);

                DataSet lastDataSet = state.LastDataSet;

                if (lastDataSet == null)
                {
                    state.LastDataSet = Utils.Clone(dataset) as DataSet;
                    return dataset;
                }

                var changed = false;

                for (var ii = 0; ii < dataset.Fields.Length && ii < lastDataSet.Fields.Length; ii++)
                {
                    var field1 = dataset.Fields[ii];
                    var field2 = lastDataSet.Fields[ii];

                    if (field1 == null || field2 == null)
                    {
                        changed = true;
                        continue;
                    }

                    if (field1.Value.StatusCode != field2.Value.StatusCode)
                    {
                        changed = true;
                        continue;
                    }

                    if (!Utils.IsEqual(field1.Value.WrappedValue, field2.Value.WrappedValue))
                    {
                        changed = true;
                        continue;
                    }

                    dataset.Fields[ii] = null;
                }

                if (!changed)
                {
                    return null;
                }
            }

            return dataset;
        }

        /// <summary>
        /// Increments the message counter.
        /// </summary>
        public void OnMessagePublished(DataSetWriterDataType writer, DataSet dataset)
        {
            lock (dataSetStates_)
            {
                DataSetState state = GetState(writer);
                state.MessageCount++;

                if (writer.KeyFrameCount > 1)
                {

                    state.ConfigurationVersion =
                        dataset.DataSetMetaData.ConfigurationVersion.Clone() as ConfigurationVersionDataType;

                    if (state.LastDataSet == null)
                    {
                        state.LastDataSet = Utils.Clone(dataset) as DataSet;
                        return;
                    }

                    for (var ii = 0; ii < dataset.Fields.Length && ii < state.LastDataSet.Fields.Length; ii++)
                    {
                        var field = dataset.Fields[ii];

                        if (field != null)
                        {
                            state.LastDataSet.Fields[ii] = Utils.Clone(field) as Field;
                        }
                    }

                }
            }
        }
        #endregion

        #region Private Methods


        private DataSetState GetState(DataSetWriterDataType writer)
        {
            DataSetState state;

            if (!dataSetStates_.TryGetValue(writer.DataSetWriterId, out state))
            {
                dataSetStates_[writer.DataSetWriterId] = state = new DataSetState();
            }

            return state;
        }
        #endregion
    }
}
