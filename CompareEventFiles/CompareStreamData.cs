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
    /// Supporting class for CompareFileData.cs
    /// Compares stream data from two different event files and stores the results for display in the UI.
    /// </summary>
    public sealed class CompareStreamData
    {
        /// <summary>
        /// Initializes a new instance of the CompareStreamData class
        /// </summary>
        public CompareStreamData()
        {
            this.Same = false;
            this.StreamDetailsCompareText = string.Empty;
            this.StreamPublicMetadataCompareText = string.Empty;
            this.StreamPersonalMetadataCompareText = string.Empty;
            this.StreamName = string.Empty;
            this.LeftPublicMetadataCount = 0;
            this.RightPublicMetadataCount = 0;
            this.LeftPersonalMetadataCount = 0;
            this.RightPersonalMetadataCount = 0;
            this.StreamDetails = new ObservableCollection<CompareData>();
            this.PublicMetadata = new ObservableCollection<CompareData>();
            this.PersonalMetadata = new ObservableCollection<CompareData>();
        }

        /// <summary>
        /// Initializes a new instance of the CompareStreamData class and sets all properties based on the StreamData provided
        /// </summary>
        /// <param name="leftStream">First StreamData object to use for comparison</param>
        /// <param name="rightStream">Second StreamData object to use for comparison</param>
        public CompareStreamData(StreamData leftStream, StreamData rightStream)
        {
            if (leftStream == null && rightStream == null)
            {
                throw new InvalidOperationException(Strings.ErrorNoStreams);
            }

            this.StreamName = Strings.Unknown;
            this.LeftPublicMetadataCount = 0;
            this.RightPublicMetadataCount = 0;
            this.LeftPersonalMetadataCount = 0;
            this.RightPersonalMetadataCount = 0;
            string dataTypeIdKey = Strings.DataTypeIdHeader;
            string semanticIdKey = Strings.SemanticIdHeader;
            string eventCountKey = Strings.EventCountHeader;
            string startTimeKey = Strings.StreamStartTimeHeader;
            string endTimeKey = Strings.StreamEndTimeHeader;
            string leftSemanticId = string.Empty;
            string rightSemanticId = string.Empty;
            string leftEventCount = string.Empty;
            string rightEventCount = string.Empty;
            string leftStartTime = string.Empty;
            string rightStartTime = string.Empty;
            string leftEndTime = string.Empty;
            string rightEndTime = string.Empty;
            string leftDataTypeId = string.Empty;
            string rightDataTypeId = string.Empty;
            KStudioMetadata leftPublicMetadata = null;
            KStudioMetadata rightPublicMetadata = null;
            KStudioMetadata leftPersonalMetadata = null;
            KStudioMetadata rightPersonalMetadata = null;

            if (leftStream != null)
            {
                this.StreamName = leftStream.EventStream.DataTypeName;
                leftSemanticId = leftStream.EventStream.SemanticId.ToString();
                leftEventCount = leftStream.EventHeaders.Count.ToString();
                leftStartTime = leftStream.StartTime.ToString();
                leftEndTime = leftStream.EndTime.ToString();
                leftDataTypeId = leftStream.EventStream.DataTypeId.ToString();
                leftPublicMetadata = leftStream.PublicMetadata;
                leftPersonalMetadata = leftStream.PersonalMetadata;
                this.LeftPublicMetadataCount = leftStream.PublicMetadata.Count;
                this.LeftPersonalMetadataCount = leftStream.PersonalMetadata.Count;
            }

            if (rightStream != null)
            {
                if (leftStream == null)
                {
                    this.StreamName = rightStream.EventStream.DataTypeName;
                }

                rightSemanticId = rightStream.EventStream.SemanticId.ToString();
                rightEventCount = rightStream.EventHeaders.Count.ToString();
                rightStartTime = rightStream.StartTime.ToString();
                rightEndTime = rightStream.EndTime.ToString();
                rightDataTypeId = rightStream.EventStream.DataTypeId.ToString();
                rightPublicMetadata = rightStream.PublicMetadata;
                rightPersonalMetadata = rightStream.PersonalMetadata;
                this.RightPublicMetadataCount = rightStream.PublicMetadata.Count;
                this.RightPersonalMetadataCount = rightStream.PersonalMetadata.Count;
            }

            // compare stream metadata
            bool samePublicMetadata = true;
            bool samePersonalMetadata = true;
            this.PublicMetadata = CompareFileData.CompareMetadata(leftPublicMetadata, rightPublicMetadata, out samePublicMetadata);
            this.PersonalMetadata = CompareFileData.CompareMetadata(leftPersonalMetadata, rightPersonalMetadata, out samePersonalMetadata);

            if (this.LeftPublicMetadataCount == 0 && this.RightPublicMetadataCount == 0)
            {
                this.StreamPublicMetadataCompareText = string.Format(Strings.PublicMetadataHeader, Strings.None);
            }
            else
            {
                this.StreamPublicMetadataCompareText = string.Format(Strings.PublicMetadataHeader, samePublicMetadata ? Strings.Same : Strings.Different);
            }

            if (this.LeftPersonalMetadataCount == 0 && this.RightPersonalMetadataCount == 0)
            {
                this.StreamPersonalMetadataCompareText = string.Format(Strings.PersonalMetadataHeader, Strings.None);
            }
            else
            {
                this.StreamPersonalMetadataCompareText = string.Format(Strings.PersonalMetadataHeader, samePersonalMetadata ? Strings.Same : Strings.Different);
            }

            // compare stream details
            this.StreamDetails = new ObservableCollection<CompareData>();
            this.StreamDetails.Add(new CompareData(dataTypeIdKey, leftDataTypeId, rightDataTypeId));
            this.StreamDetails.Add(new CompareData(semanticIdKey, leftSemanticId, rightSemanticId));
            this.StreamDetails.Add(new CompareData(eventCountKey, leftEventCount, rightEventCount));
            this.StreamDetails.Add(new CompareData(startTimeKey, leftStartTime, rightStartTime));
            this.StreamDetails.Add(new CompareData(endTimeKey, leftEndTime, rightEndTime));

            bool sameDetails = this.StreamDetails.All(data => data.Same);

            this.StreamDetailsCompareText = string.Format(Strings.StreamDetailsHeader, sameDetails ? Strings.Same : Strings.Different);

            this.Same = (samePublicMetadata && samePersonalMetadata && sameDetails) ? true : false;
        }

        /// <summary> Gets a value indicating whether two streams are identical </summary>
        public bool Same { get; private set; }

        /// <summary> Gets the number of public metadata items associated with the left stream </summary>
        public int LeftPublicMetadataCount { get; private set; }

        /// <summary> Gets the number of personal metadata items associated with the left stream </summary>
        public int LeftPersonalMetadataCount { get; private set; }

        /// <summary> Gets the number of public metadata items associated with the right stream </summary>
        public int RightPublicMetadataCount { get; private set; }

        /// <summary> Gets the number of personal metadata items associated with the right stream </summary>
        public int RightPersonalMetadataCount { get; private set; }
        
        /// <summary> Gets a string which represents the comparison result of two StreamData objects at the detail level </summary>
        public string StreamDetailsCompareText { get; private set; }
        
        /// <summary> Gets a string which represents the comparison result for public metadata between two StreamData objects </summary>
        public string StreamPublicMetadataCompareText { get; private set; }
        
        /// <summary>  Gets a string which represents the comparison result for personal metadata between two StreamData objects </summary>
        public string StreamPersonalMetadataCompareText { get; private set; }
        
        /// <summary>  Gets a string which represents the stream that is being compared </summary>
        public string StreamName { get; private set; }
        
        /// <summary> Gets a list of CompareData objects which represent details within the stream </summary>
        public ObservableCollection<CompareData> StreamDetails { get; private set; }
        
        /// <summary> Gets a list of CompareData objects which represent public metadata within the stream </summary>
        public ObservableCollection<CompareData> PublicMetadata { get; private set; }
        
        /// <summary> Gets a list of CompareData objects which represent personal metadata within the stream </summary>
        public ObservableCollection<CompareData> PersonalMetadata { get; private set; }
    }
}
