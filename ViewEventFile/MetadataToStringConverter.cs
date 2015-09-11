//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Allows conversion of metadata values to strings when binding to the UI
    /// </summary>
    public class MetadataToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a metadata value to a string for display in the UI
        /// </summary>
        /// <param name="value">The item to convert</param>
        /// <param name="targetType">targetType is not used</param>
        /// <param name="parameter">parameter is not used</param>
        /// <param name="culture">culture is not used</param>
        /// <returns>A string representing the value provided</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return Metadata.ConvertMetadataValueToString(value);
        }

        /// <summary>
        /// NOT Implemented
        /// </summary>
        /// <param name="value">The parameter is not used</param>
        /// <param name="targetType">targetType is not used</param>
        /// <param name="parameter">parameter is not used</param>
        /// <param name="culture">culture is not used</param>
        /// <returns>not used</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
