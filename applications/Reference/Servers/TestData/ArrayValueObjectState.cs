#region Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Reflection;

using Opc.Ua;
#endregion

namespace TestData
{
    public partial class ArrayValueObjectState
    {
        #region Initialization
        /// <summary>
        /// Initializes the object as a collection of counters which change value on read.
        /// </summary>
        protected override void OnAfterCreate(ISystemContext context, NodeState node)
        {
            base.OnAfterCreate(context, node);

            InitializeVariable(context, BooleanValue, TestData.Variables.ArrayValueObjectType_BooleanValue);
            InitializeVariable(context, SByteValue, TestData.Variables.ArrayValueObjectType_SByteValue);
            InitializeVariable(context, ByteValue, TestData.Variables.ArrayValueObjectType_ByteValue);
            InitializeVariable(context, Int16Value, TestData.Variables.ArrayValueObjectType_Int16Value);
            InitializeVariable(context, UInt16Value, TestData.Variables.ArrayValueObjectType_UInt16Value);
            InitializeVariable(context, Int32Value, TestData.Variables.ArrayValueObjectType_Int32Value);
            InitializeVariable(context, UInt32Value, TestData.Variables.ArrayValueObjectType_UInt32Value);
            InitializeVariable(context, Int64Value, TestData.Variables.ArrayValueObjectType_Int64Value);
            InitializeVariable(context, UInt64Value, TestData.Variables.ArrayValueObjectType_UInt64Value);
            InitializeVariable(context, FloatValue, TestData.Variables.ArrayValueObjectType_FloatValue);
            InitializeVariable(context, DoubleValue, TestData.Variables.ArrayValueObjectType_DoubleValue);
            InitializeVariable(context, StringValue, TestData.Variables.ArrayValueObjectType_StringValue);
            InitializeVariable(context, DateTimeValue, TestData.Variables.ArrayValueObjectType_DateTimeValue);
            InitializeVariable(context, GuidValue, TestData.Variables.ArrayValueObjectType_GuidValue);
            InitializeVariable(context, ByteStringValue, TestData.Variables.ArrayValueObjectType_ByteStringValue);
            InitializeVariable(context, XmlElementValue, TestData.Variables.ArrayValueObjectType_XmlElementValue);
            InitializeVariable(context, NodeIdValue, TestData.Variables.ArrayValueObjectType_NodeIdValue);
            InitializeVariable(context, ExpandedNodeIdValue, TestData.Variables.ArrayValueObjectType_ExpandedNodeIdValue);
            InitializeVariable(context, QualifiedNameValue, TestData.Variables.ArrayValueObjectType_QualifiedNameValue);
            InitializeVariable(context, LocalizedTextValue, TestData.Variables.ArrayValueObjectType_LocalizedTextValue);
            InitializeVariable(context, StatusCodeValue, TestData.Variables.ArrayValueObjectType_StatusCodeValue);
            InitializeVariable(context, VariantValue, TestData.Variables.ArrayValueObjectType_VariantValue);
            InitializeVariable(context, EnumerationValue, TestData.Variables.ArrayValueObjectType_EnumerationValue);
            InitializeVariable(context, StructureValue, TestData.Variables.ArrayValueObjectType_StructureValue);
            InitializeVariable(context, NumberValue, TestData.Variables.ArrayValueObjectType_NumberValue);
            InitializeVariable(context, IntegerValue, TestData.Variables.ArrayValueObjectType_IntegerValue);
            InitializeVariable(context, UIntegerValue, TestData.Variables.ArrayValueObjectType_UIntegerValue);
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

            GenerateValue(system, BooleanValue);
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
            GenerateValue(system, StringValue);
            GenerateValue(system, DateTimeValue);
            GenerateValue(system, GuidValue);
            GenerateValue(system, ByteStringValue);
            GenerateValue(system, XmlElementValue);
            GenerateValue(system, NodeIdValue);
            GenerateValue(system, ExpandedNodeIdValue);
            GenerateValue(system, QualifiedNameValue);
            GenerateValue(system, LocalizedTextValue);
            GenerateValue(system, StatusCodeValue);
            GenerateValue(system, VariantValue);
            GenerateValue(system, EnumerationValue);
            GenerateValue(system, StructureValue);
            GenerateValue(system, NumberValue);
            GenerateValue(system, IntegerValue);
            GenerateValue(system, UIntegerValue);

            return base.OnGenerateValues(context, method, objectId, count);
        }
        #endregion
    }
}