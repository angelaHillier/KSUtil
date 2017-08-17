﻿//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using Microsoft.Kinect.Tools;

    /// <summary>
    /// Interaction logic for the App
    /// Handles all command-line operations and initializes the UI
    /// </summary>
    public partial class App : Application
    {
        /// <summary> Command-line prefixes which indicate a parameter to be processed  </summary>
        private char[] argPrefixes = { '-', '/' };
        
        /// <summary> Value indicating if output will be sent to the command line </summary>
        private bool runningCommandLine = false;

        /// <summary>
        /// Enumeration types for command results
        /// </summary>
        private enum CommandLineResult
        {
            /// <summary> Invalid command operation </summary>
            Invalid,
            
            /// <summary> Failed command operation </summary>
            Failed,
            
            /// <summary> Succeeded command operation, launch UI </summary>
            Succeeded,
            
            /// <summary> Succeeded command operation, do not launch UI </summary>
            SucceededExit
        }

        /// <summary>
        /// Start the App, processes command line args and/or launches the UI
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event args</param>
        private void App_Startup(object sender, StartupEventArgs e)
        {
            CommandLineResult commandLineResult = this.ProcessCommandLine(e.Args);

            switch (commandLineResult)
            {
                case CommandLineResult.Failed:
                    this.RunCommandLine();
                    this.Shutdown(-1);
                    break;
                case CommandLineResult.Invalid:
                    this.RunCommandLine();
                    this.ShowCommandLineHelp();
                    this.Shutdown(-1);
                    break;
                case CommandLineResult.SucceededExit:
                    this.Shutdown();
                    break;
                case CommandLineResult.Succeeded:
                    var main = new MainWindow();
                    main.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    main.Show();
                    break;
            }
        }

        /// <summary>
        /// Closes the App
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event args</param>
        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (this.runningCommandLine)
            {
                // AttachConsole() will fail to show a prompt when complete, so display one.
                Console.WriteLine();
                Console.Write(">");
            }
        }

        /// <summary>
        /// Checks to see if the command line parameter is a supported argument
        /// </summary>
        /// <param name="arg">String in question</param>
        /// <returns>True, if the string is a supported argument</returns>
        private bool IsArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
            {
                return false;
            }

            bool result = false;
            
            foreach (char p in this.argPrefixes)
            {
                if (arg.StartsWith(p.ToString()))
                {
                    if (!arg.Substring(0, 2).Equals("//"))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Handles all command-line arguments
        /// </summary>
        /// <param name="args">Arguments array</param>
        /// <returns>CommandLineResult to indicate if the operation was successful or not</returns>
        private CommandLineResult ProcessCommandLine(string[] args)
        {
            string logFile = string.Empty;
            List<string> streamList = new List<string>();
            bool updatePersonalMetadata = false;
            uint loopCount = 0;
			TimeSpan StartingRelativeTime = TimeSpan.MinValue;
			TimeSpan EndingRelativeTime = TimeSpan.MinValue;
			string StartingMetadataKey = "";
			string EndingMetadataKey = "";

			Dictionary<string, List<string>> commands = new Dictionary<string, List<string>>();

            try
            {
                if (args == null)
                {
                    return CommandLineResult.Succeeded;
                }

                if (args.Length == 0)
                {
                    return CommandLineResult.Succeeded;
                }

                // gather commands and support parameters
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (this.IsArgument(arg))
                    {
                        string key = arg.Substring(1).ToUpperInvariant();

                        if (!commands.ContainsKey(key))
                        {
                            commands.Add(key, new List<string>());

                            for (int j = i + 1; j < args.Length; j++)
                            {
                                string param = args[j];
                                if (!this.IsArgument(param))
                                {
                                    commands[key].Add(param);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                // process all command args
                if (commands.ContainsKey("?") || commands.ContainsKey(Strings.Command_Help))
                {
                    this.RunCommandLine();
                    this.ShowCommandLineHelp();
                    return CommandLineResult.SucceededExit;
                }

                // -log <filename>
                if (commands.ContainsKey(Strings.Command_Log))
                {
                    if (commands[Strings.Command_Log].Count != 1)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Log));
                        return CommandLineResult.Invalid;
                    }

                    logFile = commands[Strings.Command_Log][0];
                }

				// -metaspan <start> <end>
				if (commands.ContainsKey(Strings.Command_MetaSpan))
				{
					if (commands[Strings.Command_MetaSpan].Count != 2)
					{
						Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_MetaSpan));
						return CommandLineResult.Invalid;
					}

					if (string.IsNullOrEmpty(commands[Strings.Command_MetaSpan][0]))
					{
						Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_MetaSpan));
						return CommandLineResult.Invalid;
					}
					else
					{
						//StartingRelativeTime
						StartingMetadataKey = commands[Strings.Command_MetaSpan][0];
					}

					if (string.IsNullOrEmpty(commands[Strings.Command_MetaSpan][1]))
					{
						Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_MetaSpan));
						return CommandLineResult.Invalid;
					}
					else
					{
						//EndingRelativeTime
						EndingMetadataKey = commands[Strings.Command_MetaSpan][1];
					}
				}

				// -span <start> <end>
				if (commands.ContainsKey(Strings.Command_Span))
				{
					if (commands[Strings.Command_Span].Count != 2)
					{
						Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Span));
						return CommandLineResult.Invalid;
					}

					if (!TimeSpan.TryParse(commands[Strings.Command_Span][0], out StartingRelativeTime))
					{
						Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Span));
						return CommandLineResult.Invalid;
					}

					if (!TimeSpan.TryParse(commands[Strings.Command_Span][1], out EndingRelativeTime))
					{
						Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Span));
						return CommandLineResult.Invalid;
					}
				}

				// -loop <count>
				if (commands.ContainsKey(Strings.Command_Loop))
                {
                    if (commands[Strings.Command_Loop].Count != 1)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Loop));
                        return CommandLineResult.Invalid;
                    }

                    if (!uint.TryParse(commands[Strings.Command_Loop][0], out loopCount))
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Loop));
                        return CommandLineResult.Invalid;
                    }
                }

                // -pii
                if (commands.ContainsKey(Strings.Command_PII))
                {
                    updatePersonalMetadata = true;
                }

                // -stream <stream1> <stream2> <stream3> ...
                if (commands.ContainsKey(Strings.Command_Stream))
                {
                    if (commands[Strings.Command_Stream].Count == 0)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Stream));
                        return CommandLineResult.Invalid;
                    }

                    streamList = commands[Strings.Command_Stream];
                }

                // -view <filename>
                if (commands.ContainsKey(Strings.Command_View))
                {
                    this.RunCommandLine();
                    if (commands[Strings.Command_View].Count != 1)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_View));
                        return CommandLineResult.Invalid;
                    }

                    string fileInfo = string.Empty;
                    string filePath = commands[Strings.Command_View][0];
                    this.CheckFile(filePath);
                    Console.WriteLine(Strings.WaitForViewFile);

                    using (KStudioClient client = KStudio.CreateClient())
                    {
                        if (filePath.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()))
                        {
                            client.ConnectToService();
                        }
						FileInfo filePath_fi = new FileInfo(filePath);
						using (KStudioEventFile eventFile = client.OpenEventFile(filePath_fi.FullName))
                        {
                            FileData data = new FileData(eventFile);
                            fileInfo = data.GetFileDataAsText();
                            Console.WriteLine(fileInfo);
                        }
                    }

                    if (!string.IsNullOrEmpty(logFile))
                    {
                        Logger.Start(logFile, true);
                        Logger.Log(fileInfo);
                        Logger.Stop();
                    }

                    Console.WriteLine(Strings.Done);
                    return CommandLineResult.SucceededExit;
                }

                // -compare <filename1> <filename2>
                if (commands.ContainsKey(Strings.Command_Compare))
                {
                    this.RunCommandLine();
                    if (commands[Strings.Command_Compare].Count != 2)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Compare));
                        return CommandLineResult.Invalid;
                    }

                    string fileInfo = string.Empty;
                    string file1 = commands[Strings.Command_Compare][0];
                    string file2 = commands[Strings.Command_Compare][1];
                    this.CheckFile(file1);
                    this.CheckFile(file2);
                    Console.WriteLine(Strings.WaitForCompareFiles);

                    using (KStudioClient client = KStudio.CreateClient())
                    {
                        if (file1.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()) || file2.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()))
                        {
                            client.ConnectToService();
                        }

						FileInfo filePath1_fi = new FileInfo(file1);
						FileInfo filePath2_fi = new FileInfo(file2);

						using (KStudioEventFile eventFile1 = client.OpenEventFile(filePath1_fi.FullName))
                        using (KStudioEventFile eventFile2 = client.OpenEventFile(filePath2_fi.FullName))
                        {
                            FileData fileData1 = new FileData(eventFile1);
                            FileData fileData2 = new FileData(eventFile2);
                            CompareFileData compareData = new CompareFileData(fileData1, fileData2);
                            fileInfo = compareData.GetCompareFileDataAsText();
                            Console.WriteLine(fileInfo);
                        }
                    }

                    if (!string.IsNullOrEmpty(logFile))
                    {
                        Logger.Start(logFile, true);
                        Logger.Log(fileInfo);
                        Logger.Stop();
                    }

                    Console.WriteLine(Strings.Done);
                    return CommandLineResult.SucceededExit;
                }

                // -update <filename> <metadata key> <metadata value>
                if (commands.ContainsKey(Strings.Command_Update))
                {
                    this.RunCommandLine();
                    if (commands[Strings.Command_Update].Count != 3)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Update));
                        return CommandLineResult.Invalid;
                    }

                    string filePath = commands[Strings.Command_Update][0];
                    string key = commands[Strings.Command_Update][1];
                    object value = commands[Strings.Command_Update][2];
                    this.CheckFile(filePath);
                    string metadataText = string.Empty;

					FileInfo filePath_fi = new FileInfo(filePath);

                    if (streamList.Count > 0)
                    {
                        // update stream metadata
                        foreach (string streamName in streamList)
                        {
                            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Strings.UpdatingStreamMetadata, streamName));
                            metadataText = this.EditMetadata(filePath_fi.FullName, key, value, streamName, updatePersonalMetadata, true);
                            Console.WriteLine(metadataText);
                        }
                    }
                    else
                    {
                        // update file metadata
                        Console.WriteLine(Strings.UpdatingFileMetadata);
                        metadataText = this.EditMetadata(filePath_fi.FullName, key, value, string.Empty, updatePersonalMetadata, false);
                        Console.WriteLine(metadataText);
                    }

                    Console.WriteLine(Strings.Done);
                    return CommandLineResult.SucceededExit;
                }

                // -remove <filename> <metadata key>
                if (commands.ContainsKey(Strings.Command_Remove))
                {
                    this.RunCommandLine();
                    if (commands[Strings.Command_Remove].Count != 2)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Remove));
                        return CommandLineResult.Invalid;
                    }

                    string filePath = commands[Strings.Command_Remove][0];
                    string key = commands[Strings.Command_Remove][1];
                    this.CheckFile(filePath);
                    string metadataText = string.Empty;
					FileInfo filePath_fi = new FileInfo(filePath);

					if (streamList.Count > 0)
                    {
                        // update stream metadata
                        foreach (string streamName in streamList)
                        {
                            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Strings.UpdatingStreamMetadata, streamName));
                            metadataText = this.EditMetadata(filePath_fi.FullName, key, null, streamName, updatePersonalMetadata, true);
                            Console.WriteLine(metadataText);
                        }
                    }
                    else
                    {
                        // update file metadata
                        Console.WriteLine(Strings.UpdatingFileMetadata);
                        metadataText = this.EditMetadata(filePath_fi.FullName, key, null, string.Empty, updatePersonalMetadata, false);
                        Console.WriteLine(metadataText);
                    }

                    Console.WriteLine(Strings.Done);
                    return CommandLineResult.SucceededExit;
                }

                // -play <filename>
                if (commands.ContainsKey(Strings.Command_Play))
                {
                    this.RunCommandLine();
                    if (commands[Strings.Command_Play].Count != 1)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Play));
                        return CommandLineResult.Invalid;
                    }

                    string filePath = commands[Strings.Command_Play][0];
                    this.CheckFile(filePath);
					FileInfo filePath_fi = new FileInfo(filePath);

					if(!string.IsNullOrEmpty(StartingMetadataKey) && !string.IsNullOrEmpty(EndingMetadataKey))
					{
						object start_metadata_value = null;
						string start_metadata_value_str = QueryMetadata(filePath_fi.FullName, StartingMetadataKey, null, true, false, out start_metadata_value);

						object end_metadata_value = null;
						string end_metadata_value_str = QueryMetadata(filePath_fi.FullName, EndingMetadataKey, null, true, false, out end_metadata_value);

						if(start_metadata_value == null || start_metadata_value.GetType() != typeof(TimeSpan))
						{
							Console.Error.WriteLine(string.Format(Strings.ErrorInvalidMetadataPair, Strings.Command_Play));
							return CommandLineResult.Invalid;
						}

						if (end_metadata_value == null || end_metadata_value.GetType() != typeof(TimeSpan))
						{
							Console.Error.WriteLine(string.Format(Strings.ErrorInvalidMetadataPair, Strings.Command_Play));
							return CommandLineResult.Invalid;
						}

						StartingRelativeTime = (TimeSpan)start_metadata_value;
						EndingRelativeTime = (TimeSpan)end_metadata_value;
					}

					using (KStudioClient client = KStudio.CreateClient())
                    {
                        Console.WriteLine(Strings.WaitToConnect);
                        client.ConnectToService();
                        
                        Console.WriteLine(Strings.StartPlayback);
                        Playback.PlaybackClip(client, filePath_fi.FullName, streamList, loopCount,StartingRelativeTime,EndingRelativeTime);
                        Console.WriteLine(Strings.StopPlayback);
                        
                        client.DisconnectFromService();
                    }

                    Console.WriteLine(Strings.Done);
                    return CommandLineResult.SucceededExit;
                }

                // -record <filename> <duration>
                if (commands.ContainsKey(Strings.Command_Record))
                {
                    this.RunCommandLine();
                    if (commands[Strings.Command_Record].Count != 2)
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Record));
                        return CommandLineResult.Invalid;
                    }

                    string filePath = commands[Strings.Command_Record][0];
                    this.CheckDirectory(filePath);
					FileInfo filePath_fi = new FileInfo(filePath);

					double time = 0;
                    if (!double.TryParse(commands[Strings.Command_Record][1], out time))
                    {
                        Console.Error.WriteLine(string.Format(Strings.ErrorInvalidArgs, Strings.Command_Record));
                        return CommandLineResult.Invalid;
                    }

                    TimeSpan duration = TimeSpan.FromSeconds(time);
                    string errorMsg = string.Empty;
                    string fileInfo = string.Empty;

                    using (KStudioClient client = KStudio.CreateClient())
                    {
                        Console.WriteLine(Strings.WaitToConnect);
                        client.ConnectToService();

                        Console.WriteLine(Strings.StartRecording);
                        Recording.RecordClip(client, filePath_fi.FullName, duration, streamList);
                        Console.WriteLine(Strings.StopRecording);

                        using (KStudioEventFile eventFile = client.OpenEventFile(filePath_fi.FullName))
                        {
                            FileData fileData = new FileData(eventFile);
                            fileInfo = fileData.GetFileDataAsText();
                            Console.WriteLine(fileInfo);
                        }                        
                        
                        client.DisconnectFromService();
                    }

                    if (!string.IsNullOrEmpty(logFile))
                    {
                        Logger.Start(logFile, true);
                        Logger.Log(fileInfo);
                        Logger.Stop();
                    }

                    Console.WriteLine(Strings.Done);                    
                    return CommandLineResult.SucceededExit;
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format(Strings.ErrorPrepend, ex.Message);
                Console.Error.WriteLine(errorMsg);
                return CommandLineResult.Failed;
            }

            return CommandLineResult.Invalid;
        }

        /// <summary>
        /// Outputs help text for the user
        /// </summary>
        private void ShowCommandLineHelp()
        {
            Console.WriteLine(Strings.CommandLineHelp);
        }

        /// <summary>
        /// Attaches to the parent console
        /// </summary>
        private void RunCommandLine()
        {
            this.runningCommandLine = true;
            NativeMethods.AttachToCommandPrompt();
            Console.WriteLine();
        }

        /// <summary>
        /// Verifies that an event file is valid
        /// </summary>
        /// <param name="filePath">Path to event file</param>
        private void CheckFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException(Strings.ErrorInvalidPath);
            }

            if (!filePath.ToUpperInvariant().EndsWith(Strings.XefExtension.ToUpperInvariant()) && !filePath.ToUpperInvariant().EndsWith(Strings.XrfExtension.ToUpperInvariant()))
            {
                throw new InvalidOperationException(Strings.ErrorInvalidFileExtension);
            }

            if (!filePath.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()))
            {
                if (!File.Exists(filePath))
                {
                    throw new InvalidOperationException(Strings.ErrorInvalidFile);
                }
            }
        }

        /// <summary>
        /// Verifies that a file's directory is valid
        /// </summary>
        /// <param name="filePath">Path to event file</param>
        private void CheckDirectory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException(Strings.ErrorInvalidPath);
            }

            if (!filePath.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()))
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    throw new InvalidOperationException(Strings.ErrorInvalidDirectory);
                }
            }
        }

        /// <summary>
        /// Edits file or stream-level metadata
        /// </summary>
        /// <param name="filePath">Path of file which contains metadata to edit</param>
        /// <param name="key">Key of metadata item to alter</param>
        /// <param name="value">New value to set for the metadata item</param>
        /// <param name="streamName">String which represents the stream to alter metadata for</param>
        /// <param name="updatePersonalMetadata">Value which indicates if personal metadata should be altered (default is public)</param>
        /// <param name="updateStreamMetadata">Value which indicates if stream metadata should be altered (default is file)</param>
        /// <returns>String containing updated contents of the target metadata object</returns>
        private string EditMetadata(string filePath, string key, object value, string streamName, bool updatePersonalMetadata, bool updateStreamMetadata)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            string metadataText = string.Empty;

            using (KStudioClient client = KStudio.CreateClient())
            {
                if (filePath.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()))
                {
                    client.ConnectToService();
                }

                KStudioMetadataType type = KStudioMetadataType.Public;
                if (updatePersonalMetadata)
                {
                    type = KStudioMetadataType.PersonallyIdentifiableInformation;
                }

                if (updateStreamMetadata)
                {
                    metadataText = Metadata.UpdateStreamMetadata(client, filePath, streamName, type, key, value);
                }
                else
                {
                    metadataText = Metadata.UpdateFileMetadata(client, filePath, type, key, value);
                }
            }

            return metadataText;
        }

		/// <summary>
		/// Queries file or stream-level metadata
		/// </summary>
		/// <param name="filePath">Path of file which contains metadata to query</param>
		/// <param name="key">Key of metadata item to query</param>
		/// <param name="value">New value to set for the metadata item</param>
		/// <param name="streamName">String which represents the stream to query metadata from</param>
		/// <param name="isPersonalMetadata">Value which indicates if the key being queried is categorized as personal metadata (default is personal)</param>
		/// <param name="isStreamMetadata">Value which indicates if the key being queried is categorized as stream metadata (default is file)</param>
		/// <returns>String containing contents of the target metadata object</returns>
		private string QueryMetadata(string filePath, string key, string streamName, bool isPersonalMetadata, bool isStreamMetadata,out object value)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}

			string metadataText = string.Empty;
			value = null;

			using (KStudioClient client = KStudio.CreateClient())
			{
				if (filePath.ToUpperInvariant().StartsWith(Strings.ConsoleClipRepository.ToUpperInvariant()))
				{
					client.ConnectToService();
				}

				KStudioMetadataType type = KStudioMetadataType.Public;
				if (isPersonalMetadata)
				{
					type = KStudioMetadataType.PersonallyIdentifiableInformation;
				}

				if (isStreamMetadata)
				{
					metadataText = Metadata.QueryStreamMetadata(client, filePath, streamName, type, key, out value);
				}
				else
				{
					metadataText = Metadata.QueryFileMetadata(client, filePath, type, key, out value);
				}
			}

			return metadataText;
		}
	}
}
