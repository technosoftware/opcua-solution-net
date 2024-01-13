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

using Opc.Ua;
#endregion

namespace SampleCompany.NodeManagers.Alarms
{
    /// <summary>
    /// Helper class to allow for creation of various entities
    /// </summary>

    public class AlarmHelpers
    {
        /// <summary>
        /// Create a mechanism to create a folder
        /// </summary>
        public static FolderState CreateFolder(NodeState parent, ushort nameSpaceIndex, string path, string name)
        {
            FolderState folder = new FolderState(parent);

            folder.SymbolicName = name;
            folder.ReferenceTypeId = ReferenceTypes.Organizes;
            folder.TypeDefinitionId = ObjectTypeIds.FolderType;
            folder.NodeId = new NodeId(path, nameSpaceIndex);
            folder.BrowseName = new QualifiedName(path, nameSpaceIndex);
            folder.DisplayName = new LocalizedText("en", name);
            folder.WriteMask = AttributeWriteMask.None;
            folder.UserWriteMask = AttributeWriteMask.None;
            folder.EventNotifier = EventNotifiers.None;

            if (parent != null)
            {
                parent.AddChild(folder);
            }

            return folder;
        }

        /// <summary>
        /// Create a mechanism to create a variable
        /// </summary>
        public static BaseDataVariableState CreateVariable(NodeState parent, ushort nameSpaceIndex, string path, string name, bool boolValue = false)
        {
            uint dataTypeIdentifier = Opc.Ua.DataTypes.Int32;
            if (boolValue)
            {
                dataTypeIdentifier = Opc.Ua.DataTypes.Boolean;
            }
            return CreateVariable(parent, nameSpaceIndex, path, name, dataTypeIdentifier);
        }

        /// <summary>
        /// Create a mechanism to create a Variable
        /// </summary>
        public static BaseDataVariableState CreateVariable(NodeState parent, ushort nameSpaceIndex, string path, string name, uint dataTypeIdentifier)
        {
            BaseDataVariableState variable = new BaseDataVariableState(parent);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.TypeDefinitionId = VariableTypeIds.BaseDataVariableType;
            variable.NodeId = new NodeId(path, nameSpaceIndex);
            variable.BrowseName = new QualifiedName(name, nameSpaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description;
            variable.UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description;
            switch (dataTypeIdentifier)
            {
                case Opc.Ua.DataTypes.Boolean:
                    variable.DataType = DataTypeIds.Boolean;
                    variable.Value = false;
                    break;
                case Opc.Ua.DataTypes.Int32:
                    variable.DataType = DataTypeIds.Int32;
                    variable.Value = AlarmConstants.NormalStartValue;
                    break;
                case Opc.Ua.DataTypes.Double:
                    variable.DataType = DataTypeIds.Double;
                    variable.Value = (double)AlarmConstants.NormalStartValue;
                    break;

            }
            variable.ValueRank = ValueRanks.Scalar;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        /// <summary>
        /// Create a mechanism to create a method
        /// </summary>
        public static MethodState CreateMethod(NodeState parent, ushort nameSpaceIndex, string path, string name)
        {
            MethodState method = new MethodState(parent);

            method.SymbolicName = name;
            method.ReferenceTypeId = ReferenceTypeIds.HasComponent;
            method.NodeId = new NodeId(path, nameSpaceIndex);
            method.BrowseName = new QualifiedName(path, nameSpaceIndex);
            method.DisplayName = new LocalizedText("en", name);
            method.WriteMask = AttributeWriteMask.None;
            method.UserWriteMask = AttributeWriteMask.None;
            method.Executable = true;
            method.UserExecutable = true;

            if (parent != null)
            {
                parent.AddChild(method);
            }

            return method;
        }

        /// <summary>
        /// Add the input parameter description for a Start method.
        /// </summary>
        public static void AddStartInputParameters(MethodState startMethod, ushort namespaceIndex)
        {
            // set input arguments
            startMethod.InputArguments = new PropertyState<Argument[]>(startMethod);
            startMethod.InputArguments.NodeId = new NodeId(startMethod.BrowseName.Name + "InArgs", namespaceIndex);
            startMethod.InputArguments.BrowseName = BrowseNames.InputArguments;
            startMethod.InputArguments.DisplayName = startMethod.InputArguments.BrowseName.Name;
            startMethod.InputArguments.TypeDefinitionId = VariableTypeIds.PropertyType;
            startMethod.InputArguments.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            startMethod.InputArguments.DataType = DataTypeIds.Argument;
            startMethod.InputArguments.ValueRank = ValueRanks.OneDimension;

            startMethod.InputArguments.Value = new Argument[]
            {
                        new Argument() { Name = "UInt32 value", Description = "Runtime of Alarms in seconds.",  DataType = DataTypeIds.UInt32, ValueRank = ValueRanks.Scalar }
            };
        }
    }
}
