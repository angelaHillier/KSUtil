//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    /// <summary>
    /// Compares two string items from two different event files
    /// Supporting class for CompareFileData.cs
    /// </summary>
    public sealed class CompareData
    {
        /// <summary>
        /// Initializes a new instance of the CompareData class
        /// </summary>
        public CompareData()
        {
            this.Key = string.Empty;
            this.LeftValue = string.Empty;
            this.RightValue = string.Empty;
            this.Same = true;
        }

        /// <summary>
        /// Initializes a new instance of the CompareData class and sets object properties based on the values provided
        /// </summary>
        /// <param name="key">Unique identifier for the comparison object</param>
        /// <param name="leftValue">First value to use in comparison</param>
        /// <param name="rightValue">Second value to use in comparison</param>
        public CompareData(string key, string leftValue, string rightValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                this.Key = Strings.Unknown;
            }
            else
            {
                this.Key = key;
            }

            if (string.IsNullOrEmpty(leftValue))
            {
                this.LeftValue = " ";
            }
            else
            {
                this.LeftValue = leftValue;
            }

            if (string.IsNullOrEmpty(rightValue))
            {
                this.RightValue = " ";
            }
            else
            {
                this.RightValue = rightValue;
            }

            this.Same = leftValue.Equals(rightValue);
        }

        /// <summary>
        /// Gets an identifier for the values being compared
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets or sets the first value to use for comparison
        /// </summary>
        public string LeftValue { get; set; }

        /// <summary>
        /// Gets or sets the second value to use for comparison
        /// </summary>
        public string RightValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the left and right values are identical
        /// </summary>
        public bool Same { get; set; }
    }
}
