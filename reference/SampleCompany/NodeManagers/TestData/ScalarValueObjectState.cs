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
using System.Xml;
using System.IO;
using System.Reflection;
using Opc.Ua;
#endregion

namespace SampleCompany.NodeManagers.TestData
{
    public partial class ScalarValueObjectState
    {
        #region Initialization
        /// <summary>
        /// Initializes the object as a collection of counters which change value on read.
        /// </summary>
        protected override void OnAfterCreate(ISystemContext context, NodeState node)
        {
            base.OnAfterCreate(context, node);

            InitializeVariable(context, BooleanValue, Variables.ScalarValueObjectType_BooleanValue);
            InitializeVariable(context, SByteValue, Variables.ScalarValueObjectType_SByteValue);
            InitializeVariable(context, ByteValue, Variables.ScalarValueObjectType_ByteValue);
            InitializeVariable(context, Int16Value, Variables.ScalarValueObjectType_Int16Value);
            InitializeVariable(context, UInt16Value, Variables.ScalarValueObjectType_UInt16Value);
            InitializeVariable(context, Int32Value, Variables.ScalarValueObjectType_Int32Value);
            InitializeVariable(context, UInt32Value, Variables.ScalarValueObjectType_UInt32Value);
            InitializeVariable(context, Int64Value, Variables.ScalarValueObjectType_Int64Value);
            InitializeVariable(context, UInt64Value, Variables.ScalarValueObjectType_UInt64Value);
            InitializeVariable(context, FloatValue, Variables.ScalarValueObjectType_FloatValue);
            InitializeVariable(context, DoubleValue, Variables.ScalarValueObjectType_DoubleValue);
            InitializeVariable(context, StringValue, Variables.ScalarValueObjectType_StringValue);
            InitializeVariable(context, DateTimeValue, Variables.ScalarValueObjectType_DateTimeValue);
            InitializeVariable(context, GuidValue, Variables.ScalarValueObjectType_GuidValue);
            InitializeVariable(context, ByteStringValue, Variables.ScalarValueObjectType_ByteStringValue);
            InitializeVariable(context, XmlElementValue, Variables.ScalarValueObjectType_XmlElementValue);
            InitializeVariable(context, NodeIdValue, Variables.ScalarValueObjectType_NodeIdValue);
            InitializeVariable(context, ExpandedNodeIdValue, Variables.ScalarValueObjectType_ExpandedNodeIdValue);
            InitializeVariable(context, QualifiedNameValue, Variables.ScalarValueObjectType_QualifiedNameValue);
            InitializeVariable(context, LocalizedTextValue, Variables.ScalarValueObjectType_LocalizedTextValue);
            InitializeVariable(context, StatusCodeValue, Variables.ScalarValueObjectType_StatusCodeValue);
            InitializeVariable(context, VariantValue, Variables.ScalarValueObjectType_VariantValue);
            InitializeVariable(context, EnumerationValue, Variables.ScalarValueObjectType_EnumerationValue);
            InitializeVariable(context, StructureValue, Variables.ScalarValueObjectType_StructureValue);
            InitializeVariable(context, NumberValue, Variables.ScalarValueObjectType_NumberValue);
            InitializeVariable(context, IntegerValue, Variables.ScalarValueObjectType_IntegerValue);
            InitializeVariable(context, UIntegerValue, Variables.ScalarValueObjectType_UIntegerValue);
            InitializeVariable(context, VectorValue, Variables.ScalarValueObjectType_VectorValue);
            InitializeVariable(context, VectorUnionValue, Variables.ScalarValueObjectType_VectorUnionValue);
            InitializeVariable(context, VectorWithOptionalFieldsValue, Variables.ScalarValueObjectType_VectorWithOptionalFieldsValue);
            InitializeVariable(context, MultipleVectorsValue, Variables.ScalarValueObjectType_MultipleVectorsValue);
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
            var system = context.SystemHandle as TestDataSystem;

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
            GenerateValue(system, VectorValue);
            GenerateValue(system, VectorUnionValue);
            GenerateValue(system, VectorWithOptionalFieldsValue);
            GenerateValue(system, MultipleVectorsValue);

            return base.OnGenerateValues(context, method, objectId, count);
        }
        #endregion
    }
}
