//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Supporting class for FileData.cs
    /// Demonstrates how to get stream information from an event file using the KStudioSeekableEventStream class
    /// </summary>
    public sealed class StreamData
    {
        /// <summary>
        /// Initializes a new instance of the StreamData class
        /// </summary>
        public StreamData()
        {
            this.EventStream = null;
            this.StartTime = new TimeSpan();
            this.EndTime = new TimeSpan();
            this.PublicMetadata = null;
            this.PersonalMetadata = null;
            this.EventHeaders = new ObservableCollection<KStudioEventHeader>();
        }

        /// <summary>
        /// Initializes a new instance of the StreamData class and sets all properties based on the KStudioEventStream provided
        /// </summary>
        /// <param name="stream">KStudioEventStream to get data from</param>
        public StreamData(KStudioEventStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            this.EventStream = stream;            
            this.PublicMetadata = stream.GetMetadata(KStudioMetadataType.Public);
            this.PersonalMetadata = stream.GetMetadata(KStudioMetadataType.PersonallyIdentifiableInformation);
            this.EventHeaders = new ObservableCollection<KStudioEventHeader>();
            
            KStudioSeekableEventStream seekStream = stream as KStudioSeekableEventStream;
            if (seekStream != null)
            {
                this.StartTime = seekStream.StartRelativeTime;
                this.EndTime = seekStream.EndRelativeTime;

                List<KStudioEventHeader> headers = seekStream.EventHeaders.ToList();
                foreach (KStudioEventHeader header in headers)
                {
                    this.EventHeaders.Add(header);
                }
            }
        }
        
        /// <summary> Gets a KStudioEventStream object which represents a Kinect data stream in the event file </summary>
        public KStudioEventStream EventStream { get; private set; }
        
        /// <summary> Gets a KStudioMetadata objects which represents public, stream-level metadata </summary>
        public KStudioMetadata PublicMetadata { get; private set; }
        
        /// <summary> Gets a KStudioMetadata object which represents personal, stream-level metadata </summary>
        public KStudioMetadata PersonalMetadata { get; private set; }
        
        /// <summary> Gets a collection of events that occur in the stream </summary>
        public ObservableCollection<KStudioEventHeader> EventHeaders { get; private set; }

        /// <summary> Gets the time of the first event tick in the stream </summary>
        public TimeSpan StartTime { get; private set; }

        /// <summary> Gets the time of the final event tick in the stream </summary>
        public TimeSpan EndTime { get; private set; }
    }
}
