#region Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2021 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
// 
// License: 
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// SPDX-License-Identifier: MIT
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion

namespace SampleCompany.SampleServer.UnderlyingSystem
{
    /// <summary>
    /// Stores the metadata for a node representing a folder on a file system.
    /// </summary>
    public class ArchiveFolder
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public ArchiveFolder(string uniquePath, DirectoryInfo folder)
        {
            UniquePath = uniquePath;
            DirectoryInfo = folder;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the child folders.
        /// </summary>
        public ArchiveFolder[] GetChildFolders()
        {
            var folders = new List<ArchiveFolder>();

            if (!DirectoryInfo.Exists)
            {
                return folders.ToArray();
            }

            foreach (var directory in DirectoryInfo.GetDirectories())
            {
                var buffer = new StringBuilder(UniquePath);
                buffer.Append('/');
                buffer.Append(directory.Name);
                folders.Add(new ArchiveFolder(buffer.ToString(), directory));
            }

            return folders.ToArray();
        }

        /// <summary>
        /// Returns the child folders.
        /// </summary>
        public ArchiveItem[] GetItems()
        {
            var items = new List<ArchiveItem>();

            if (!DirectoryInfo.Exists)
            {
                return items.ToArray();
            }

            foreach (var file in DirectoryInfo.GetFiles("*.csv"))
            {
                var buffer = new StringBuilder(UniquePath);
                buffer.Append('/');
                buffer.Append(file.Name);
                items.Add(new ArchiveItem(buffer.ToString(), file));
            }

            return items.ToArray();
        }

        /// <summary>
        /// Returns the parent folder.
        /// </summary>
        public ArchiveFolder GetParentFolder()
        {
            var parentPath = String.Empty;

            if (!DirectoryInfo.Exists)
            {
                return null;
            }

            var index = UniquePath.LastIndexOf('/');

            if (index > 0)
            {
                parentPath = UniquePath.Substring(0, index);
            }

            return new ArchiveFolder(parentPath, DirectoryInfo.Parent);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The unique path to the folder in the archive.
        /// </summary>
        public string UniquePath { get; }

        /// <summary>
        /// A name for the folder.
        /// </summary>
        public string Name
        {
            get { return DirectoryInfo.Name; }
        }

        /// <summary>
        /// The physical folder in the archive.
        /// </summary>
        public DirectoryInfo DirectoryInfo { get; }
        #endregion

        #region Private Fields
        #endregion
    }
}
