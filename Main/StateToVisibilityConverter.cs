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
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Allows conversion of State enumerator to Visibility enumerator when binding to the UI
    /// </summary>
    public class StateToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a State value to a Visibility value for displaying controls in MainWindow
        /// </summary>
        /// <param name="value">The State enumerator value to convert</param>
        /// <param name="targetType">targetType is not used</param>
        /// <param name="parameter">The expected State enumerator value</param>
        /// <param name="culture">culture is not used</param>
        /// <returns>Visible, if value equals parameter; Hidden, otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || !(value is MainWindow.State))
            {
                throw new InvalidOperationException(Strings.ErrorInvalidConversion);
            }

            string currentState = value.ToString();
            string requiredState = parameter.ToString();

            return currentState.Equals(requiredState) ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// NOT Implemented
        /// </summary>
        /// <param name="value">value is not used</param>
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
