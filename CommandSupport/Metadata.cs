//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Metadata class which contains supporting functions to access/update/remove metadata from an event file
    /// </summary>
    public static class Metadata
    {
        /// <summary>
        /// Converts a metadata value to a string
        /// </summary>
        /// <param name="metadataValue">Value to convert</param>
        /// <returns>String representation of the metadata value</returns>
        public static string ConvertMetadataValueToString(object metadataValue)
        {
            if (metadataValue == null)
            {
                throw new ArgumentNullException("metadataValue");
            }

            string result = string.Empty;

            if (metadataValue is ICollection)
            {
                ICollection collection = metadataValue as ICollection;
                foreach (object o in collection)
                {
                    result += o.ToString() + ";";
                }
            }
            else if (metadataValue is KStudioMetadataValueBuffer)
            {
                result = Strings.MetadataValueBufferFriendlyName;
            }
            else
            {
                result = metadataValue.ToString();
            }

            return result;
        }

        /// <summary>
        /// Updates (adds/edits) file-level metadata in an event file
        /// </summary>
        /// <param name="client">KStudioClient to use for accessing the event file</param>
        /// <param name="fileName">Path to event file</param>
        /// <param name="type">Type of metadata (Public or Personal)</param>
        /// <param name="key">Key of metadata object to add/edit</param>
        /// <param name="value">Value of metadata object to add/edit</param>
        /// <returns>String which contains the updated contents of the target metadata object</returns>
        public static string UpdateFileMetadata(KStudioClient client, string fileName, KStudioMetadataType type, string key, object value)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            string metadataText = string.Empty;
            
            using (KStudioEventFile file = client.OpenEventFileForEdit(fileName))
            {
                KStudioMetadata metadata = file.GetMetadata(type);
                Metadata.AlterMetadata(metadata, key, value);
                metadataText = Metadata.GetMetadataAsText(metadata, type, string.Empty);
            }

            return metadataText;
        }

        /// <summary>
        /// Updates (add/edits) stream-level metadata in an event file
        /// </summary>
        /// <param name="client">KStudioClient to use for accessing the event file</param>
        /// <param name="fileName">Path to event file</param>
        /// <param name="streamName">Name of stream which should contain the metadata</param>
        /// <param name="type">Type of metadata to update (Public or Personal)</param>
        /// <param name="key">Key of metadata object to add/edit</param>
        /// <param name="value">Value of metadata object to add/edit</param>
        /// <returns>String which contains the updated contents of the target metadata object</returns>
        public static string UpdateStreamMetadata(KStudioClient client, string fileName, string streamName, KStudioMetadataType type, string key, object value)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (string.IsNullOrEmpty(streamName))
            {
                throw new ArgumentNullException("streamName");
            }

            string metadataText = string.Empty;

            using (KStudioEventFile file = client.OpenEventFileForEdit(fileName))
            {
                // find the stream in the file and alter its metadata
                Guid dataTypeId = StreamSupport.ConvertStreamStringToGuid(streamName);
                if (dataTypeId == KStudioEventStreamDataTypeIds.Null)
                {
                    throw new InvalidOperationException(Strings.ErrorNullStream);
                }

                KStudioEventStream stream = file.EventStreams.FirstOrDefault(s => s.DataTypeId == dataTypeId);
                if (stream != null)
                {
                    KStudioMetadata metadata = stream.GetMetadata(type);
                    Metadata.AlterMetadata(metadata, key, value);
                    metadataText = Metadata.GetMetadataAsText(metadata, type, stream.DataTypeName);
                }
            }

            return metadataText;
        }

        /// <summary>
        /// Alters (add/edit/remove) a metadata item within a KStudioMetadata object
        /// </summary>
        /// <param name="metadata">The metadata object to alter</param>
        /// <param name="key">The key of the particular metadata item to alter</param>
        /// <param name="value">New value for the metadata object when adding/editing</param>
        private static void AlterMetadata(KStudioMetadata metadata, string key, object value)
        {
            if (metadata == null)
            {
                throw new InvalidOperationException(Strings.ErrorInvalidMetadataPair);
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException(Strings.ErrorNullMetadataKey);
            }

            metadata[key] = value;
        }

        /// <summary>
        /// Returns all key/value pairs in a metadata object as a single string
        /// </summary>
        /// <param name="metadata">Collection of metadata items</param>
        /// <param name="type">Public or PersonallyIdentifiableInformation</param>
        /// <param name="streamName">Name of stream which metadata belongs to, empty if file-level metadata</param>
        /// <returns>A string which contains all key/value pairs in the metadata object</returns>
        private static string GetMetadataAsText(KStudioMetadata metadata, KStudioMetadataType type, string streamName)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            StringBuilder metadataString = new StringBuilder();
            metadataString.Append(Environment.NewLine);

            if (string.IsNullOrEmpty(streamName))
            {
                metadataString.Append(Strings.File);
            }
            else
            {
                metadataString.Append(streamName);
            }

            metadataString.Append(" ");
            
            if (type == KStudioMetadataType.Public)
            {
                metadataString.Append(string.Format(Strings.PublicMetadataHeader, metadata.Count));
            }
            else
            {
                metadataString.Append(string.Format(Strings.PersonalMetadataHeader, metadata.Count));
            }
            
            foreach (KeyValuePair<string, object> pair in metadata)
            {
                metadataString.Append(Environment.NewLine);
                metadataString.Append(" ");
                metadataString.Append(pair.Key);
                metadataString.Append(" = ");
                metadataString.Append(Metadata.ConvertMetadataValueToString(pair.Value));
            }

            metadataString.Append(Environment.NewLine);
            return metadataString.ToString();
        }
    }
}
