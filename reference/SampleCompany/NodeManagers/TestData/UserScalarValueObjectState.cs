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
    public partial class UserScalarValueObjectState
    {
        #region Initialization
        /// <summary>
        /// Initializes the object as a collection of counters which change value on read.
        /// </summary>
        protected override void OnAfterCreate(ISystemContext context, NodeState node)
        {
            base.OnAfterCreate(context, node);

            InitializeVariable(context, BooleanValue, Variables.UserScalarValueObjectType_BooleanValue);
            InitializeVariable(context, SByteValue, Variables.UserScalarValueObjectType_SByteValue);
            InitializeVariable(context, ByteValue, Variables.UserScalarValueObjectType_ByteValue);
            InitializeVariable(context, Int16Value, Variables.UserScalarValueObjectType_Int16Value);
            InitializeVariable(context, UInt16Value, Variables.UserScalarValueObjectType_UInt16Value);
            InitializeVariable(context, Int32Value, Variables.UserScalarValueObjectType_Int32Value);
            InitializeVariable(context, UInt32Value, Variables.UserScalarValueObjectType_UInt32Value);
            InitializeVariable(context, Int64Value, Variables.UserScalarValueObjectType_Int64Value);
            InitializeVariable(context, UInt64Value, Variables.UserScalarValueObjectType_UInt64Value);
            InitializeVariable(context, FloatValue, Variables.UserScalarValueObjectType_FloatValue);
            InitializeVariable(context, DoubleValue, Variables.UserScalarValueObjectType_DoubleValue);
            InitializeVariable(context, StringValue, Variables.UserScalarValueObjectType_StringValue);
            InitializeVariable(context, DateTimeValue, Variables.UserScalarValueObjectType_DateTimeValue);
            InitializeVariable(context, GuidValue, Variables.UserScalarValueObjectType_GuidValue);
            InitializeVariable(context, ByteStringValue, Variables.UserScalarValueObjectType_ByteStringValue);
            InitializeVariable(context, XmlElementValue, Variables.UserScalarValueObjectType_XmlElementValue);
            InitializeVariable(context, NodeIdValue, Variables.UserScalarValueObjectType_NodeIdValue);
            InitializeVariable(context, ExpandedNodeIdValue, Variables.UserScalarValueObjectType_ExpandedNodeIdValue);
            InitializeVariable(context, QualifiedNameValue, Variables.UserScalarValueObjectType_QualifiedNameValue);
            InitializeVariable(context, LocalizedTextValue, Variables.UserScalarValueObjectType_LocalizedTextValue);
            InitializeVariable(context, StatusCodeValue, Variables.UserScalarValueObjectType_StatusCodeValue);
            InitializeVariable(context, VariantValue, Variables.UserScalarValueObjectType_VariantValue);
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

            return base.OnGenerateValues(context, method, objectId, count);
        }
        #endregion
    }
}
