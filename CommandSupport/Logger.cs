//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System;
    using System.IO;

    /// <summary>
    /// Logger class for writing strings to a text file, can be used during command-line operations.
    /// </summary>
    public static class Logger
    {
        /// <summary> StreamWriter object which represents the current log </summary>
        private static StreamWriter logger = null;

        /// <summary> Object to lock for thread-safety </summary>
        private static object lockObject = new object();

        /// <summary> Gets a value indicating whether or not the logger is active </summary>
        public static bool IsRunning
        {
            get
            {
                return logger != null;
            }
        }

        /// <summary>
        /// Initializes the logger and prepares the file to accept input
        /// </summary>
        /// <param name="path">Path of file to log output to</param>
        /// <param name="overwrite">True, if file contents should be overwritten by the logger</param>
        public static void Start(string path, bool overwrite)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (logger != null)
            {
                return;
            }

            lock (lockObject)
            {
                if (overwrite)
                {
                    logger = File.CreateText(path);
                }
                else
                {
                    logger = File.AppendText(path);
                }
            }
        }

        /// <summary>
        /// Writes text to the log file
        /// </summary>
        /// <param name="toLog">String to write</param>
        public static void Log(string toLog)
        {
            if (!string.IsNullOrEmpty(toLog))
            {
                lock (lockObject)
                {
                    logger.Write(toLog);
                }
            }
        }

        /// <summary>
        /// Closes the log file and uninitializes the logger
        /// </summary>
        public static void Stop()
        {
            if (logger != null)
            {
                lock (lockObject)
                {
                    logger.Close();
                    logger = null;
                }
            }
        }
    }
}
