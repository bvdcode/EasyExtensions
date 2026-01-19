// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EasyExtensions.Windows
{
    /// <summary>
    /// Windows API static methods.
    /// </summary>
    public static class WinApi
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;

            [MarshalAs(UnmanagedType.U4)]
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;

            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        private const int FO_DELETE = 0x0003;
        private const int FOF_ALLOWUNDO = 0x0040; // Preserve undo information, if possible.
        private const int FOF_NOCONFIRMATION = 0x0010; // Show no confirmation dialog box to the user

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        internal static bool DeleteFileOrFolder(string path)
        {
            SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT
            {
                wFunc = FO_DELETE,
                pFrom = path + '\0' + '\0',
                fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION
            };
            var rc = SHFileOperation(ref fileop);
            return rc == 0;
        }

        /// <summary>
        /// Create a shortcut file.
        /// </summary>
        /// <param name="targetFile">Target file path.</param>
        /// <param name="linkFile">Link file path.</param>
        /// <param name="iconFile">Icon file path.</param>
        /// <param name="workingDirectory">Working directory path.</param>
        /// <exception cref="FileNotFoundException">Thrown when target file or icon file not found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when working directory not found.</exception>
        /// <exception cref="IOException">Thrown when cannot create link file at specified location.</exception>
        /// <exception cref="FormatException">Thrown when link file must have .lnk extension.</exception>
        public static void CreateLink(string targetFile, string linkFile, string? iconFile = null, string? workingDirectory = null)
        {
            FileInfo fileInfo = new FileInfo(targetFile);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Target file not found.", targetFile);
            }
            if (!linkFile.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("Link file must have .lnk extension.");
            }
            try
            {
                File.WriteAllText(linkFile, string.Empty);
                File.Delete(linkFile);
            }
            catch (Exception ex)
            {
                throw new IOException("Cannot create link file at specified location.", ex);
            }
            if (iconFile != null)
            {
                if (!File.Exists(iconFile))
                {
                    throw new FileNotFoundException("Icon file not found.", iconFile);
                }
            }
            if (workingDirectory != null)
            {
                if (!Directory.Exists(workingDirectory))
                {
                    throw new DirectoryNotFoundException("Working directory not found.");
                }
            }

            workingDirectory ??= fileInfo.DirectoryName;
            iconFile ??= targetFile;

            ShellLink sl = new ShellLink
            {
                Target = targetFile,
                IconPath = iconFile,
                ShortcutFile = linkFile,
                WorkingDirectory = workingDirectory
            };
            sl.Save();
        }
    }
}
