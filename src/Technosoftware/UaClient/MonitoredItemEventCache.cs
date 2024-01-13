#region Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is subject to the Technosoftware GmbH Software License 
// Agreement, which can be found here:
// https://technosoftware.com/documents/Source_License_Agreement.pdf
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System.Collections.Generic;

using Opc.Ua;
#endregion

namespace Technosoftware.UaClient
{ 
    /// <summary>
    /// Saves the events received from the server.
    /// </summary>
    public class MonitoredItemEventCache
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Constructs a cache for a monitored item.
        /// </summary>
        public MonitoredItemEventCache(int queueSize)
        {
            queueSize_ = queueSize;
            events_ = new Queue<EventFieldList>();
        }
        #endregion

        #region Public Members
        /// <summary>
        /// The size of the queue to maintain.
        /// </summary>
        public int QueueSize => queueSize_;

        /// <summary>
        /// The last event received.
        /// </summary>
        public EventFieldList LastEvent => lastEvent_;

        /// <summary>
        /// Returns all events in the queue.
        /// </summary>
        public IList<EventFieldList> Publish()
        {
            var events = new EventFieldList[events_.Count];

            for (var ii = 0; ii < events.Length; ii++)
            {
                events[ii] = events_.Dequeue();
            }

            return events;
        }

        /// <summary>
        /// Saves a notification in the cache.
        /// </summary>
        public void OnNotification(EventFieldList notification)
        {
            events_.Enqueue(notification);
            lastEvent_ = notification;

            while (events_.Count > queueSize_)
            {
                events_.Dequeue();
            }
        }

        /// <summary>
        /// Changes the queue size.
        /// </summary>
        public void SetQueueSize(int queueSize)
        {
            if (queueSize == queueSize_)
            {
                return;
            }

            if (queueSize < 1)
            {
                queueSize = 1;
            }

            queueSize_ = queueSize;

            while (events_.Count > queueSize_)
            {
                events_.Dequeue();
            }
        }
        #endregion

        #region Private Fields
        private int queueSize_;
        private EventFieldList lastEvent_;
        private readonly Queue<EventFieldList> events_;
        #endregion
    }
}
