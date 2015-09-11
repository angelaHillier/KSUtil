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
    using System.Linq;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Class which contains supporting functions for converting streams from text to guids, 
    /// and determining if specified streams are valid for playback/record.
    /// </summary>
    internal static class StreamSupport
    {
        /// <summary>
        /// Supported Kinect streams
        /// </summary>
        private static Dictionary<Guid, string> streamNameByGuid = new Dictionary<Guid, string>()
        {
            { KStudioEventStreamDataTypeIds.Body, Strings.Stream_Body.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.BodyIndex, Strings.Stream_BodyIndex.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.UncompressedColor, Strings.Stream_Color.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.Depth, Strings.Stream_Depth.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.Ir, Strings.Stream_IR.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.LongExposureIr, Strings.Stream_LongExpIR.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.RawIr, Strings.Stream_RawIR.ToUpperInvariant() },
            { KStudioEventStreamDataTypeIds.Null, Strings.Unknown.ToUpperInvariant() }
        };

        /// <summary>
        /// Converts a DataTypeID to a public stream
        /// </summary>
        /// <param name="streamDataTypeId">DataTypeID of stream to convert</param>
        /// <returns>String representation of a stream which matches the DataTypeID given</returns>
        internal static string ConvertStreamGuidToString(Guid streamDataTypeId)
        {
            if (streamDataTypeId == null)
            {
                throw new ArgumentNullException("streamDataTypeId");
            }

            if (streamNameByGuid.ContainsKey(streamDataTypeId))
            {
                return streamNameByGuid[streamDataTypeId];
            }
            else
            {
                return streamNameByGuid[KStudioEventStreamDataTypeIds.Null];
            }
        }

        /// <summary>
        /// Converts a string representing a stream name to a GUID (DataTypeID)
        /// </summary>
        /// <param name="streamName">String representation of a stream</param>
        /// <returns>DataTypeID of a stream which corresponds to the stream name provided</returns>
        internal static Guid ConvertStreamStringToGuid(string streamName)
        {
            if (string.IsNullOrEmpty(streamName))
            {
                throw new ArgumentNullException("streamName");
            }

            if (streamNameByGuid.ContainsValue(streamName.ToUpperInvariant()))
            {
                return streamNameByGuid.First(x => x.Value.Equals(streamName.ToUpperInvariant())).Key;
            }
            else
            {
                throw new InvalidOperationException(string.Format(Strings.ErrorInvalidStream, streamName));
            }
        }

        /// <summary>
        /// Converts a collection of stream strings to DataTypeIDs (Guids)
        /// </summary>
        /// <param name="streamNames">Collection of strings to convert to Guids</param>
        /// <returns>Collection of Guids, which includes DataTypeIDs for each stream</returns>
        internal static HashSet<Guid> ConvertStreamsToGuids(IEnumerable<string> streamNames)
        {
            if (streamNames == null)
            {
                throw new ArgumentNullException("streamNames");
            }

            if (streamNames.Count<string>() == 0)
            {
                throw new InvalidOperationException(Strings.ErrorNoStreams);
            }

            HashSet<Guid> streamGuids = new HashSet<Guid>();
            foreach (string streamName in streamNames)
            {
                streamGuids.Add(StreamSupport.ConvertStreamStringToGuid(streamName));
            }

            return streamGuids;
        }

        /// <summary>
        /// Converts a collection of stream strings to DataTypeIDs (Guids) that can be played back to the Kinect sensor
        /// </summary>
        /// <param name="streamNames">Collection of strings to convert to Guids</param>
        /// <returns>Collection of Guids, which includes DataTypeIDs for each stream</returns>
        internal static HashSet<Guid> ConvertStreamsToPlaybackGuids(IEnumerable<string> streamNames)
        {
            if (streamNames == null)
            {
                throw new ArgumentNullException("streamNames");
            }

            if (streamNames.Count<string>() == 0)
            {
                throw new InvalidOperationException(Strings.ErrorNoStreams);
            }

            HashSet<Guid> streamGuids = StreamSupport.ConvertStreamsToGuids(streamNames);

            // remove body streams from the playback collection
            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.Body))
            {
                Console.WriteLine(Strings.WarningIgnoreBodyDuringPlayback);
                streamGuids.Remove(KStudioEventStreamDataTypeIds.Body);
            }

            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.BodyIndex))
            {
                Console.WriteLine(Strings.WarningIgnoreBodyIndexDuringPlayback);
                streamGuids.Remove(KStudioEventStreamDataTypeIds.BodyIndex);
            }

            return streamGuids;
        }

        /// <summary>
        /// Converts a collection of stream GUIDs to a KStudioEventStreamSelectorCollection object that can be used for playback and record
        /// </summary>
        /// <param name="streamGuids">collection of stream Guids to add</param>
        /// <param name="swapColor">true, if the uncompressed and compressed color streams should be swapped</param>
        /// <returns>A KStudioEventStreamSelectorCollection object which contains the requested streams</returns>
        internal static KStudioEventStreamSelectorCollection CreateStreamCollection(IEnumerable<Guid> streamGuids, bool swapColor)
        {
            KStudioEventStreamSelectorCollection streamCollection = new KStudioEventStreamSelectorCollection();
            HashSet<Guid> streamDataTypeIds = streamGuids as HashSet<Guid>;

            if (swapColor)
            {
                if (streamDataTypeIds.Contains(KStudioEventStreamDataTypeIds.UncompressedColor))
                {
                    streamDataTypeIds.Remove(KStudioEventStreamDataTypeIds.UncompressedColor);
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.CompressedColor);
                }
                else if (streamDataTypeIds.Contains(KStudioEventStreamDataTypeIds.CompressedColor))
                {
                    streamDataTypeIds.Remove(KStudioEventStreamDataTypeIds.CompressedColor);
                    streamDataTypeIds.Add(KStudioEventStreamDataTypeIds.UncompressedColor);
                }
            }

            foreach (Guid dataTypeId in streamDataTypeIds)
            {
                streamCollection.Add(dataTypeId);
            }

            return streamCollection;
        }

        /// <summary>
        /// Verifies that the combination of streams provided are valid for record and/or playback
        /// </summary>
        /// <param name="streamGuids">Collection of Guids which represent KStudioEventStreamDataTypeIds</param>
        internal static void VerifyStreamsForRecordAndPlayback(IEnumerable<Guid> streamGuids)
        {
            if (streamGuids == null)
            {
                throw new ArgumentNullException("streamGuids");
            }

            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.Null))
            {
                throw new InvalidOperationException(Strings.ErrorNullStream);
            }

            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.RawIr) && streamGuids.Contains(KStudioEventStreamDataTypeIds.Ir))
            {
                throw new InvalidOperationException(Strings.ErrorRawIrWithIR);
            }

            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.Ir) && !streamGuids.Contains(KStudioEventStreamDataTypeIds.Depth))
            {
                throw new InvalidOperationException(Strings.ErrorIRWithoutDepth);
            }

            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.Depth) && !streamGuids.Contains(KStudioEventStreamDataTypeIds.Ir))
            {
                throw new InvalidOperationException(Strings.ErrorDepthWithoutIR);
            }

            if (streamGuids.Contains(KStudioEventStreamDataTypeIds.CompressedColor) && streamGuids.Contains(KStudioEventStreamDataTypeIds.CompressedColor))
            {
                throw new InvalidOperationException(Strings.ErrorCompressedAndUncompressedColor);
            }

            if ((streamGuids.Contains(KStudioEventStreamDataTypeIds.CompressedColor) ||
                 streamGuids.Contains(KStudioEventStreamDataTypeIds.UncompressedColor)) &&
                !(streamGuids.Contains(KStudioEventStreamDataTypeIds.RawIr) || streamGuids.Contains(KStudioEventStreamDataTypeIds.Ir)))
            {
                throw new InvalidOperationException(Strings.ErrorColorWithoutIR);
            }
        }
    }
}