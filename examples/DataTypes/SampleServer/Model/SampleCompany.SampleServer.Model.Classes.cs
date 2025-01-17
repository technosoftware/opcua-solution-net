/* ========================================================================
 * Copyright (c) 2005-2021 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using Opc.Ua;

namespace SampleCompany.SampleServer.Model
{
    #region GetMachineDataMethodState Class
    #if (!OPCUA_EXCLUDE_GetMachineDataMethodState)
    /// <remarks />
    /// <exclude />
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Opc.Ua.ModelCompiler", "1.0.0.0")]
    public partial class GetMachineDataMethodState : MethodState
    {
        #region Constructors
        /// <remarks />
        public GetMachineDataMethodState(NodeState parent) : base(parent)
        {
        }

        /// <remarks />
        public new static NodeState Construct(NodeState parent)
        {
            return new GetMachineDataMethodState(parent);
        }

        #if (!OPCUA_EXCLUDE_InitializationStrings)
        /// <remarks />
        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            Initialize(context, InitializationString);
            InitializeOptionalChildren(context);
        }

        /// <remarks />
        protected override void InitializeOptionalChildren(ISystemContext context)
        {
            base.InitializeOptionalChildren(context);
        }

        #region Initialization String
        private const string InitializationString =
           "AQAAACsAAABodHRwOi8vc2FtcGxlY29tcGFueS5jb20vU2FtcGxlU2VydmVyL01vZGVs/////wRhggoE" +
           "AAAAAQAYAAAAR2V0TWFjaGluZURhdGFNZXRob2RUeXBlAQEEAAAvAQEEAAQAAAABAf////8CAAAAF2Cp" +
           "CgIAAAAAAA4AAABJbnB1dEFyZ3VtZW50cwEBBQAALgBEBQAAAJYBAAAAAQAqAQEaAAAACwAAAE1hY2hp" +
           "bmVOYW1lAAz/////AAAAAAABACgBAQAAAAEAAAAAAAAAAQH/////AAAAABdgqQoCAAAAAAAPAAAAT3V0" +
           "cHV0QXJndW1lbnRzAQEGAAAuAEQGAAAAlgEAAAABACoBARwAAAALAAAATWFjaGluZURhdGEBAQMA////" +
           "/wAAAAAAAQAoAQEAAAABAAAAAAAAAAEB/////wAAAAA=";
        #endregion
        #endif
        #endregion

        #region Event Callbacks
        /// <remarks />
        public GetMachineDataMethodStateMethodCallHandler OnCall;
        #endregion

        #region Public Properties
        #endregion

        #region Overridden Methods
        /// <remarks />
        protected override ServiceResult Call(
            ISystemContext _context,
            NodeId _objectId,
            IList<object> _inputArguments,
            IList<object> _outputArguments)
        {
            if (OnCall == null)
            {
                return base.Call(_context, _objectId, _inputArguments, _outputArguments);
            }

            ServiceResult _result = null;

            string machineName = (string)_inputArguments[0];

            MachineDataType machineData = (MachineDataType)_outputArguments[0];

            if (OnCall != null)
            {
                _result = OnCall(
                    _context,
                    this,
                    _objectId,
                    machineName,
                    ref machineData);
            }

            _outputArguments[0] = machineData;

            return _result;
        }
        #endregion

        #region Private Fields
        #endregion
    }

    /// <remarks />
    /// <exclude />
    public delegate ServiceResult GetMachineDataMethodStateMethodCallHandler(
        ISystemContext _context,
        MethodState _method,
        NodeId _objectId,
        string machineName,
        ref MachineDataType machineData);
    #endif
    #endregion

    #region GenericControllerState Class
    #if (!OPCUA_EXCLUDE_GenericControllerState)
    /// <remarks />
    /// <exclude />
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Opc.Ua.ModelCompiler", "1.0.0.0")]
    public partial class GenericControllerState : BaseObjectState
    {
        #region Constructors
        /// <remarks />
        public GenericControllerState(NodeState parent) : base(parent)
        {
        }

        /// <remarks />
        protected override NodeId GetDefaultTypeDefinitionId(NamespaceTable namespaceUris)
        {
            return Opc.Ua.NodeId.Create(SampleCompany.SampleServer.Model.ObjectTypes.GenericControllerType, SampleCompany.SampleServer.Model.Namespaces.SampleServer, namespaceUris);
        }

        #if (!OPCUA_EXCLUDE_InitializationStrings)
        /// <remarks />
        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            Initialize(context, InitializationString);
            InitializeOptionalChildren(context);
        }

        /// <remarks />
        protected override void Initialize(ISystemContext context, NodeState source)
        {
            InitializeOptionalChildren(context);
            base.Initialize(context, source);
        }

        /// <remarks />
        protected override void InitializeOptionalChildren(ISystemContext context)
        {
            base.InitializeOptionalChildren(context);
        }

        #region Initialization String
        private const string InitializationString =
           "AQAAACsAAABodHRwOi8vc2FtcGxlY29tcGFueS5jb20vU2FtcGxlU2VydmVyL01vZGVs/////wRggAIB" +
           "AAAAAQAdAAAAR2VuZXJpY0NvbnRyb2xsZXJUeXBlSW5zdGFuY2UBAQcAAQEHAAcAAAD/////AgAAABVg" +
           "iQoCAAAAAQAIAAAAU2V0UG9pbnQBAQgAAC8BAEAJCAAAAAAL/////wMD/////wEAAAAVYIkKAgAAAAAA" +
           "BwAAAEVVUmFuZ2UBAQwAAC4ARAwAAAABAHQD/////wEB/////wAAAAAVYIkKAgAAAAEACwAAAE1lYXN1" +
           "cmVtZW50AQEOAAAvAQBACQ4AAAAAC/////8BAf////8BAAAAFWCJCgIAAAAAAAcAAABFVVJhbmdlAQES" +
           "AAAuAEQSAAAAAQB0A/////8BAf////8AAAAA";
        #endregion
        #endif
        #endregion

        #region Public Properties
        /// <remarks />
        public AnalogItemState<double> SetPoint
        {
            get
            {
                return m_setPoint;
            }

            set
            {
                if (!Object.ReferenceEquals(m_setPoint, value))
                {
                    ChangeMasks |= NodeStateChangeMasks.Children;
                }

                m_setPoint = value;
            }
        }

        /// <remarks />
        public AnalogItemState<double> Measurement
        {
            get
            {
                return m_measurement;
            }

            set
            {
                if (!Object.ReferenceEquals(m_measurement, value))
                {
                    ChangeMasks |= NodeStateChangeMasks.Children;
                }

                m_measurement = value;
            }
        }
        #endregion

        #region Overridden Methods
        /// <remarks />
        public override void GetChildren(
            ISystemContext context,
            IList<BaseInstanceState> children)
        {
            if (m_setPoint != null)
            {
                children.Add(m_setPoint);
            }

            if (m_measurement != null)
            {
                children.Add(m_measurement);
            }

            base.GetChildren(context, children);
        }
            
        /// <remarks />
        protected override BaseInstanceState FindChild(
            ISystemContext context,
            QualifiedName browseName,
            bool createOrReplace,
            BaseInstanceState replacement)
        {
            if (QualifiedName.IsNull(browseName))
            {
                return null;
            }

            BaseInstanceState instance = null;

            switch (browseName.Name)
            {
                case SampleCompany.SampleServer.Model.BrowseNames.SetPoint:
                {
                    if (createOrReplace)
                    {
                        if (SetPoint == null)
                        {
                            if (replacement == null)
                            {
                                SetPoint = new AnalogItemState<double>(this);
                            }
                            else
                            {
                                SetPoint = (AnalogItemState<double>)replacement;
                            }
                        }
                    }

                    instance = SetPoint;
                    break;
                }

                case SampleCompany.SampleServer.Model.BrowseNames.Measurement:
                {
                    if (createOrReplace)
                    {
                        if (Measurement == null)
                        {
                            if (replacement == null)
                            {
                                Measurement = new AnalogItemState<double>(this);
                            }
                            else
                            {
                                Measurement = (AnalogItemState<double>)replacement;
                            }
                        }
                    }

                    instance = Measurement;
                    break;
                }
            }

            if (instance != null)
            {
                return instance;
            }

            return base.FindChild(context, browseName, createOrReplace, replacement);
        }
        #endregion

        #region Private Fields
        private AnalogItemState<double> m_setPoint;
        private AnalogItemState<double> m_measurement;
        #endregion
    }
    #endif
    #endregion

    #region FlowControllerState Class
    #if (!OPCUA_EXCLUDE_FlowControllerState)
    /// <remarks />
    /// <exclude />
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Opc.Ua.ModelCompiler", "1.0.0.0")]
    public partial class FlowControllerState : GenericControllerState
    {
        #region Constructors
        /// <remarks />
        public FlowControllerState(NodeState parent) : base(parent)
        {
        }

        /// <remarks />
        protected override NodeId GetDefaultTypeDefinitionId(NamespaceTable namespaceUris)
        {
            return Opc.Ua.NodeId.Create(SampleCompany.SampleServer.Model.ObjectTypes.FlowControllerType, SampleCompany.SampleServer.Model.Namespaces.SampleServer, namespaceUris);
        }

        #if (!OPCUA_EXCLUDE_InitializationStrings)
        /// <remarks />
        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            Initialize(context, InitializationString);
            InitializeOptionalChildren(context);
        }

        /// <remarks />
        protected override void Initialize(ISystemContext context, NodeState source)
        {
            InitializeOptionalChildren(context);
            base.Initialize(context, source);
        }

        /// <remarks />
        protected override void InitializeOptionalChildren(ISystemContext context)
        {
            base.InitializeOptionalChildren(context);
        }

        #region Initialization String
        private const string InitializationString =
           "AQAAACsAAABodHRwOi8vc2FtcGxlY29tcGFueS5jb20vU2FtcGxlU2VydmVyL01vZGVs/////wRggAIB" +
           "AAAAAQAaAAAARmxvd0NvbnRyb2xsZXJUeXBlSW5zdGFuY2UBARQAAQEUABQAAAD/////AgAAABVgiQoC" +
           "AAAAAQAIAAAAU2V0UG9pbnQBARUAAC8BAEAJFQAAAAAL/////wMD/////wEAAAAVYIkKAgAAAAAABwAA" +
           "AEVVUmFuZ2UBARkAAC4ARBkAAAABAHQD/////wEB/////wAAAAAVYIkKAgAAAAEACwAAAE1lYXN1cmVt" +
           "ZW50AQEbAAAvAQBACRsAAAAAC/////8BAf////8BAAAAFWCJCgIAAAAAAAcAAABFVVJhbmdlAQEfAAAu" +
           "AEQfAAAAAQB0A/////8BAf////8AAAAA";
        #endregion
        #endif
        #endregion

        #region Public Properties
        #endregion

        #region Overridden Methods
        #endregion

        #region Private Fields
        #endregion
    }
    #endif
    #endregion

    #region LevelControllerState Class
    #if (!OPCUA_EXCLUDE_LevelControllerState)
    /// <remarks />
    /// <exclude />
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Opc.Ua.ModelCompiler", "1.0.0.0")]
    public partial class LevelControllerState : GenericControllerState
    {
        #region Constructors
        /// <remarks />
        public LevelControllerState(NodeState parent) : base(parent)
        {
        }

        /// <remarks />
        protected override NodeId GetDefaultTypeDefinitionId(NamespaceTable namespaceUris)
        {
            return Opc.Ua.NodeId.Create(SampleCompany.SampleServer.Model.ObjectTypes.LevelControllerType, SampleCompany.SampleServer.Model.Namespaces.SampleServer, namespaceUris);
        }

        #if (!OPCUA_EXCLUDE_InitializationStrings)
        /// <remarks />
        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            Initialize(context, InitializationString);
            InitializeOptionalChildren(context);
        }

        /// <remarks />
        protected override void Initialize(ISystemContext context, NodeState source)
        {
            InitializeOptionalChildren(context);
            base.Initialize(context, source);
        }

        /// <remarks />
        protected override void InitializeOptionalChildren(ISystemContext context)
        {
            base.InitializeOptionalChildren(context);
        }

        #region Initialization String
        private const string InitializationString =
           "AQAAACsAAABodHRwOi8vc2FtcGxlY29tcGFueS5jb20vU2FtcGxlU2VydmVyL01vZGVs/////wRggAIB" +
           "AAAAAQAbAAAATGV2ZWxDb250cm9sbGVyVHlwZUluc3RhbmNlAQEhAAEBIQAhAAAA/////wIAAAAVYIkK" +
           "AgAAAAEACAAAAFNldFBvaW50AQEiAAAvAQBACSIAAAAAC/////8DA/////8BAAAAFWCJCgIAAAAAAAcA" +
           "AABFVVJhbmdlAQEmAAAuAEQmAAAAAQB0A/////8BAf////8AAAAAFWCJCgIAAAABAAsAAABNZWFzdXJl" +
           "bWVudAEBKAAALwEAQAkoAAAAAAv/////AQH/////AQAAABVgiQoCAAAAAAAHAAAARVVSYW5nZQEBLAAA" +
           "LgBELAAAAAEAdAP/////AQH/////AAAAAA==";
        #endregion
        #endif
        #endregion

        #region Public Properties
        #endregion

        #region Overridden Methods
        #endregion

        #region Private Fields
        #endregion
    }
    #endif
    #endregion

    #region TemperatureControllerState Class
    #if (!OPCUA_EXCLUDE_TemperatureControllerState)
    /// <remarks />
    /// <exclude />
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Opc.Ua.ModelCompiler", "1.0.0.0")]
    public partial class TemperatureControllerState : GenericControllerState
    {
        #region Constructors
        /// <remarks />
        public TemperatureControllerState(NodeState parent) : base(parent)
        {
        }

        /// <remarks />
        protected override NodeId GetDefaultTypeDefinitionId(NamespaceTable namespaceUris)
        {
            return Opc.Ua.NodeId.Create(SampleCompany.SampleServer.Model.ObjectTypes.TemperatureControllerType, SampleCompany.SampleServer.Model.Namespaces.SampleServer, namespaceUris);
        }

        #if (!OPCUA_EXCLUDE_InitializationStrings)
        /// <remarks />
        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            Initialize(context, InitializationString);
            InitializeOptionalChildren(context);
        }

        /// <remarks />
        protected override void Initialize(ISystemContext context, NodeState source)
        {
            InitializeOptionalChildren(context);
            base.Initialize(context, source);
        }

        /// <remarks />
        protected override void InitializeOptionalChildren(ISystemContext context)
        {
            base.InitializeOptionalChildren(context);
        }

        #region Initialization String
        private const string InitializationString =
           "AQAAACsAAABodHRwOi8vc2FtcGxlY29tcGFueS5jb20vU2FtcGxlU2VydmVyL01vZGVs/////wRggAIB" +
           "AAAAAQAhAAAAVGVtcGVyYXR1cmVDb250cm9sbGVyVHlwZUluc3RhbmNlAQEuAAEBLgAuAAAA/////wIA" +
           "AAAVYIkKAgAAAAEACAAAAFNldFBvaW50AQEvAAAvAQBACS8AAAAAC/////8DA/////8BAAAAFWCJCgIA" +
           "AAAAAAcAAABFVVJhbmdlAQEzAAAuAEQzAAAAAQB0A/////8BAf////8AAAAAFWCJCgIAAAABAAsAAABN" +
           "ZWFzdXJlbWVudAEBNQAALwEAQAk1AAAAAAv/////AQH/////AQAAABVgiQoCAAAAAAAHAAAARVVSYW5n" +
           "ZQEBOQAALgBEOQAAAAEAdAP/////AQH/////AAAAAA==";
        #endregion
        #endif
        #endregion

        #region Public Properties
        #endregion

        #region Overridden Methods
        #endregion

        #region Private Fields
        #endregion
    }
    #endif
    #endregion

    #region MachineState Class
    #if (!OPCUA_EXCLUDE_MachineState)
    /// <remarks />
    /// <exclude />
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Opc.Ua.ModelCompiler", "1.0.0.0")]
    public partial class MachineState : BaseObjectState
    {
        #region Constructors
        /// <remarks />
        public MachineState(NodeState parent) : base(parent)
        {
        }

        /// <remarks />
        protected override NodeId GetDefaultTypeDefinitionId(NamespaceTable namespaceUris)
        {
            return Opc.Ua.NodeId.Create(SampleCompany.SampleServer.Model.ObjectTypes.MachineType, SampleCompany.SampleServer.Model.Namespaces.SampleServer, namespaceUris);
        }

        #if (!OPCUA_EXCLUDE_InitializationStrings)
        /// <remarks />
        protected override void Initialize(ISystemContext context)
        {
            base.Initialize(context);
            Initialize(context, InitializationString);
            InitializeOptionalChildren(context);
        }

        /// <remarks />
        protected override void Initialize(ISystemContext context, NodeState source)
        {
            InitializeOptionalChildren(context);
            base.Initialize(context, source);
        }

        /// <remarks />
        protected override void InitializeOptionalChildren(ISystemContext context)
        {
            base.InitializeOptionalChildren(context);
        }

        #region Initialization String
        private const string InitializationString =
           "AQAAACsAAABodHRwOi8vc2FtcGxlY29tcGFueS5jb20vU2FtcGxlU2VydmVyL01vZGVs/////wRggAIB" +
           "AAAAAQATAAAATWFjaGluZVR5cGVJbnN0YW5jZQEBOwABATsAOwAAAP////8EAAAABGCACgEAAAABAAsA" +
           "AABUZW1wZXJhdHVyZQEBPAAALwEBLgA8AAAA/////wIAAAAVYIkKAgAAAAEACAAAAFNldFBvaW50AQE9" +
           "AAAvAQBACT0AAAAAC/////8DA/////8BAAAAFWCJCgIAAAAAAAcAAABFVVJhbmdlAQFBAAAuAERBAAAA" +
           "AQB0A/////8BAf////8AAAAAFWCJCgIAAAABAAsAAABNZWFzdXJlbWVudAEBQwAALwEAQAlDAAAAAAv/" +
           "////AQH/////AQAAABVgiQoCAAAAAAAHAAAARVVSYW5nZQEBRwAALgBERwAAAAEAdAP/////AQH/////" +
           "AAAAAARggAoBAAAAAQAEAAAARmxvdwEBSQAALwEBFABJAAAA/////wIAAAAVYIkKAgAAAAEACAAAAFNl" +
           "dFBvaW50AQFKAAAvAQBACUoAAAAAC/////8DA/////8BAAAAFWCJCgIAAAAAAAcAAABFVVJhbmdlAQFO" +
           "AAAuAEROAAAAAQB0A/////8BAf////8AAAAAFWCJCgIAAAABAAsAAABNZWFzdXJlbWVudAEBUAAALwEA" +
           "QAlQAAAAAAv/////AQH/////AQAAABVgiQoCAAAAAAAHAAAARVVSYW5nZQEBVAAALgBEVAAAAAEAdAP/" +
           "////AQH/////AAAAAARggAoBAAAAAQAFAAAATGV2ZWwBAVYAAC8BASEAVgAAAP////8CAAAAFWCJCgIA" +
           "AAABAAgAAABTZXRQb2ludAEBVwAALwEAQAlXAAAAAAv/////AwP/////AQAAABVgiQoCAAAAAAAHAAAA" +
           "RVVSYW5nZQEBWwAALgBEWwAAAAEAdAP/////AQH/////AAAAABVgiQoCAAAAAQALAAAATWVhc3VyZW1l" +
           "bnQBAV0AAC8BAEAJXQAAAAAL/////wEB/////wEAAAAVYIkKAgAAAAAABwAAAEVVUmFuZ2UBAWEAAC4A" +
           "RGEAAAABAHQD/////wEB/////wAAAAAVYIkKAgAAAAEACwAAAE1hY2hpbmVEYXRhAQFjAAAuAERjAAAA" +
           "AQEDAP////8DA/////8AAAAA";
        #endregion
        #endif
        #endregion

        #region Public Properties
        /// <remarks />
        public TemperatureControllerState Temperature
        {
            get
            {
                return m_temperature;
            }

            set
            {
                if (!Object.ReferenceEquals(m_temperature, value))
                {
                    ChangeMasks |= NodeStateChangeMasks.Children;
                }

                m_temperature = value;
            }
        }

        /// <remarks />
        public FlowControllerState Flow
        {
            get
            {
                return m_flow;
            }

            set
            {
                if (!Object.ReferenceEquals(m_flow, value))
                {
                    ChangeMasks |= NodeStateChangeMasks.Children;
                }

                m_flow = value;
            }
        }

        /// <remarks />
        public LevelControllerState Level
        {
            get
            {
                return m_level;
            }

            set
            {
                if (!Object.ReferenceEquals(m_level, value))
                {
                    ChangeMasks |= NodeStateChangeMasks.Children;
                }

                m_level = value;
            }
        }

        /// <remarks />
        public PropertyState<MachineDataType> MachineData
        {
            get
            {
                return m_machineData;
            }

            set
            {
                if (!Object.ReferenceEquals(m_machineData, value))
                {
                    ChangeMasks |= NodeStateChangeMasks.Children;
                }

                m_machineData = value;
            }
        }
        #endregion

        #region Overridden Methods
        /// <remarks />
        public override void GetChildren(
            ISystemContext context,
            IList<BaseInstanceState> children)
        {
            if (m_temperature != null)
            {
                children.Add(m_temperature);
            }

            if (m_flow != null)
            {
                children.Add(m_flow);
            }

            if (m_level != null)
            {
                children.Add(m_level);
            }

            if (m_machineData != null)
            {
                children.Add(m_machineData);
            }

            base.GetChildren(context, children);
        }
            
        /// <remarks />
        protected override BaseInstanceState FindChild(
            ISystemContext context,
            QualifiedName browseName,
            bool createOrReplace,
            BaseInstanceState replacement)
        {
            if (QualifiedName.IsNull(browseName))
            {
                return null;
            }

            BaseInstanceState instance = null;

            switch (browseName.Name)
            {
                case SampleCompany.SampleServer.Model.BrowseNames.Temperature:
                {
                    if (createOrReplace)
                    {
                        if (Temperature == null)
                        {
                            if (replacement == null)
                            {
                                Temperature = new TemperatureControllerState(this);
                            }
                            else
                            {
                                Temperature = (TemperatureControllerState)replacement;
                            }
                        }
                    }

                    instance = Temperature;
                    break;
                }

                case SampleCompany.SampleServer.Model.BrowseNames.Flow:
                {
                    if (createOrReplace)
                    {
                        if (Flow == null)
                        {
                            if (replacement == null)
                            {
                                Flow = new FlowControllerState(this);
                            }
                            else
                            {
                                Flow = (FlowControllerState)replacement;
                            }
                        }
                    }

                    instance = Flow;
                    break;
                }

                case SampleCompany.SampleServer.Model.BrowseNames.Level:
                {
                    if (createOrReplace)
                    {
                        if (Level == null)
                        {
                            if (replacement == null)
                            {
                                Level = new LevelControllerState(this);
                            }
                            else
                            {
                                Level = (LevelControllerState)replacement;
                            }
                        }
                    }

                    instance = Level;
                    break;
                }

                case SampleCompany.SampleServer.Model.BrowseNames.MachineData:
                {
                    if (createOrReplace)
                    {
                        if (MachineData == null)
                        {
                            if (replacement == null)
                            {
                                MachineData = new PropertyState<MachineDataType>(this);
                            }
                            else
                            {
                                MachineData = (PropertyState<MachineDataType>)replacement;
                            }
                        }
                    }

                    instance = MachineData;
                    break;
                }
            }

            if (instance != null)
            {
                return instance;
            }

            return base.FindChild(context, browseName, createOrReplace, replacement);
        }
        #endregion

        #region Private Fields
        private TemperatureControllerState m_temperature;
        private FlowControllerState m_flow;
        private LevelControllerState m_level;
        private PropertyState<MachineDataType> m_machineData;
        #endregion
    }
    #endif
    #endregion
}