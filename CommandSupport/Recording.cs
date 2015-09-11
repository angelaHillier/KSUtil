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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Recording class, which supports recording streams from the Kinect sensor
    /// </summary>
    public static class Recording
    {
        /// <summary>
        /// Records streams from the Kinect sensor to an event file
        /// </summary>
        /// <param name="client">KStudioClient which is connected to the Kinect service</param>
        /// <param name="filePath">Path to new event file which will be created for recording</param>
        /// <param name="duration">How long the recording should last before being stopped</param>
        /// <param name="streamNames">Collection of streams to include in the recording</param>
        public static void RecordClip(KStudioClient client, string filePath, TimeSpan duration, IEnumerable<string> streamNames)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (!client.IsServiceConnected)
            {
                throw new InvalidOperationException(Strings.ErrorNotConnected);
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            HashSet<Guid> streamDataTypeIds = new HashSet<Guid>();
            KStudioEventStreamSelectorCollection streamCollection = new KStudioEventStreamSelectorCollection();
            KStudioRecording recording = null;

            // decide which streams to record
            if (streamNames != null && streamNames.Count<string>() > 0)
            {
                streamDataTypeIds = StreamSupport.ConvertStreamsToGuids(streamNames);
                StreamSupport.VerifyStreamsForRecordAndPlayback(streamDataTypeIds);
            }
            else
            {
                if (Path.GetExtension(filePath).ToLower().Equals(Strings.XrfExtension))
                {
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.RawIr);
                }
                else
                {
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.Ir);
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.Depth);
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.Body);
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.BodyIndex);
                }
            }

            // verify streams are recordable by the Kinect sensor
            foreach (Guid stream in streamDataTypeIds)
            {
                KStudioEventStream eventStream = client.GetEventStream(stream, KStudioEventStreamSemanticIds.KinectDefaultSensorProducer);
                if (!eventStream.IsRecordable)
                {
                    throw new InvalidOperationException(string.Format(Strings.ErrorRecordingStreamNotSupported, StreamSupport.ConvertStreamGuidToString(stream)));
                }

                streamCollection.Add(stream);
            }

            // fix file extension, if necessary
            if (streamDataTypeIds.Contains(KStudioEventStreamDataTypeIds.RawIr) && Path.GetExtension(filePath).ToUpperInvariant().Equals(Strings.XefExtension.ToUpperInvariant()))
            {
                Path.ChangeExtension(filePath, Strings.XrfExtension);
            }

            // attempt to record streams for the specified duration
            try
            {
                recording = client.CreateRecording(filePath, streamCollection);
            }
            catch (Exception)
            {
                //K4W supports uncompressed and compressed color, so if we get an error, try recording the other type
                streamCollection = StreamSupport.CreateStreamCollection(streamDataTypeIds, true);
                recording = client.CreateRecording(filePath, streamCollection);
            }

            using (recording)
            {
                recording.StartTimed(duration);
                while (recording.State == KStudioRecordingState.Recording)
                {
                    Thread.Sleep(500);
                }

                if (recording.State == KStudioRecordingState.Error)
                {
                    throw new InvalidOperationException(Strings.ErrorRecordingFailed);
                }
            }
        }
    }
}
