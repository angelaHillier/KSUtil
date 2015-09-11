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
    using System.Text;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Demonstrates how to get metadata and stream information from an event file with the KStudioEventFile class.
    /// Properties are gathered for display in the UI and/or output to the command window.
    /// </summary>
    public sealed class FileData
    {
        /// <summary>
        /// Initializes a new instance of the FileData class
        /// </summary>
        public FileData()
        {
            this.FileName = string.Empty;
            this.PublicMetadata = null;
            this.PersonalMetadata = null;
            this.Streams = new ObservableCollection<StreamData>();
        }

        /// <summary>
        /// Initializes a new instance of the FileData class and sets all properties to the values found within the KStudioEventFile object
        /// </summary>
        /// <param name="eventFile">A KStudioEventFile that is currently opened for read access</param>
        public FileData(KStudioEventFile eventFile)
        {
            if (eventFile == null)
            {
                throw new ArgumentNullException("eventFile");
            }
            
            this.FileName = eventFile.FilePath;
            this.PublicMetadata = eventFile.GetMetadata(KStudioMetadataType.Public);
            this.PersonalMetadata = eventFile.GetMetadata(KStudioMetadataType.PersonallyIdentifiableInformation);
            this.Streams = new ObservableCollection<StreamData>();
            foreach (KStudioEventStream stream in eventFile.EventStreams)
            {
                this.Streams.Add(new StreamData(stream));
            }
        }

        /// <summary> Gets the name of the event file </summary>
        public string FileName { get; private set; }

        /// <summary> Gets a KStudioMetadata object which contains all public, file-level metadata within the event file </summary>
        public KStudioMetadata PublicMetadata { get; private set; }

        /// <summary> Gets a KStudioMetadata object which contains all personal, file-level metadata within the event file </summary>
        public KStudioMetadata PersonalMetadata { get; private set; }
        
        /// <summary> Gets a collection of StreamData objects which represent all of the streams in the event file </summary>
        public ObservableCollection<StreamData> Streams { get; private set; }

        /// <summary>
        /// Retrieves a string representation of event file data which can be used in command output and logs
        /// </summary>
        /// <returns>A string which represents data in the event file</returns>
        public string GetFileDataAsText()
        {
            StringBuilder fileInfo = new StringBuilder();
            fileInfo.Append("-----------------------------------------------------");
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(string.Format(Strings.FileHeader, this.FileName));
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append("-----------------------------------------------------");
            fileInfo.Append(Environment.NewLine);
            
            fileInfo.Append(Strings.FileMetadataHeader);
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append("-----------------------------------------------------");
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(string.Format(Strings.PublicMetadataHeader, this.PublicMetadata.Count));
            fileInfo.Append(this.GetMetadataAsText(this.PublicMetadata, false));

            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(string.Format(Strings.PersonalMetadataHeader, this.PersonalMetadata.Count));
            fileInfo.Append(this.GetMetadataAsText(this.PersonalMetadata, false));

            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append("-----------------------------------------------------");
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append(string.Format(Strings.StreamsHeader, this.Streams.Count));
            fileInfo.Append(Environment.NewLine);
            fileInfo.Append("-----------------------------------------------------");
            foreach (StreamData stream in this.Streams)
            {
                fileInfo.Append(Environment.NewLine);
                fileInfo.Append(Environment.NewLine);
                fileInfo.Append(stream.EventStream.DataTypeName);
                fileInfo.Append(Environment.NewLine);
                fileInfo.Append("  ");
                fileInfo.Append(Strings.DataTypeIdHeader);
                fileInfo.Append(": ");
                fileInfo.Append(stream.EventStream.DataTypeId);
                fileInfo.Append(Environment.NewLine);
                fileInfo.Append("  ");
                fileInfo.Append(Strings.SemanticIdHeader);
                fileInfo.Append(": ");
                fileInfo.Append(stream.EventStream.SemanticId);
                fileInfo.Append(Environment.NewLine);
                fileInfo.Append("  ");
                fileInfo.Append(string.Format(Strings.EventsHeader, stream.EventHeaders.Count));

                fileInfo.Append(Environment.NewLine);
                fileInfo.Append("  ");
                fileInfo.Append(string.Format(Strings.PublicMetadataHeader, stream.PublicMetadata.Count));
                fileInfo.Append(this.GetMetadataAsText(stream.PublicMetadata, true));

                fileInfo.Append(Environment.NewLine);
                fileInfo.Append("  ");
                fileInfo.Append(string.Format(Strings.PersonalMetadataHeader, stream.PersonalMetadata.Count));
                fileInfo.Append(this.GetMetadataAsText(stream.PersonalMetadata, true));
            }

            fileInfo.Append(Environment.NewLine);
            fileInfo.Append("-----------------------------------------------------");

            return fileInfo.ToString();
        }

        /// <summary>
        /// Returns all key/value pairs in a metadata object as a single string
        /// </summary>
        /// <param name="metadata">Collection of metadata items</param>
        /// <param name="isStreamMetadata">True for stream-level metadata; false for file-level metadata</param>
        /// <returns>A string which contains all key/value pairs in the metadata object</returns>
        private string GetMetadataAsText(IEnumerable<KeyValuePair<string, object>> metadata, bool isStreamMetadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            
            StringBuilder metadataString = new StringBuilder();
            
            foreach (KeyValuePair<string, object> pair in metadata)
            {
                metadataString.Append(Environment.NewLine);
                if (isStreamMetadata)
                {
                    metadataString.Append("\t");
                }
                else
                {
                    metadataString.Append(" ");
                }

                metadataString.Append(pair.Key);
                metadataString.Append(" = ");
                metadataString.Append(Metadata.ConvertMetadataValueToString(pair.Value));
            }

            return metadataString.ToString();
        }
    }
}
