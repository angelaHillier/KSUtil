//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CompareDataGrid
    /// Shows two comparison objects side-by-side in the UI
    /// Used by the CompareFileDataGrid display in the UI
    /// </summary>
    public partial class CompareDataGrid : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the CompareDataGrid class
        /// </summary>
        public CompareDataGrid()
        {
            this.InitializeComponent();
        }
    }
}
