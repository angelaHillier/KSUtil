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
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Class used to compare two event files together, stores the results for display in the UI
    /// </summary>
    public sealed class CompareFileData
    {
        /// <summary> 
        /// Initializes a new instance of the CompareFileData class
        /// </summary>
        public CompareFileData()
        {
            this.FileCompareText = string.Empty;
            this.PersonalMetadataCompareText = string.Empty;
            this.PublicMetadataCompareText = string.Empty;
            this.StreamsCompareText = string.Empty;
            this.LeftFileData = null;
            this.RightFileData = null;
            this.PublicMetadata = new ObservableCollection<CompareData>();
            this.PersonalMetadata = new ObservableCollection<CompareData>();
            this.Streams = new ObservableCollection<CompareStreamData>();
        }

        /// <summary>
        /// Initializes a new instance of the CompareFileData class and sets properties based on the FileData provided
        /// </summary>
        /// <param name="leftFile">First set of FileData to use in comparison</param>
        /// <param name="rightFile">Second set of FileData to use in comparison</param>
        public CompareFileData(FileData leftFile, FileData rightFile)
        {
            if (leftFile == null)
            {
                throw new ArgumentNullException("leftFile");
            }

            if (rightFile == null)
            {
                throw new ArgumentNullException("rightFile");
            }

            this.LeftFileData = leftFile;
            this.RightFileData = rightFile;
            this.FileCompareText = string.Format(Strings.CompareFilesHeader, Path.GetFileName(leftFile.FileName), Path.GetFileName(rightFile.FileName));
            bool samePublicMetadata = true;
            bool samePersonalMetadata = true;
            this.PublicMetadata = CompareFileData.CompareMetadata(leftFile.PublicMetadata, rightFile.PublicMetadata, out samePublicMetadata);
            this.PersonalMetadata = CompareFileData.CompareMetadata(leftFile.PersonalMetadata, rightFile.PersonalMetadata, out samePersonalMetadata);
            this.Streams = CompareFileData.CompareStreams(leftFile.Streams, rightFile.Streams);

            if (leftFile.PublicMetadata.Count == 0 && rightFile.PublicMetadata.Count == 0)
            {
                this.PublicMetadataCompareText = string.Format(Strings.PublicMetadataHeader, Strings.None);
            }
            else
            {
                this.PublicMetadataCompareText = string.Format(Strings.PublicMetadataHeader, samePublicMetadata ? Strings.Same : Strings.Different);
            }

            if (leftFile.PersonalMetadata.Count == 0 && rightFile.PersonalMetadata.Count == 0)
            {
                this.PersonalMetadataCompareText = string.Format(Strings.PersonalMetadataHeader, Strings.None);
            }
            else
            {
                this.PersonalMetadataCompareText = string.Format(Strings.PersonalMetadataHeader, samePersonalMetadata ? Strings.Same : Strings.Different);
            }

            if (leftFile.Streams.Count == 0 && rightFile.Streams.Count == 0)
            {
                this.StreamsCompareText = string.Format(Strings.StreamsHeader, Strings.None);
            }
            else
            {
                bool sameStreamData = this.Streams.All(data => data.Same);
                this.StreamsCompareText = string.Format(Strings.StreamsHeader, sameStreamData ? Strings.Same : Strings.Different);
            }
        }

        /// <summary> Gets a comparison string which represents both Files used in comparison </summary>
        public string FileCompareText { get; private set; }

        /// <summary> Gets the comparison result for Public Metadata </summary>
        public string PublicMetadataCompareText { get; private set; }

        /// <summary> Gets the comparison result for Personal Metadata </summary>
        public string PersonalMetadataCompareText { get; private set; }

        /// <summary> Gets the comparison result for Streams </summary>
        public string StreamsCompareText { get; private set; }

        /// <summary> Gets the left FileData object used in comparison </summary>
        public FileData LeftFileData { get; private set; }

        /// <summary> Gets the right FileData object used in comparison </summary>
        public FileData RightFileData { get; private set; }

        /// <summary> Gets the collection of all public, file-level metadata used in comparison </summary>
        public ObservableCollection<CompareData> PublicMetadata { get; private set; }

        /// <summary> Gets the collection of all personal, file-level metadata used in comparison </summary>
        public ObservableCollection<CompareData> PersonalMetadata { get; private set; }

        /// <summary> Gets the collection of all streams used in comparison </summary>
        public ObservableCollection<CompareStreamData> Streams { get; private set; }

        /// <summary>
        /// Formats an output string to show results for the -compare command-line parameter
        /// </summary>
        /// <returns>A string which represents the comparison result of two event files</returns>
        public string GetCompareFileDataAsText()
        {
            StringBuilder compareText = new StringBuilder();

            if (!string.IsNullOrEmpty(this.FileCompareText))
            {
                compareText.Append("-----------------------------------------------------");
                compareText.Append(Environment.NewLine);
                compareText.Append(this.FileCompareText);
                compareText.Append(Environment.NewLine);
                compareText.Append("-----------------------------------------------------");
                compareText.Append(Environment.NewLine);
                compareText.Append(Strings.FileMetadataHeader);
                compareText.Append(Environment.NewLine);
                compareText.Append("-----------------------------------------------------");
                compareText.Append(Environment.NewLine);
                
                compareText.Append(string.Format(Strings.PublicMetadataCompareCountHeader, this.LeftFileData.PublicMetadata.Count, this.RightFileData.PublicMetadata.Count));
                compareText.Append(this.GetCompareDataAsText(this.PublicMetadata));

                compareText.Append(Environment.NewLine + Environment.NewLine + string.Format(Strings.PersonalMetadataCompareCountHeader, this.LeftFileData.PersonalMetadata.Count, this.RightFileData.PersonalMetadata.Count));
                compareText.Append(this.GetCompareDataAsText(this.PersonalMetadata));

                compareText.Append(Environment.NewLine);
                compareText.Append(Environment.NewLine);
                compareText.Append("-----------------------------------------------------");
                compareText.Append(Environment.NewLine);
                compareText.Append(string.Format(Strings.StreamsCompareCountHeader, this.LeftFileData.Streams.Count, this.RightFileData.Streams.Count));
                compareText.Append(Environment.NewLine);
                compareText.Append("-----------------------------------------------------");
                
                foreach (CompareStreamData stream in this.Streams)
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(stream.StreamName);
                    compareText.Append(this.GetCompareDataAsText(stream.StreamDetails));

                    compareText.Append(Environment.NewLine);
                    compareText.Append("   ");
                    compareText.Append(string.Format(Strings.PublicMetadataCompareCountHeader, stream.LeftPublicMetadataCount, stream.RightPublicMetadataCount));
                    compareText.Append(this.GetCompareDataAsText(stream.PublicMetadata));

                    compareText.Append(Environment.NewLine);
                    compareText.Append("   ");
                    compareText.Append(string.Format(Strings.PersonalMetadataCompareCountHeader, stream.LeftPersonalMetadataCount, stream.RightPersonalMetadataCount));
                    compareText.Append(this.GetCompareDataAsText(stream.PersonalMetadata));
                }

                compareText.Append(Environment.NewLine);
                compareText.Append(Environment.NewLine);
                compareText.Append("-----------------------------------------------------");
                compareText.Append(Environment.NewLine);
                compareText.Append(Strings.ComparisonResultsHeader);
                compareText.Append(Environment.NewLine);
                compareText.Append("-----------------------------------------------------");
                
                if (this.PublicMetadataCompareText.Contains(Strings.Different))
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(Strings.PublicMetadataDifferentResult);
                }
                else
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(Strings.PublicMetadataSameResult);
                }

                if (this.PersonalMetadataCompareText.Contains(Strings.Different))
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(Strings.PersonalMetadataDifferentResult);
                }
                else
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(Strings.PersonalMetadataSameResult);
                }

                if (this.StreamsCompareText.Contains(Strings.Different))
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(Strings.StreamsDifferentResult);
                }
                else
                {
                    compareText.Append(Environment.NewLine);
                    compareText.Append(Strings.StreamsSameResult);
                }
            }

            compareText.Append(Environment.NewLine);
            compareText.Append("-----------------------------------------------------");

            return compareText.ToString();
        }

        /// <summary>
        /// Compares two metadata collections together
        /// </summary>
        /// <param name="leftMetadata">Metadata list from the first file to use in comparison</param>
        /// <param name="rightMetadata">Metadata list from the second file to use in comparison</param>
        /// <param name="same">True if the metadata keys and values were identical between both files, false otherwise</param>
        /// <returns>A list which contains the combined metadata from both files used in comparison</returns>
        internal static ObservableCollection<CompareData> CompareMetadata(IEnumerable<KeyValuePair<string, object>> leftMetadata, IEnumerable<KeyValuePair<string, object>> rightMetadata, out bool same)
        {
            ObservableCollection<CompareData> compareMetadata = new ObservableCollection<CompareData>();
            same = true;

            // populate the comparison list with all key/value pairs found in the leftMetadata list
            if (leftMetadata != null)
            {
                foreach (KeyValuePair<string, object> pair in leftMetadata)
                {
                    string key = pair.Key;
                    string leftValue = Metadata.ConvertMetadataValueToString(pair.Value);
                    string rightValue = " ";
                    compareMetadata.Add(new CompareData(key, leftValue, rightValue));
                }
            }

            // populate the comparison list with any new values found in the rightMetadata list
            if (rightMetadata != null)
            {
                foreach (KeyValuePair<string, object> pair in rightMetadata)
                {
                    bool found = false;

                    for (int i = 0; i < compareMetadata.Count; i++)
                    {
                        if (compareMetadata[i].Key == pair.Key)
                        {
                            // update existing CompareData object
                            compareMetadata[i].RightValue = Metadata.ConvertMetadataValueToString(pair.Value);
                            compareMetadata[i].Same = compareMetadata[i].LeftValue.Equals(compareMetadata[i].RightValue);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        // add new CompareData object
                        string key = pair.Key;
                        string leftValue = " ";
                        string rightValue = Metadata.ConvertMetadataValueToString(pair.Value);
                        compareMetadata.Add(new CompareData(key, leftValue, rightValue));
                    }
                }
            }

            foreach (CompareData data in compareMetadata)
            {
                if (!data.Same)
                {
                    same = false;
                    break;
                }
            }

            return compareMetadata;
        }

        /// <summary>
        /// Compares similar streams from two different files together
        /// </summary>
        /// <param name="leftStreams">Stream list from first file to use in comparison</param>
        /// <param name="rightStreams">Stream list from second file to use in comparison</param>
        /// <returns>Collection of CompareStreamData, which contains comparison results of all streams</returns>
        internal static ObservableCollection<CompareStreamData> CompareStreams(IEnumerable<StreamData> leftStreams, IEnumerable<StreamData> rightStreams)
        {
            ObservableCollection<CompareStreamData> compareStreams = new ObservableCollection<CompareStreamData>();
            List<string> streamNames = new List<string>();

            // find unique and shared streams across both files
            if (leftStreams != null)
            {
                foreach (StreamData stream in leftStreams)
                {
                    streamNames.Add(stream.EventStream.DataTypeName);
                }
            }

            if (rightStreams != null)
            {
                foreach (StreamData stream in rightStreams)
                {
                    if (!streamNames.Contains(stream.EventStream.DataTypeName))
                    {
                        streamNames.Add(stream.EventStream.DataTypeName);
                    }
                }
            }

            // loop through each stream to compare between files
            foreach (string streamName in streamNames)
            {
                StreamData leftStream = null;
                StreamData rightStream = null;

                // find stream in left file
                if (leftStreams != null)
                {
                    foreach (StreamData stream in leftStreams)
                    {
                        if (stream.EventStream.DataTypeName.Equals(streamName))
                        {
                            leftStream = stream;
                            break;
                        }
                    }
                }

                // find stream in right file
                if (rightStreams != null)
                {
                    foreach (StreamData stream in rightStreams)
                    {
                        if (stream.EventStream.DataTypeName.Equals(streamName))
                        {
                            rightStream = stream;
                            break;
                        }
                    }
                }

                compareStreams.Add(new CompareStreamData(leftStream, rightStream));
            }

            return compareStreams;
        }

        /// <summary>
        /// Returns the CompareData object as a string for output to the command-line or log file
        /// </summary>
        /// <param name="compareData">The object to convert</param>
        /// <returns>A string which represents the CompareData object</returns>
        private string GetCompareDataAsText(IEnumerable<CompareData> compareData)
        {
            StringBuilder compareText = new StringBuilder();
            foreach (CompareData cd in compareData)
            {
                compareText.Append(Environment.NewLine);
                compareText.Append("   ");
                compareText.Append(cd.Key);
                compareText.Append(Environment.NewLine);
                compareText.Append("\t");
                compareText.Append(Strings.LeftValueHeader);
                compareText.Append(": ");
                compareText.Append(cd.LeftValue);
                compareText.Append(Environment.NewLine);
                compareText.Append("\t");
                compareText.Append(Strings.RightValueHeader);
                compareText.Append(": ");
                compareText.Append(cd.RightValue);
            }

            return compareText.ToString();
        }
    }
}
