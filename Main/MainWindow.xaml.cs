//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Microsoft.Kinect.Tools;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for the MainWindow
    /// Handles all UI operations, including View and Compare Files.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary> KStudioClient object which is used to connect to the Kinect service and open event files </summary>
        private KStudioClient client = null;

        /// <summary> KStudioEventFile object, the file currently opened for view or the first file used for comparison </summary>
        private KStudioEventFile eventFile = null;
        
        /// <summary> FileData object which represents all information gathered from an event file </summary>
        private FileData fileData = null;
        
        /// <summary> Represents the current state of the UI </summary>
        private State currentState = State.Welcome;

        /// <summary> Wait message text </summary>
        private string waitText = string.Empty;

        /// <summary> Error message text </summary>
        private string errorText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.client = KStudio.CreateClient();
            this.DataContext = this;
        }

        /// <summary> PropertyChanged event </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Enumeration of different UI states
        /// </summary>
        public enum State
        {
            /// <summary> View state for when the UI is displaying FileData results </summary>
            ViewFile,
            
            /// <summary> Compare state, for when the UI is displaying CompareFileData results </summary>
            Compare,

            /// <summary> Error state, for when the UI is displaying an error message to the user </summary>
            Error,

            /// <summary> Wait state, for when the UI is displaying a wait message to the user </summary>
            Wait,
            
            /// <summary> Welcome state, default/starting state </summary>
            Welcome
        }

        /// <summary>
        /// Gets or sets the visual state of the UI
        /// </summary>
        public State CurrentState
        {
            get
            {
                return this.currentState;
            }

            set
            {
                if (this.currentState != value)
                {
                    this.currentState = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for the WaitMessage control
        /// </summary>
        public string WaitMessageText
        {
            get
            {
                return this.waitText;
            }

            set
            {
                if (!this.waitText.Equals(value))
                {
                    this.waitText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for the ErrorMessage control
        /// </summary>
        public string ErrorMessageText
        {
            get
            {
                return this.errorText;
            }

            set
            {
                if (!this.waitText.Equals(value))
                {
                    this.waitText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Handles the click event for the View File button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">routed event</param>
        private void ViewFile_Click(object sender, RoutedEventArgs e)
        {
            State previous = this.currentState;
            this.WaitMessageText = Strings.WaitForViewFile;
            this.CurrentState = State.Wait;

            try
            {
                string filePath = this.OpenFileForInspection();

                if (!string.IsNullOrEmpty(filePath))
                {
                    this.eventFile = this.client.OpenEventFile(filePath);
                    this.fileData = new FileData(this.eventFile);
                    this.ViewFileDataGrid.DataContext = this.fileData;
                    
                    // show the data
                    this.CurrentState = State.ViewFile;
                }
                else
                {
                    // user canceled the OpenFileDialog, return to previous state
                    this.CurrentState = previous;
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessageText = string.Format(Strings.ErrorLoadFileFailed, ex.Message);
                this.CurrentState = State.Error;
            }
        }

        /// <summary>
        /// Handles the click event for the Exit button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">routed event</param>
        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            if (this.eventFile != null)
            {
                this.eventFile.Dispose();
                this.eventFile = null;
            }

            this.fileData = null;
            this.CurrentState = State.Welcome;
        }

        /// <summary>
        /// Handles the click event for the Exit button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">routed event</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (this.eventFile != null)
            {
                this.eventFile.Dispose();
                this.eventFile = null;
            }

            this.Close();
        }

        /// <summary>
        /// Launches the OpenFileDialog window to help user find/select an event file for inspection
        /// </summary>
        /// <returns>Path to the event file selected by the user</returns>
        private string OpenFileForInspection()
        {
            string fileName = string.Empty;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = Strings.EventFileDescription; // Default file name
            dlg.DefaultExt = Strings.XefExtension; // Default file extension
            dlg.Filter = Strings.EventFileDescription + " " + Strings.EventFileFilter; // Filter files by extension 
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                fileName = dlg.FileName;
            }

            return fileName;
        }

        /// <summary>
        /// Handles the click event for the 'Compare Files' button
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">routed event args</param>
        private void CompareFiles_Click(object sender, RoutedEventArgs e)
        {
            State previous = this.currentState;
            this.WaitMessageText = Strings.WaitForCompareFiles;
            this.CurrentState = State.Wait;
            
            string file1 = string.Empty;
            string file2 = string.Empty;
            KStudioEventFile eventFile2 = null;

            try
            {
                // open the first file, if not already opened for View
                if (this.fileData == null)
                {
                    // open a new file
                    file1 = this.OpenFileForInspection();

                    // if user cancels from OpenFileDialog, return to previous state
                    if (string.IsNullOrEmpty(file1))
                    {
                        this.CurrentState = previous;
                        return;
                    }
                }

                // open a second file to compare with the first
                file2 = this.OpenFileForInspection();

                // if the user cancels from OpenFileDialog, return to the previous state
                if (string.IsNullOrEmpty(file2))
                {
                    this.CurrentState = previous;
                    return;
                }

                // create a FileData object for the first file, unless one already exists
                if (this.fileData == null)
                {
                    this.eventFile = this.client.OpenEventFile(file1);
                    this.fileData = new FileData(this.eventFile);
                }

                // create a second FileData object for comparison
                eventFile2 = this.client.OpenEventFile(file2);
                FileData fileData2 = new FileData(eventFile2);

                // compare data between files
                CompareFileData compareData = new CompareFileData(this.fileData, fileData2);
                this.ViewCompareFileDataGrid.DataContext = compareData;

                // show comparison data
                this.CurrentState = State.Compare;
            }
            catch (Exception ex)
            {
                this.ErrorMessageText = string.Format(Strings.ErrorCompareFilesFailed, ex.Message);
                this.CurrentState = State.Error;
            }
            finally
            {
                if (eventFile2 != null)
                {
                    eventFile2.Dispose();
                    eventFile2 = null;
                }
            }
        }
    }
}
