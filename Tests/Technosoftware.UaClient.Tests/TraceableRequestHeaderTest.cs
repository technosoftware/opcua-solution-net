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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

using Opc.Ua;
#endregion

namespace Technosoftware.UaClient.Tests
{
    public class TraceableRequestHeaderTest : ClientTestFramework
    {
        #region Test Setup
        /// <summary>
        /// Setup a server and client fixture.
        /// </summary>
        [OneTimeSetUp]
        public Task OneTimeSetUpAsync()
        {
            return base.OneTimeSetUpAsync(writer: null, securityNone: false, enableServerSideTracing: true, enableClientSideTracing: true);
        }

        /// <summary>
        /// Tear down the Server and the Client.
        /// </summary>
        [OneTimeTearDown]
        public new Task OneTimeTearDownAsync()
        {
            Utils.SilentDispose(ClientFixture);
            return base.OneTimeTearDownAsync();
        }

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public new Task SetUp()
        {
            return base.SetUp();
        }

        /// <summary>
        /// Test teardown.
        /// </summary>
        [TearDown]
        public new Task TearDown()
        {
            return base.TearDown();
        }
        #endregion

        #region Benchmark Setup
        /// <summary>
        /// Global Setup for benchmarks.
        /// </summary>
        [GlobalSetup]
        public new void GlobalSetup()
        {
            Console.WriteLine("GlobalSetup: Start Server");
            OneTimeSetUpAsync(Console.Out, enableServerSideTracing: true, enableClientSideTracing: true, disableActivityLogging: true).GetAwaiter().GetResult();
            Console.WriteLine("GlobalSetup: Connecting");
            InitializeSession(ClientFixture.ConnectAsync(ServerUrl, SecurityPolicy).GetAwaiter().GetResult());
            Console.WriteLine("GlobalSetup: Ready");
        }

        /// <summary>
        /// Global cleanup for benchmarks.
        /// </summary>
        [GlobalCleanup]
        public new void GlobalCleanup()
        {
            base.GlobalCleanup();
        }
        #endregion

        #region Test Methods

        [Test]
        [Benchmark]
        public void ReadValuesWithTracing()
        {
            var namespaceUris = Session.NamespaceUris;
            var testSet = new NodeIdCollection(GetTestSetStatic(namespaceUris));
            testSet.AddRange(GetTestSetFullSimulation(namespaceUris));
            Session.ReadValues(testSet, out DataValueCollection values, out IList<ServiceResult> errors);
            Assert.AreEqual(testSet.Count, values.Count);
            Assert.AreEqual(testSet.Count, errors.Count);
        }
        #endregion
    }
}
