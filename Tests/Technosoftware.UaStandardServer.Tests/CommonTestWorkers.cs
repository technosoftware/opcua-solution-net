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
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Opc.Ua;
#endregion

namespace Technosoftware.UaStandardServer.Tests
{
    /// <summary>
    /// Test workers using test services.
    /// </summary>
    public static class CommonTestWorkers
    {
        public const int DefaultMonitoredItemsQueueSize = 0;
        public const int DefaultMonitoredItemsSamplingInterval = -1;

        #region Public Test Sets
        public static readonly ExpandedNodeId[] NodeIdTestSetStatic =
        {
            new ExpandedNodeId("Scalar_Static_SByte", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_Int16", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_Int32", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_Byte", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_UInt16", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_UInt32", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_NodeId", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_LocalizedText", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_QualifiedName", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Static_Variant", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
        };

        // static variables from namespace TestData
        public static readonly ExpandedNodeId[] NodeIdTestDataSetStatic =
        {
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Static_Scalar_Int16Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Static_Scalar_Int32Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Static_Scalar_UInt16Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Static_Scalar_UInt32Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
        };

        // CTT simulation data
        public static readonly ExpandedNodeId[] NodeIdTestSetSimulation =
        {
            new ExpandedNodeId("Scalar_Simulation_SByte", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_Int16", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_Int32", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_Byte", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_UInt16", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_UInt32", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_NodeId", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_LocalizedText", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_QualifiedName", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
            new ExpandedNodeId("Scalar_Simulation_Variant", SampleCompany.NodeManagers.Reference.Namespaces.ReferenceServer),
        };

        /// <summary>
        /// Ref server test data node manager.
        /// </summary>
        public static readonly ExpandedNodeId[] NodeIdTestSetDataSimulation =
        {
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_Int16Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_Int32Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_UInt16Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_UInt32Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.AnalogScalarValueObjectType_UInt32Value,SampleCompany.NodeManagers. TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_AnalogArray_ByteValue, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_VectorValue, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_VectorValue_X, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Structure_ScalarStructure, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
        };

        public static readonly ExpandedNodeId[] NodeIdTestDataHistory =
        {
            new ExpandedNodeId(SampleCompany.NodeManagers.TestData.Variables.Data_Dynamic_Scalar_Int32Value, SampleCompany.NodeManagers.TestData.Namespaces.TestData),
        };

        public static readonly ExpandedNodeId[] NodeIdMemoryBufferSimulation =
            {
            // dynamic variables from namespace MemoryBuffer
            new ExpandedNodeId("UInt32[64]", SampleCompany.NodeManagers.MemoryBuffer.Namespaces.MemoryBuffer + "/Instance"),
            new ExpandedNodeId("Double[40]", SampleCompany.NodeManagers.MemoryBuffer.Namespaces.MemoryBuffer + "/Instance"),
        };
        #endregion

        #region Public Workers
        /// <summary>
        /// Worker function to browse the full address space of a server.
        /// </summary>
        /// <param name="services">The service interface.</param>
        /// <param name="operationLimits">The operation limits.</param>
        public static ReferenceDescriptionCollection BrowseFullAddressSpaceWorker(
            IServerTestServices services,
            RequestHeader requestHeader,
            OperationLimits operationLimits = null,
            BrowseDescription browseDescription = null)
        {
            operationLimits = operationLimits ?? new OperationLimits();
            requestHeader.Timestamp = DateTime.UtcNow;

            // Browse template
            var startingNode = Objects.RootFolder;
            var browseTemplate = browseDescription ?? new BrowseDescription {
                NodeId = startingNode,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = 0,
                ResultMask = (uint)BrowseResultMask.All
            };
            var browseDescriptionCollection = ServerFixtureUtils.CreateBrowseDescriptionCollectionFromNodeId(
                new NodeIdCollection(new NodeId[] { Objects.RootFolder }),
                browseTemplate);

            // Browse
            ResponseHeader response = null;
            uint requestedMaxReferencesPerNode = operationLimits.MaxNodesPerBrowse;
            bool verifyMaxNodesPerBrowse = operationLimits.MaxNodesPerBrowse > 0;
            var referenceDescriptions = new ReferenceDescriptionCollection();

            // Test if server responds with BadNothingToDo
            {
                var sre = Assert.Throws<ServiceResultException>(() =>
                    _ = services.Browse(requestHeader, null,
                        0, browseDescriptionCollection.Take(0).ToArray(),
                        out var results, out var infos));
                Assert.AreEqual(StatusCodes.BadNothingToDo, sre.StatusCode);
            }

            while (browseDescriptionCollection.Any())
            {
                BrowseResultCollection allResults = new BrowseResultCollection();
                if (verifyMaxNodesPerBrowse &&
                    browseDescriptionCollection.Count > operationLimits.MaxNodesPerBrowse)
                {
                    verifyMaxNodesPerBrowse = false;
                    // Test if server responds with BadTooManyOperations
                    var sre = Assert.Throws<ServiceResultException>(() =>
                        _ = services.Browse(requestHeader, null,
                            0, browseDescriptionCollection,
                            out var results, out var infos));
                    Assert.AreEqual(StatusCodes.BadTooManyOperations, sre.StatusCode);

                    // Test if server responds with BadTooManyOperations
                    var tempBrowsePath = browseDescriptionCollection.Take((int)operationLimits.MaxNodesPerBrowse + 1).ToArray();
                    sre = Assert.Throws<ServiceResultException>(() =>
                        _ = services.Browse(requestHeader, null,
                            0, tempBrowsePath,
                            out var results, out var infos));
                    Assert.AreEqual(StatusCodes.BadTooManyOperations, sre.StatusCode);
                }

                bool repeatBrowse;
                var maxNodesPerBrowse = operationLimits.MaxNodesPerBrowse;
                BrowseResultCollection browseResultCollection = new BrowseResultCollection();
                DiagnosticInfoCollection diagnosticsInfoCollection;
                do
                {
                    var browseCollection = (maxNodesPerBrowse == 0) ?
                        browseDescriptionCollection :
                        browseDescriptionCollection.Take((int)maxNodesPerBrowse).ToArray();
                    repeatBrowse = false;
                    try
                    {
                        requestHeader.Timestamp = DateTime.UtcNow;
                        response = services.Browse(requestHeader, null,
                            requestedMaxReferencesPerNode, browseCollection,
                            out browseResultCollection, out diagnosticsInfoCollection);
                        ServerFixtureUtils.ValidateResponse(response, browseResultCollection, browseCollection);
                        ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticsInfoCollection, browseCollection, response.StringTable);

                        allResults.AddRange(browseResultCollection);
                    }
                    catch (ServiceResultException sre)
                    {
                        if (sre.StatusCode == StatusCodes.BadEncodingLimitsExceeded ||
                            sre.StatusCode == StatusCodes.BadResponseTooLarge)
                        {
                            // try to address by overriding operation limit
                            maxNodesPerBrowse = maxNodesPerBrowse == 0 ?
                                (uint)browseCollection.Count / 2 : maxNodesPerBrowse / 2;
                            repeatBrowse = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                } while (repeatBrowse);

                if (maxNodesPerBrowse == 0)
                {
                    browseDescriptionCollection.Clear();
                }
                else
                {
                    browseDescriptionCollection = browseDescriptionCollection.Skip((int)maxNodesPerBrowse).ToArray();
                }

                // Browse next
                var continuationPoints = ServerFixtureUtils.PrepareBrowseNext(browseResultCollection);
                while (continuationPoints.Any())
                {
                    requestHeader.Timestamp = DateTime.UtcNow;
                    response = services.BrowseNext(requestHeader, false, continuationPoints,
                        out var browseNextResultCollection, out diagnosticsInfoCollection);
                    ServerFixtureUtils.ValidateResponse(response, browseNextResultCollection, continuationPoints);
                    ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticsInfoCollection, continuationPoints, response.StringTable);
                    allResults.AddRange(browseNextResultCollection);
                    continuationPoints = ServerFixtureUtils.PrepareBrowseNext(browseNextResultCollection);
                }

                // Build browse request for next level
                var browseTable = new NodeIdCollection();
                foreach (var result in allResults)
                {
                    referenceDescriptions.AddRange(result.References);
                    foreach (var reference in result.References)
                    {
                        browseTable.Add(ExpandedNodeId.ToNodeId(reference.NodeId, null));
                    }
                }
                browseDescriptionCollection = ServerFixtureUtils.CreateBrowseDescriptionCollectionFromNodeId(browseTable, browseTemplate);
            }

            referenceDescriptions.Sort((x, y) => (x.NodeId.CompareTo(y.NodeId)));

            TestContext.Out.WriteLine("Found {0} references on server.", referenceDescriptions.Count);
            foreach (var reference in referenceDescriptions)
            {
                TestContext.Out.WriteLine("NodeId {0} {1} {2}", reference.NodeId, reference.NodeClass, reference.BrowseName);
            }
            return referenceDescriptions;
        }

        /// <summary>
        /// Worker method to translate the browse path.
        /// </summary>
        public static BrowsePathResultCollection TranslateBrowsePathWorker(
            IServerTestServices services,
            ReferenceDescriptionCollection referenceDescriptions,
            RequestHeader requestHeader,
            OperationLimits operationLimits)
        {
            // Browse template
            var startingNode = Objects.RootFolder;
            requestHeader.Timestamp = DateTime.UtcNow;

            // TranslateBrowsePath
            bool verifyMaxNodesPerBrowse = operationLimits.MaxNodesPerTranslateBrowsePathsToNodeIds > 0;
            var browsePaths = new BrowsePathCollection(
                referenceDescriptions.Select(r => new BrowsePath() { RelativePath = new RelativePath(r.BrowseName), StartingNode = startingNode })
                );
            BrowsePathResultCollection allBrowsePaths = new BrowsePathResultCollection();
            while (browsePaths.Any())
            {
                if (verifyMaxNodesPerBrowse &&
                    browsePaths.Count > operationLimits.MaxNodesPerTranslateBrowsePathsToNodeIds)
                {
                    verifyMaxNodesPerBrowse = false;
                    // Test if server responds with BadTooManyOperations
                    var sre = Assert.Throws<ServiceResultException>(() =>
                        _ = services.TranslateBrowsePathsToNodeIds(requestHeader, browsePaths, out var results, out var infos));
                    Assert.AreEqual(StatusCodes.BadTooManyOperations, sre.StatusCode);
                }
                var browsePathSnippet = (operationLimits.MaxNodesPerTranslateBrowsePathsToNodeIds > 0) ?
                    browsePaths.Take((int)operationLimits.MaxNodesPerTranslateBrowsePathsToNodeIds).ToArray() :
                    browsePaths;
                ResponseHeader response = services.TranslateBrowsePathsToNodeIds(requestHeader, browsePathSnippet, out var browsePathResults, out var diagnosticInfos);
                ServerFixtureUtils.ValidateResponse(response, browsePathResults, browsePathSnippet);
                ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, browsePathSnippet, response.StringTable);
                allBrowsePaths.AddRange(browsePathResults);
                foreach (var result in browsePathResults)
                {
                    if (result.Targets?.Count > 0)
                    {
                        TestContext.Out.WriteLine("BrowsePath {0}", result.Targets[0].ToString());
                    }
                }

                if (operationLimits.MaxNodesPerTranslateBrowsePathsToNodeIds == 0)
                {
                    browsePaths.Clear();
                }
                else
                {
                    browsePaths = browsePaths.Skip((int)operationLimits.MaxNodesPerTranslateBrowsePathsToNodeIds).ToArray();
                }
            }
            return allBrowsePaths;
        }

        /// <summary>
        /// Worker method to test subscriptions of a server.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="requestHeader"></param>
        public static void SubscriptionTest(
            IServerTestServices services,
            RequestHeader requestHeader)
        {
            // start time
            requestHeader.Timestamp = DateTime.UtcNow;

            // create subscription
            double publishingInterval = 1000.0;
            uint lifetimeCount = 60;
            uint maxKeepAliveCount = 2;
            uint maxNotificationPerPublish = 0;
            byte priority = 128;
            bool enabled = false;
            uint queueSize = 5;

            var response = services.CreateSubscription(requestHeader,
                publishingInterval, lifetimeCount, maxKeepAliveCount,
                maxNotificationPerPublish, enabled, priority,
                out uint id, out double revisedPublishingInterval, out uint revisedLifetimeCount, out uint revisedMaxKeepAliveCount);
            Assert.AreEqual(publishingInterval, revisedPublishingInterval);
            Assert.AreEqual(lifetimeCount, revisedLifetimeCount);
            Assert.AreEqual(maxKeepAliveCount, revisedMaxKeepAliveCount);
            ServerFixtureUtils.ValidateResponse(response);

            MonitoredItemCreateRequestCollection itemsToCreate = new MonitoredItemCreateRequestCollection();
            // check badnothingtodo
            var sre = Assert.Throws<ServiceResultException>(() =>
                services.CreateMonitoredItems(requestHeader, id, TimestampsToReturn.Neither, itemsToCreate,
                    out MonitoredItemCreateResultCollection mockResults, out DiagnosticInfoCollection mockInfos));
            Assert.AreEqual(StatusCodes.BadNothingToDo, sre.StatusCode);

            // add item
            uint handleCounter = 1;
            itemsToCreate.Add(new MonitoredItemCreateRequest() {
                ItemToMonitor = new ReadValueId() {
                    AttributeId = Attributes.Value,
                    NodeId = VariableIds.Server_ServerStatus_CurrentTime
                },
                MonitoringMode = MonitoringMode.Reporting,
                RequestedParameters = new MonitoringParameters() {
                    ClientHandle = ++handleCounter,
                    SamplingInterval = -1,
                    Filter = null,
                    DiscardOldest = true,
                    QueueSize = queueSize
                }
            });
            response = services.CreateMonitoredItems(requestHeader, id, TimestampsToReturn.Neither, itemsToCreate,
                out MonitoredItemCreateResultCollection itemCreateResults, out DiagnosticInfoCollection diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, itemCreateResults, itemsToCreate);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, itemsToCreate, response.StringTable);

            // modify subscription
            response = services.ModifySubscription(requestHeader, id,
                publishingInterval, lifetimeCount, maxKeepAliveCount,
                maxNotificationPerPublish, priority,
                out revisedPublishingInterval, out revisedLifetimeCount, out revisedMaxKeepAliveCount);
            Assert.AreEqual(publishingInterval, revisedPublishingInterval);
            Assert.AreEqual(lifetimeCount, revisedLifetimeCount);
            Assert.AreEqual(maxKeepAliveCount, revisedMaxKeepAliveCount);
            ServerFixtureUtils.ValidateResponse(response);

            // modify monitored item, just timestamps to return
            var itemsToModify = new MonitoredItemModifyRequestCollection();
            foreach (var itemCreated in itemCreateResults)
            {
                itemsToModify.Add(
                    new MonitoredItemModifyRequest() {
                        MonitoredItemId = itemCreated.MonitoredItemId
                    });
            };
            response = services.ModifyMonitoredItems(requestHeader, id, TimestampsToReturn.Both, itemsToModify,
                        out MonitoredItemModifyResultCollection modifyResults, out diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, modifyResults, itemsToModify);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, itemsToModify, response.StringTable);

            // publish request
            var acknowledgements = new SubscriptionAcknowledgementCollection();
            response = services.Publish(requestHeader, acknowledgements,
                        out uint subscriptionId, out UInt32Collection availableSequenceNumbers,
                        out bool moreNotifications, out NotificationMessage notificationMessage,
                        out StatusCodeCollection statuses, out diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, statuses, acknowledgements);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, acknowledgements, response.StringTable);
            Assert.AreEqual(id, subscriptionId);
            Assert.AreEqual(0, availableSequenceNumbers.Count);

            // enable publishing
            enabled = true;
            var subscriptions = new UInt32Collection() { id };
            response = services.SetPublishingMode(requestHeader, enabled, subscriptions,
                        out statuses, out diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, statuses, subscriptions);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, subscriptions, response.StringTable);

            // wait some time to fill queue
            int loopCounter = (int)queueSize;
            Thread.Sleep(loopCounter * 1000);

            acknowledgements = new SubscriptionAcknowledgementCollection();
            do
            {
                // get publish responses
                response = services.Publish(requestHeader, acknowledgements,
                    out subscriptionId, out availableSequenceNumbers,
                    out moreNotifications, out notificationMessage,
                    out statuses, out diagnosticInfos);
                ServerFixtureUtils.ValidateResponse(response, statuses, acknowledgements);
                ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, acknowledgements, response.StringTable);
                Assert.AreEqual(id, subscriptionId);

                var dataChangeNotification = notificationMessage.NotificationData[0].Body as DataChangeNotification;
                TestContext.Out.WriteLine("Notification: {0} {1} {2}",
                                notificationMessage.SequenceNumber,
                                dataChangeNotification?.MonitoredItems[0].Value.ToString(),
                                notificationMessage.PublishTime);

                acknowledgements.Clear();
                acknowledgements.Add(new SubscriptionAcknowledgement() {
                    SubscriptionId = id,
                    SequenceNumber = notificationMessage.SequenceNumber
                });

            } while (acknowledgements.Count > 0 && --loopCounter > 0);

            // republish
            response = services.Republish(requestHeader, subscriptionId, notificationMessage.SequenceNumber, out notificationMessage);
            ServerFixtureUtils.ValidateResponse(response);

            // disable publishing
            enabled = false;
            response = services.SetPublishingMode(requestHeader, enabled, subscriptions,
                out statuses, out diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, statuses, subscriptions);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, subscriptions, response.StringTable);

            // delete subscription
            response = services.DeleteSubscriptions(requestHeader, subscriptions, out statuses, out diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, statuses, subscriptions);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, subscriptions, response.StringTable);
        }

        /// <summary>
        /// Worker method to test TransferSubscriptions of a server.
        /// </summary>
        public static UInt32Collection CreateSubscriptionForTransfer(
            IServerTestServices services,
            RequestHeader requestHeader,
            NodeId[] testNodes,
            uint queueSize = DefaultMonitoredItemsQueueSize,
            int samplingInterval = DefaultMonitoredItemsSamplingInterval)
        {
            // start time

            requestHeader.Timestamp = DateTime.UtcNow;
            uint subscriptionId = CreateSubscription(services, requestHeader);
            uint clientHandle = 1;
            foreach (NodeId testNode in testNodes)
            {
                CreateMonitoredItem(services, requestHeader, subscriptionId, testNode, clientHandle++, queueSize, samplingInterval);
            }

            var subscriptionIds = new UInt32Collection();
            subscriptionIds.Add(subscriptionId);

            // enable publishing
            var response = services.SetPublishingMode(requestHeader, true, subscriptionIds,
                        out var statuses, out var diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, statuses, subscriptionIds);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, subscriptionIds, response.StringTable);

            // wait some time to settle
            Thread.Sleep(1000);

            // publish request (use invalid sequence number for status)
            var acknowledgements = new SubscriptionAcknowledgementCollection() {
                new SubscriptionAcknowledgement()
                    { SubscriptionId = subscriptionId, SequenceNumber=123 }
                };
            response = services.Publish(requestHeader, acknowledgements,
                out uint publishedId, out UInt32Collection availableSequenceNumbers,
                out bool moreNotifications, out NotificationMessage notificationMessage,
                out statuses, out diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, statuses, acknowledgements);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, acknowledgements, response.StringTable);
            Assert.AreEqual(subscriptionId, publishedId);

            // static node, do not acknowledge
            Assert.AreEqual(1, availableSequenceNumbers.Count);

            return subscriptionIds;
        }

        /// <summary>
        /// Worker method to test Transfer of subscriptions to new session.
        /// </summary>
        public static void TransferSubscriptionTest(
            IServerTestServices services,
            RequestHeader requestHeader,
            UInt32Collection subscriptionIds,
            bool sendInitialData,
            bool expectAccessDenied)
        {
            Assert.AreEqual(1, subscriptionIds.Count);

            requestHeader.Timestamp = DateTime.UtcNow;
            var response = services.TransferSubscriptions(requestHeader, subscriptionIds, sendInitialData,
                out TransferResultCollection transferResults, out DiagnosticInfoCollection diagnosticInfos);
            Assert.AreEqual(StatusCodes.Good, response.ServiceResult.Code);
            Assert.AreEqual(subscriptionIds.Count, transferResults.Count);
            ServerFixtureUtils.ValidateResponse(response, transferResults, subscriptionIds);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, subscriptionIds, response.StringTable);

            foreach (var transferResult in transferResults)
            {
                TestContext.Out.WriteLine("TransferResult: {0}", transferResult.StatusCode);
                if (expectAccessDenied)
                {
                    Assert.AreEqual(StatusCodes.BadUserAccessDenied, transferResult.StatusCode.Code);
                }
                else
                {
                    Assert.IsTrue(StatusCode.IsGood(transferResult.StatusCode));
                    Assert.AreEqual(1, transferResult.AvailableSequenceNumbers.Count);
                }
            }

            if (expectAccessDenied)
            {
                return;
            }

            requestHeader.Timestamp = DateTime.UtcNow;
            var acknowledgements = new SubscriptionAcknowledgementCollection();
            response = services.Publish(requestHeader, acknowledgements,
                out uint publishedId, out UInt32Collection availableSequenceNumbers,
                out bool moreNotifications, out NotificationMessage notificationMessage,
                out StatusCodeCollection _, out diagnosticInfos);
            Assert.AreEqual(StatusCodes.Good, response.ServiceResult.Code);
            ServerFixtureUtils.ValidateResponse(response);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, acknowledgements, response.StringTable);
            Assert.AreEqual(subscriptionIds[0], publishedId);
            Assert.AreEqual(sendInitialData ? 1 : 0, notificationMessage.NotificationData.Count);
            if (sendInitialData)
            {
                var items = notificationMessage.NotificationData.FirstOrDefault();
                Assert.IsTrue(items.Body is Opc.Ua.DataChangeNotification);
                var monitoredItemsCollection = ((Opc.Ua.DataChangeNotification)items.Body).MonitoredItems;
                Assert.IsNotEmpty(monitoredItemsCollection);
            }
            //Assert.AreEqual(0, availableSequenceNumbers.Count);

            requestHeader.Timestamp = DateTime.UtcNow;
            response = services.DeleteSubscriptions(requestHeader, subscriptionIds, out StatusCodeCollection statusResults, out diagnosticInfos);
            Assert.AreEqual(StatusCodes.Good, response.ServiceResult.Code);
        }

        /// <summary>
        /// Worker method to verify the SubscriptionTransferred message of a server.
        /// </summary>
        public static void VerifySubscriptionTransferred(
            IServerTestServices services,
            RequestHeader requestHeader,
            UInt32Collection subscriptionIds,
            bool deleteSubscriptions)
        {
            // start time
            requestHeader.Timestamp = DateTime.UtcNow;

            // wait some time to settle
            Thread.Sleep(100);

            // publish request
            var acknowledgements = new SubscriptionAcknowledgementCollection();
            var response = services.Publish(requestHeader, acknowledgements,
                out uint publishedId, out UInt32Collection availableSequenceNumbers,
                out bool moreNotifications, out NotificationMessage notificationMessage,
                out StatusCodeCollection _, out var diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, acknowledgements, response.StringTable);
            Assert.IsFalse(moreNotifications);
            Assert.IsTrue(subscriptionIds.Contains(publishedId));
            Assert.AreEqual(1, notificationMessage.NotificationData.Count);
            var statusMessage = notificationMessage.NotificationData[0].ToString();
            Assert.IsTrue(statusMessage.Contains("GoodSubscriptionTransferred"));

            // static node, do not acknowledge
            if (availableSequenceNumbers != null)
            {
                Assert.AreEqual(0, availableSequenceNumbers.Count);
            }

            if (deleteSubscriptions)
            {
                response = services.DeleteSubscriptions(requestHeader, subscriptionIds, out var statuses, out diagnosticInfos);
                ServerFixtureUtils.ValidateResponse(response, statuses, subscriptionIds);
                ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, subscriptionIds, response.StringTable);
            }
        }
        #endregion

        #region Private Helpers
        private static uint CreateSubscription(IServerTestServices services, RequestHeader requestHeader)
        {
            // start time
            requestHeader.Timestamp = DateTime.UtcNow;

            // create subscription
            double publishingInterval = 1000.0;
            uint lifetimeCount = 60;
            uint maxKeepAliveCount = 2;
            uint maxNotificationPerPublish = 0;
            byte priority = 128;
            bool enabled = false;

            var response = services.CreateSubscription(requestHeader,
                publishingInterval, lifetimeCount, maxKeepAliveCount,
                maxNotificationPerPublish, enabled, priority,
                out uint id, out double revisedPublishingInterval, out uint revisedLifetimeCount, out uint revisedMaxKeepAliveCount);
            ServerFixtureUtils.ValidateResponse(response);

            return id;
        }

        private static void CreateMonitoredItem(
            IServerTestServices services, RequestHeader requestHeader,
            uint subscriptionId, NodeId nodeId,
            uint clientHandle,
            uint queueSize,
            int samplingInterval
            )
        {
            var itemsToCreate = new MonitoredItemCreateRequestCollection {
                // add item
                new MonitoredItemCreateRequest {
                    ItemToMonitor = new ReadValueId {
                        AttributeId = Attributes.Value,
                        NodeId = nodeId
                    },
                    MonitoringMode = MonitoringMode.Reporting,
                    RequestedParameters = new MonitoringParameters {
                        ClientHandle = clientHandle,
                        SamplingInterval = samplingInterval,
                        Filter = null,
                        DiscardOldest = true,
                        QueueSize = queueSize
                    }
                }
            };
            var response = services.CreateMonitoredItems(requestHeader, subscriptionId, TimestampsToReturn.Neither, itemsToCreate,
                out MonitoredItemCreateResultCollection itemCreateResults, out DiagnosticInfoCollection diagnosticInfos);
            ServerFixtureUtils.ValidateResponse(response, itemCreateResults, itemsToCreate);
            ServerFixtureUtils.ValidateDiagnosticInfos(diagnosticInfos, itemsToCreate, response.StringTable);
        }
        #endregion

    }
}
