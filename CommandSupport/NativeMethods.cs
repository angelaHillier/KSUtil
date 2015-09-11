//// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
//// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
//// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
//// PARTICULAR PURPOSE. 
//// 
//// Copyright (c) Microsoft Corporation. All rights reserved.

namespace KSUtil
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains native methods used for command-line processing
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary> Parent process </summary>
        private const int ATTACH_PARENT_PROCESS = -1;
        
        /// <summary>
        /// Attaches to a console window
        /// </summary>
        /// <param name="dwProcessId">process ID of console to attach to</param>
        /// <returns>True if attachment was successful, false otherwise</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        /// Attaches to the parent process to redirect output to the console window
        /// </summary>
        internal static void AttachToCommandPrompt()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }
    }
}
