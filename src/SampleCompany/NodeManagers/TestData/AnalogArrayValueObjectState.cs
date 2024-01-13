#region Copyright (c) 2022-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2022-2023 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2022-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Reflection;

using Opc.Ua;
#endregion

namespace SampleCompany.NodeManagers.TestData
{
    public partial class AnalogArrayValueObjectState
    {
        #region Initialization
        /// <summary>
        /// Initializes the object as a collection of counters which change value on read.
        /// </summary>
        protected override void OnAfterCreate(ISystemContext context, NodeState node)
        {
            base.OnAfterCreate(context, node);

            InitializeVariable(context, SByteValue, TestData.Variables.AnalogArrayValueObjectType_SByteValue);
            InitializeVariable(context, ByteValue, TestData.Variables.AnalogArrayValueObjectType_ByteValue);
            InitializeVariable(context, Int16Value, TestData.Variables.AnalogArrayValueObjectType_Int16Value);
            InitializeVariable(context, UInt16Value, TestData.Variables.AnalogArrayValueObjectType_UInt16Value);
            InitializeVariable(context, Int32Value, TestData.Variables.AnalogArrayValueObjectType_Int32Value);
            InitializeVariable(context, UInt32Value, TestData.Variables.AnalogArrayValueObjectType_UInt32Value);
            InitializeVariable(context, Int64Value, TestData.Variables.AnalogArrayValueObjectType_Int64Value);
            InitializeVariable(context, UInt64Value, TestData.Variables.AnalogArrayValueObjectType_UInt64Value);
            InitializeVariable(context, FloatValue, TestData.Variables.AnalogArrayValueObjectType_FloatValue);
            InitializeVariable(context, DoubleValue, TestData.Variables.AnalogArrayValueObjectType_DoubleValue);
            InitializeVariable(context, NumberValue, TestData.Variables.AnalogArrayValueObjectType_NumberValue);
            InitializeVariable(context, IntegerValue, TestData.Variables.AnalogArrayValueObjectType_IntegerValue);
            InitializeVariable(context, UIntegerValue, TestData.Variables.AnalogArrayValueObjectType_UIntegerValue);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Handles the generate values method.
        /// </summary>
        protected override ServiceResult OnGenerateValues(
            ISystemContext context,
            MethodState method,
            NodeId objectId,
            uint count)
        {
            TestDataSystem system = context.SystemHandle as TestDataSystem;

            if (system == null)
            {
                return StatusCodes.BadOutOfService;
            }

            GenerateValue(system, SByteValue);
            GenerateValue(system, ByteValue);
            GenerateValue(system, Int16Value);
            GenerateValue(system, UInt16Value);
            GenerateValue(system, Int32Value);
            GenerateValue(system, UInt32Value);
            GenerateValue(system, UInt32Value);
            GenerateValue(system, Int64Value);
            GenerateValue(system, UInt64Value);
            GenerateValue(system, FloatValue);
            GenerateValue(system, DoubleValue);
            GenerateValue(system, NumberValue);
            GenerateValue(system, IntegerValue);
            GenerateValue(system, UIntegerValue);

            return base.OnGenerateValues(context, method, objectId, count);
        }
        #endregion
    }
}
