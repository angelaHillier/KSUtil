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
    using System.Threading;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Playback class, which supports playing an event file to the Kinect service
    /// </summary>
    public static class Playback
    {
        /// <summary>
        /// Plays an event file to the Kinect service
        /// </summary>
        /// <param name="client">KStudioClient which is connected to the Kinect service</param>
        /// <param name="filePath">Path to event file which is targeted for playback</param>
        /// <param name="streamNames">Collection of streams to include in the playback session</param>
        /// <param name="loopCount">Number of times the playback should be repeated before stopping</param>
        public static void PlaybackClip(KStudioClient client, string filePath, IEnumerable<string> streamNames, uint loopCount)
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

            KStudioPlayback playback = null;

            // determine if all specified streams are valid for playback
            if (streamNames.Count<string>() > 0)
            {
                HashSet<Guid> playbackDataTypeIds = StreamSupport.ConvertStreamsToPlaybackGuids(streamNames);
                StreamSupport.VerifyStreamsForRecordAndPlayback(playbackDataTypeIds);
                Playback.VerifyStreamsForPlayback(client, filePath, playbackDataTypeIds);

                try
                {
                    KStudioEventStreamSelectorCollection streams = StreamSupport.CreateStreamCollection(playbackDataTypeIds, false);
                    playback = client.CreatePlayback(filePath, streams);
                }
                catch (Exception)
                {
                    //K4W supports uncompressed and compressed color, so if we get an error, try playing the other type
                    KStudioEventStreamSelectorCollection streams = StreamSupport.CreateStreamCollection(playbackDataTypeIds, true);
                    playback = client.CreatePlayback(filePath, streams);
                }
            }
            else
            {
                playback = client.CreatePlayback(filePath);
            }

            // begin playback
            using (playback)
            {
                playback.EndBehavior = KStudioPlaybackEndBehavior.Stop; // this is the default behavior
                playback.Mode = KStudioPlaybackMode.TimingEnabled; // this is the default behavior
                playback.LoopCount = loopCount;
                playback.Start();

                while (playback.State == KStudioPlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }

                if (playback.State == KStudioPlaybackState.Error)
                {
                    throw new InvalidOperationException(Strings.ErrorPlaybackFailed);
                }
            }
        }

        /// <summary>
        /// Verifies that the streams selected for playback exist in the file and are capable of being played on the service
        /// </summary>
        /// <param name="client">KStudioClient which is connected to the Kinect service</param>
        /// <param name="filePath">Path to file that will be played back</param>
        /// <param name="playbackStreams">Collection of streams which have been selected for playback</param>
        private static void VerifyStreamsForPlayback(KStudioClient client, string filePath, IEnumerable<Guid> playbackStreams)
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

            if (playbackStreams == null)
            {
                throw new ArgumentNullException("playbackStreams");
            }

            // verify stream exists in the file
            using (KStudioEventFile file = client.OpenEventFile(filePath))
            {
                HashSet<Guid> fileStreams = new HashSet<Guid>();
                foreach (KStudioEventStream stream in file.EventStreams)
                {
                    fileStreams.Add(stream.DataTypeId);
                }

                if (!fileStreams.IsSupersetOf(playbackStreams))
                {
                    Guid invalidStream = playbackStreams.First(x => !fileStreams.Contains(x));
                    throw new InvalidOperationException(string.Format(Strings.ErrorPlaybackStreamNotInFile, StreamSupport.ConvertStreamGuidToString(invalidStream)));
                }
            }

            // verify stream is supported for playback by the Kinect sensor
            foreach (Guid stream in playbackStreams)
            {
                KStudioEventStream eventStream = client.GetEventStream(stream, KStudioEventStreamSemanticIds.KinectDefaultSensorConsumer);
                if (!eventStream.IsPlaybackable)
                {
                    throw new InvalidOperationException(string.Format(Strings.ErrorPlaybackStreamNotSupported, StreamSupport.ConvertStreamGuidToString(stream)));
                }
            }
        }

		public static void PlaybackClip(KStudioClient client, string filePath, List<string> streamNames, uint loopCount, TimeSpan startingRelativeTime, TimeSpan endingRelativeTime)
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

			KStudioPlayback playback = null;

			// determine if all specified streams are valid for playback
			if (streamNames.Count<string>() > 0)
			{
				HashSet<Guid> playbackDataTypeIds = StreamSupport.ConvertStreamsToPlaybackGuids(streamNames);
				StreamSupport.VerifyStreamsForRecordAndPlayback(playbackDataTypeIds);
				Playback.VerifyStreamsForPlayback(client, filePath, playbackDataTypeIds);

				try
				{
					KStudioEventStreamSelectorCollection streams = StreamSupport.CreateStreamCollection(playbackDataTypeIds, false);
					playback = client.CreatePlayback(filePath, streams);
				}
				catch (Exception)
				{
					//K4W supports uncompressed and compressed color, so if we get an error, try playing the other type
					KStudioEventStreamSelectorCollection streams = StreamSupport.CreateStreamCollection(playbackDataTypeIds, true);
					playback = client.CreatePlayback(filePath, streams);
				}
			}
			else
			{
				playback = client.CreatePlayback(filePath);
			}

			// begin playback
			using (playback)
			{
				playback.EndBehavior = KStudioPlaybackEndBehavior.Stop; // this is the default behavior
				playback.Mode = KStudioPlaybackMode.TimingEnabled; // this is the default behavior
				playback.LoopCount = loopCount;
                if (startingRelativeTime != TimeSpan.MinValue)
                {
                    playback.InPointByRelativeTime = startingRelativeTime;
                }
                if (endingRelativeTime != TimeSpan.MinValue)
                {
                    playback.OutPointByRelativeTime = endingRelativeTime;
                }

				playback.Start();

				while (playback.State == KStudioPlaybackState.Playing)
				{
					Thread.Sleep(500);
				}

				if (playback.State == KStudioPlaybackState.Error)
				{
					throw new InvalidOperationException(Strings.ErrorPlaybackFailed);
				}
			}
		}
	}
}
