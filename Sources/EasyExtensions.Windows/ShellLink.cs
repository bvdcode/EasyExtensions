// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyExtensions.Windows
{
    /// <summary>
    /// Summary description for ShellLink.
    /// </summary>
    internal class ShellLink : IDisposable
    {
        #region ComInterop for IShellLink

        [ComImport()]
        [Guid("0000010C-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersist
        {
            [PreserveSig]
            //[helpstring("Returns the class identifier for the component object")]
            void GetClassID(out Guid pClassID);
        }

        #region IPersistFile Interface
        [ComImport()]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            // can't get this to go if I extend IPersist, so put it here:
            [PreserveSig]
            void GetClassID(out Guid pClassID);

            /// <summary>
            /// Checks for changes since last file write
            /// </summary>
            void IsDirty();

            /// <summary>
            /// Opens the specified file and initializes the object from its contents
            /// </summary>
            /// <param name="pszFileName">The file from which the object is to be initialized</param>
            /// <param name="dwMode">The mode to open the file</param>
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

            /// <summary>
            /// Saves the object into the specified file
            /// </summary>
            /// <param name="pszFileName">The file in which to save the object</param>
            /// <param name="fRemember">Specifies whether the file name is to be used in the future as the source for updates</param>
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            /// <summary>
            /// Notifies the object that save is completed
            /// </summary>
            /// <param name="pszFileName">The file in which the object was saved</param>
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            /// <summary>
            /// Gets the current name of the file associated with the object
            /// </summary>
            /// <param name="ppszFileName">The current name of the file associated with the object</param>
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }
        #endregion

        #region IShellLink Interface
        [ComImport()]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkA
        {
            /// <summary>
            /// Retrieves the path and filename of a shell link object
            /// </summary>
            /// <param name="pszFile">Address of a buffer that receives the path and filename of the shell link object</param>
            /// <param name="cchMaxPath">The size, in characters, of the buffer pointed to by the pszFile parameter</param>
            /// <param name="pfd">Address of a WIN32_FIND_DATA structure that receives information about the shell link object</param>
            /// <param name="fFlags">Flags that specify the type of path information to retrieve</param>
            void GetPath([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
                int cchMaxPath, ref WIN32_FIND_DATAA pfd, uint fFlags);

            /// <summary>
            /// Retrieves the list of shell link item identifiers
            /// </summary>
            /// <param name="ppidl">The address of an ITEMIDLIST pointer that receives the list of item identifiers</param>
            void GetIDList(out IntPtr ppidl);

            /// <summary>
            /// Sets the list of shell link item identifiers
            /// </summary>
            /// <param name="pidl">The address of an ITEMIDLIST that specifies the list of item identifiers to be set</param>
            void SetIDList(IntPtr pidl);

            /// <summary>
            /// Retrieves the shell link description string
            /// </summary>
            /// <param name="pszFile">The file where the description is to be stored</param>
            /// <param name="cchMaxName">The size, in characters, of the buffer pointed to by the pszName parameter</param>
            void GetDescription([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxName);

            /// <summary>
            /// Sets the shell link description string
            /// </summary>
            /// <param name="pszName">The new description string</param>
            void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

            /// <summary>
            /// Retrieves the name of the shell link working directory
            /// </summary>
            /// <param name="pszDir">The address of the buffer that receives the working directory</param>
            /// <param name="cchMaxPath">The size, in characters, of the buffer pointed to by the pszDir parameter</param>
            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cchMaxPath);

            /// <summary>
            /// Sets the name of the shell link working directory.
            /// </summary>
            /// <param name="pszDir">The new working directory.</param>
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

            /// <summary>
            /// Retrieves the shell link command-line arguments.
            /// </summary>
            /// <param name="pszArgs">The buffer that receives the command-line arguments.</param>
            /// <param name="cchMaxPath">The size, in characters, of the buffer pointed to by the pszArgs parameter.</param>
            void GetArguments([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cchMaxPath);

            /// <summary>
            /// Sets the shell link command-line arguments.
            /// </summary>
            /// <param name="pszArgs">The new command-line arguments.</param>
            void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            /// <summary>
            /// Retrieves the shell link hot key.
            /// </summary>
            /// <param name="pwHotkey">The address of a variable that receives the hot key.</param>
            void GetHotkey(out short pwHotkey);
            /// <summary>
            /// Sets the shell link hot key.
            /// </summary>
            /// <param name="pwHotkey">The new hot key.</param>
            void SetHotkey(short pwHotkey);

            /// <summary>
            /// Retrieves the shell link show command.
            /// </summary>
            /// <param name="piShowCmd">The address of a variable that receives the show command.</param>
            void GetShowCmd(out uint piShowCmd);

            /// <summary>
            /// Sets the shell link show command.
            /// </summary>
            /// <param name="piShowCmd">The new show command.</param>
            void SetShowCmd(uint piShowCmd);

            /// <summary>
            /// Retrieves the location (path and index) of the shell link icon.
            /// </summary>
            /// <param name="pszIconPath">The buffer that receives the icon path.</param>
            /// <param name="cchIconPath">The size, in characters, of the buffer pointed to by the pszIconPath parameter.</param>
            /// <param name="piIcon">The address of a variable that receives the icon index.</param>
            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            /// <summary>
            /// Sets the location (path and index) of the shell link icon.
            /// </summary>
            /// <param name="pszIconPath">The new icon path.</param>
            /// <param name="iIcon">The new icon index.</param>
            void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

            /// <summary>
            /// Sets the shell link relative path.
            /// </summary>
            /// <param name="pszPathRel">The new relative path.</param>
            /// <param name="dwReserved">Reserved. Must be zero.</param>
            void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);

            /// <summary>
            /// Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary).
            /// </summary>
            /// <param name="hWnd">A handle to the window that the system uses as a parent for any dialog boxes that it displays.</param>
            /// <param name="fFlags">Flags that control the resolution process.</param>
            void Resolve(IntPtr hWnd, uint fFlags);

            /// <summary>
            /// Sets the shell link path and filename.
            /// </summary>
            /// <param name="pszFile">The new path and filename.</param>
            void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }


        /// <summary>
        /// IShellLinkW interface for managing shell links (shortcuts) in Windows.
        /// </summary>
        [ComImport()]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            /// <summary>
            /// Retrieves the path and filename of a shell link object.
            /// </summary>
            /// <param name="pszFile">The buffer that receives the path and filename.</param>
            /// <param name="cchMaxPath">The size, in characters, of the buffer pointed to by the pszFile parameter.</param>
            /// <param name="pfd">The WIN32_FIND_DATA structure that receives information about the shell link object.</param>
            /// <param name="fFlags">Flags that specify the type of path information to retrieve.</param>
            void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, ref WIN32_FIND_DATAW pfd, uint fFlags);

            /// <summary>
            /// Retrieves the list of shell link item identifiers.
            /// </summary>
            /// <param name="ppidl">The address of an ITEMIDLIST pointer that receives the list of item identifiers.</param>
            void GetIDList(out IntPtr ppidl);

            /// <summary>
            /// Sets the list of shell link item identifiers.
            /// </summary>
            /// <param name="pidl">The address of an ITEMIDLIST that specifies the list of item identifiers to be set.</param>
            void SetIDList(IntPtr pidl);

            /// <summary>
            /// Retrieves the shell link description string.
            /// </summary>
            /// <param name="pszFile">The buffer that receives the description string.</param>
            /// <param name="cchMaxName">The size, in characters, of the buffer pointed to by the pszFile parameter.</param>
            void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);

            /// <summary>
            /// Sets the shell link description string.
            /// </summary>
            /// <param name="pszName">The new description string.</param>
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            /// <summary>
            /// Retrieves the name of the shell link working directory.
            /// </summary>
            /// <param name="pszDir">The buffer that receives the working directory.</param>
            /// <param name="cchMaxPath">The size, in characters, of the buffer pointed to by the pszDir parameter.</param>
            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

            /// <summary>
            /// Sets the name of the shell link working directory.
            /// </summary>
            /// <param name="pszDir">The new working directory.</param>
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            /// <summary>
            /// Retrieves the shell link command-line arguments.
            /// </summary>
            /// <param name="pszArgs">The buffer that receives the command-line arguments.</param>
            /// <param name="cchMaxPath">The size, in characters, of the buffer pointed to by the pszArgs parameter.</param>
            void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

            /// <summary>
            /// Sets the shell link command-line arguments.
            /// </summary>
            /// <param name="pszArgs">The new command-line arguments.</param>
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            /// <summary>
            /// Retrieves the shell link hot key.
            /// </summary>
            /// <param name="pwHotkey">The address of a variable that receives the hot key.</param>
            void GetHotkey(out short pwHotkey);

            /// <summary>
            /// Sets the shell link hot key.
            /// </summary>
            /// <param name="pwHotkey">The new hot key.</param>
            void SetHotkey(short pwHotkey);

            /// <summary>
            /// Retrieves the shell link show command.
            /// </summary>
            /// <param name="piShowCmd">The address of a variable that receives the show command.</param>
            void GetShowCmd(out uint piShowCmd);

            /// <summary>
            /// Sets the shell link show command.
            /// </summary>
            /// <param name="piShowCmd">The new show command.</param>
            void SetShowCmd(uint piShowCmd);

            /// <summary>
            /// Retrieves the location (path and index) of the shell link icon.
            /// </summary>
            /// <param name="pszIconPath">The buffer that receives the icon path.</param>
            /// <param name="cchIconPath">The size, in characters, of the buffer pointed to by the pszIconPath parameter.</param>
            /// <param name="piIcon">The address of a variable that receives the icon index.</param>
            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            /// <summary>
            /// Sets the location (path and index) of the shell link icon.
            /// </summary>
            /// <param name="pszIconPath">The new icon path.</param>
            /// <param name="iIcon">The new icon index.</param>
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            /// <summary>
            /// Sets the shell link relative path.
            /// </summary>
            /// <param name="pszPathRel">The new relative path.</param>
            /// <param name="dwReserved">Reserved. Must be zero.</param>
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

            /// <summary>
            /// Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary).
            /// </summary>
            /// <param name="hWnd">A handle to the window that the system uses as a parent for any dialog boxes that it displays.</param>
            /// <param name="fFlags">Flags that control the resolution process.</param>
            void Resolve(IntPtr hWnd, uint fFlags);

            /// <summary>
            /// Sets the shell link path and filename.
            /// </summary>
            /// <param name="pszFile">The new path and filename.</param>
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
        #endregion

        #region ShellLinkCoClass
        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport()]
        private class CShellLink { }

        #endregion

        #region Private IShellLink enumerations
        private enum EShellLinkGP : uint
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2
        }

        [Flags]
        private enum EShowWindowFlags : uint
        {
            SW_HIDE = 0,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
        }
        #endregion

        #region IShellLink Private structs

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATAW
        {
            public WIN32_FIND_DATA_INTERNAL Data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
        private struct WIN32_FIND_DATAA
        {
            public WIN32_FIND_DATA_INTERNAL Data;
        }

        private struct WIN32_FIND_DATA_INTERNAL
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }
        #endregion

        #region Unmanaged Methods
        private class UnmanagedMethods
        {
            [DllImport("Shell32", CharSet = CharSet.Auto)]
            internal static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)]
                string lpszFile, int nIconIndex, IntPtr[]? phIconLarge, IntPtr[]? phIconSmall, int nIcons);

            [DllImport("user32")]
            internal static extern int DestroyIcon(IntPtr hIcon);
        }
        #endregion

        #endregion

        #region Enumerations
        /// <summary>
        /// Flags determining how the links with missing
        /// targets are resolved.
        /// </summary>
        [Flags]
        public enum EShellLinkResolveFlags : uint
        {
            /// <summary>
            /// Allow any match during resolution.  Has no effect
            /// on ME/2000 or above, use the other flags instead.
            /// </summary>
            SLR_ANY_MATCH = 0x2,

            /// <summary>
            /// Call the Microsoft Windows Installer. 
            /// </summary>
            SLR_INVOKE_MSI = 0x80,

            /// <summary>
            /// Disable distributed link tracking. By default, 
            /// distributed link tracking tracks removable media 
            /// across multiple devices based on the volume name. 
            /// It also uses the UNC path to track remote file 
            /// systems whose drive letter has changed. Setting 
            /// SLR_NOLINKINFO disables both types of tracking.
            /// </summary>
            SLR_NOLINKINFO = 0x40,

            /// <summary>
            /// Do not display a dialog box if the link cannot be resolved. 
            /// When SLR_NO_UI is set, a time-out value that specifies the 
            /// maximum amount of time to be spent resolving the link can 
            /// be specified in milliseconds. The function returns if the 
            /// link cannot be resolved within the time-out duration. 
            /// If the timeout is not set, the time-out duration will be 
            /// set to the default value of 3,000 milliseconds (3 seconds). 
            /// </summary>										    
            SLR_NO_UI = 0x1,

            /// <summary>
            /// Not documented in SDK.  Assume same as SLR_NO_UI but 
            /// intended for applications without a hWnd.
            /// </summary>
            SLR_NO_UI_WITH_MSG_PUMP = 0x101,

            /// <summary>
            /// Do not update the link information. 
            /// </summary>
            SLR_NOUPDATE = 0x8,

            /// <summary>
            /// Do not execute the search heuristics. 
            /// </summary>																																																																																																																																																																																																														
            SLR_NOSEARCH = 0x10,

            /// <summary>
            /// Do not use distributed link tracking. 
            /// </summary>
            SLR_NOTRACK = 0x20,

            /// <summary>
            /// If the link object has changed, update its path and list 
            /// of identifiers. If SLR_UPDATE is set, you do not need to 
            /// call IPersistFile::IsDirty to determine whether or not 
            /// the link object has changed. 
            /// </summary>
            SLR_UPDATE = 0x4
        }

        /// <summary>
        /// Window Show Command enumeration
        /// </summary>
        internal enum LinkDisplayMode : uint
        {
            /// <summary>
            /// Show the window in its default state
            /// </summary>
            edmNormal = EShowWindowFlags.SW_NORMAL,

            /// <summary>
            /// Show the window minimized
            /// </summary>
            edmMinimized = EShowWindowFlags.SW_SHOWMINNOACTIVE,

            /// <summary>
            /// Show the window maximized
            /// </summary>
            edmMaximized = EShowWindowFlags.SW_MAXIMIZE
        }
        #endregion

        #region Member Variables
        // Use Unicode (W) under NT, otherwise use ANSI		
        private IShellLinkW? linkW;
        private IShellLinkA? linkA;
        private string shortcutFile = "";
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an instance of the Shell Link object.
        /// </summary>
        public ShellLink()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                linkW = (IShellLinkW)new CShellLink();
            }
            else
            {
                linkA = (IShellLinkA)new CShellLink();
            }
        }

        /// <summary>
        /// Creates an instance of a Shell Link object
        /// from the specified link file
        /// </summary>
        /// <param name="linkFile">The Shortcut file to open</param>
        public ShellLink(string linkFile) : this()
        {
            Open(linkFile);
        }
        #endregion

        #region Destructor and Dispose
        /// <summary>
        /// Call dispose just in case it hasn't happened yet
        /// </summary>
        ~ShellLink()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose the object, releasing the COM ShellLink object
        /// </summary>
        public void Dispose()
        {
            if (linkW != null)
            {
                Marshal.ReleaseComObject(linkW);
                linkW = null;
            }
            if (linkA != null)
            {
                Marshal.ReleaseComObject(linkA);
                linkA = null;
            }
        }
        #endregion

        #region Implementation

        /// <summary>
        /// Opens a shortcut file and initializes the object
        /// </summary>
        public string ShortcutFile
        {
            get
            {
                return shortcutFile;
            }
            set
            {
                shortcutFile = value;
            }
        }

        /// <summary>
        /// Gets a System.Drawing.Icon containing the icon for this
        /// ShellLink object.
        /// </summary>
        public Icon? LargeIcon => GetIcon(true);

        /// <summary>
        /// Gets a System.Drawing.Icon containing the icon for this
        /// ShellLink object.
        /// </summary>
        public Icon? SmallIcon => GetIcon(false);

        private Icon? GetIcon(bool large)
        {
            StringBuilder iconPath = new StringBuilder(260, 260);
            // Get icon index and path:
            int iconIndex;
            if (linkA == null && linkW != null)
            {
                linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
            }
            else if (linkA != null && linkW == null)
            {
                linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
            }
            else
            {
                throw new InvalidOperationException("Unable to get icon location");
            }
            string iconFile = iconPath.ToString();

            // If there are no details set for the icon, then we must use
            // the shell to get the icon for the target:
            if (iconFile.Length == 0)
            {
                return new Icon(iconFile);
            }
            else
            {
                // Use ExtractIconEx to get the icon:
                IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };
                if (large)
                {
                    _ = UnmanagedMethods.ExtractIconEx(iconFile, iconIndex, hIconEx, null, 1);
                }
                else
                {
                    _ = UnmanagedMethods.ExtractIconEx(iconFile, iconIndex, null, hIconEx, 1);
                }
                // If success then return as a GDI+ object
                Icon? icon = null;
                if (hIconEx[0] != IntPtr.Zero)
                {
                    icon = Icon.FromHandle(hIconEx[0]);
                    //UnManagedMethods.DestroyIcon(hIconEx[0]);
                }
                return icon;
            }
        }

        /// <summary>
        /// Gets the path to the file containing the icon for this shortcut.
        /// </summary>
        public string IconPath
        {
            get
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                if (linkA == null && linkW != null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out _);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out _);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get icon location");
                }
                return iconPath.ToString();
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex;
                if (linkA == null && linkW != null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                    linkW.SetIconLocation(value, iconIndex);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                    linkA.SetIconLocation(value, iconIndex);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get icon location");
                }
            }
        }

        /// <summary>
        /// Gets the index of this icon within the icon path's resources
        /// </summary>
        public int IconIndex
        {
            get
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex;
                if (linkA == null && linkW != null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get icon location");
                }
                return iconIndex;
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                if (linkA == null && linkW != null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out _);
                    linkW.SetIconLocation(iconPath.ToString(), value);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out _);
                    linkA.SetIconLocation(iconPath.ToString(), value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the fully qualified path to the link's target
        /// </summary>
        public string Target
        {
            get
            {
                StringBuilder target = new StringBuilder(260, 260);
                if (linkA == null && linkW != null)
                {
                    WIN32_FIND_DATAW fd = new WIN32_FIND_DATAW();
                    fd.Data.ftCreationTime = new FILETIME();
                    fd.Data.ftLastAccessTime = new FILETIME();
                    fd.Data.ftLastWriteTime = new FILETIME();
                    fd.Data.dwFileAttributes = 0;
                    fd.Data.nFileSizeLow = 0;
                    fd.Data.nFileSizeHigh = 0;
                    fd.Data.dwReserved0 = 0;
                    fd.Data.dwReserved1 = 0;
                    fd.Data.cFileName = string.Empty;
                    fd.Data.cAlternateFileName = string.Empty;
                    linkW.GetPath(target, target.Capacity, ref fd, (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                else if (linkA != null && linkW == null)
                {
                    WIN32_FIND_DATAA fd = new WIN32_FIND_DATAA();
                    fd.Data.dwFileAttributes = 0;
                    linkA.GetPath(target, target.Capacity, ref fd, (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get target path");
                }
                return target.ToString();
            }
            set
            {
                if (linkA == null && linkW != null)
                {
                    linkW.SetPath(value);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.SetPath(value);
                }
                else
                {
                    throw new InvalidOperationException("Unable to set target path");
                }
            }
        }

        /// <summary>
        /// Gets/sets the Working Directory for the Link
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                StringBuilder path = new StringBuilder(260, 260);
                if (linkA == null && linkW != null)
                {
                    linkW.GetWorkingDirectory(path, path.Capacity);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetWorkingDirectory(path, path.Capacity);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get working directory");
                }
                return path.ToString();
            }
            set
            {
                if (linkA == null && linkW != null)
                {
                    linkW.SetWorkingDirectory(value);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.SetWorkingDirectory(value);
                }
                else
                {
                    throw new InvalidOperationException("Unable to set working directory");
                }
            }
        }

        /// <summary>
        /// Gets/sets the description of the link
        /// </summary>
        public string Description
        {
            get
            {
                StringBuilder description = new StringBuilder(1024, 1024);
                if (linkA == null && linkW != null)
                {
                    linkW.GetDescription(description, description.Capacity);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetDescription(description, description.Capacity);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get description");
                }
                return description.ToString();
            }
            set
            {
                if (linkA == null && linkW != null)
                {
                    linkW.SetDescription(value);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.SetDescription(value);
                }
                else
                {
                    throw new InvalidOperationException("Unable to set description");
                }
            }
        }

        /// <summary>
        /// Gets/sets any command line arguments associated with the link
        /// </summary>
        public string Arguments
        {
            get
            {
                StringBuilder arguments = new StringBuilder(260, 260);
                if (linkA == null && linkW != null)
                {
                    linkW.GetArguments(arguments, arguments.Capacity);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetArguments(arguments, arguments.Capacity);
                }
                else
                {
                    throw new InvalidOperationException("Unable to get arguments");
                }
                return arguments.ToString();
            }
            set
            {
                if (linkA == null && linkW != null)
                {
                    linkW.SetArguments(value);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.SetArguments(value);
                }
                else
                {
                    throw new InvalidOperationException("Unable to set arguments");
                }
            }
        }

        /// <summary>
        /// Gets/sets the initial display mode when the shortcut is
        /// run
        /// </summary>
        public LinkDisplayMode DisplayMode
        {
            get
            {
                uint cmd = 0;
                if (linkA == null && linkW != null)
                {
                    linkW.GetShowCmd(out cmd);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.GetShowCmd(out cmd);
                }
                return (LinkDisplayMode)cmd;
            }
            set
            {
                if (linkA == null && linkW != null)
                {
                    linkW.SetShowCmd((uint)value);
                }
                else if (linkA != null && linkW == null)
                {
                    linkA.SetShowCmd((uint)value);
                }
            }
        }

        /// <summary>
        /// Saves the shortcut to ShortCutFile.
        /// </summary>
        public void Save()
        {
            Save(shortcutFile);
        }

        /// <summary>
        /// Saves the shortcut to the specified file
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk)</param>
        public void Save(string linkFile)
        {
            // Save the object to disk
            if (linkA == null && linkW != null)
            {
                ((IPersistFile)linkW).Save(linkFile, true);
                shortcutFile = linkFile;
            }
            else if (linkA != null && linkW == null)
            {
                ((IPersistFile)linkA).Save(linkFile, true);
                shortcutFile = linkFile;
            }
            else
            {
                throw new InvalidOperationException("Unable to save shortcut");
            }
        }

        /// <summary>
        /// Loads a shortcut from the specified file
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        public void Open(string linkFile)
        {
            Open(linkFile, IntPtr.Zero, EShellLinkResolveFlags.SLR_ANY_MATCH | EShellLinkResolveFlags.SLR_NO_UI, 1);
        }

        /// <summary>
        /// Loads a shortcut from the specified file, and allows flags controlling
        /// the UI behaviour if the shortcut's target isn't found to be set.
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        /// <param name="hWnd">The window handle of the application's UI, if any</param>
        /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
        public void Open(string linkFile, IntPtr hWnd, EShellLinkResolveFlags resolveFlags)
        {
            Open(linkFile, hWnd, resolveFlags, 1);
        }

        /// <summary>
        /// Loads a shortcut from the specified file, and allows flags controlling
        /// the UI behaviour if the shortcut's target isn't found to be set.  If
        /// no SLR_NO_UI is specified, you can also specify a timeout.
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        /// <param name="hWnd">The window handle of the application's UI, if any</param>
        /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
        /// <param name="timeOut">Timeout if SLR_NO_UI is specified, in ms.</param>
        public void Open(string linkFile, IntPtr hWnd, EShellLinkResolveFlags resolveFlags, ushort timeOut)
        {
            uint flags;

            if ((resolveFlags & EShellLinkResolveFlags.SLR_NO_UI)
                == EShellLinkResolveFlags.SLR_NO_UI)
            {
                flags = (uint)((int)resolveFlags | (timeOut << 16));
            }
            else
            {
                flags = (uint)resolveFlags;
            }

            if (linkA == null && linkW != null)
            {
                ((IPersistFile)linkW).Load(linkFile, 0); //STGM_DIRECT)
                linkW.Resolve(hWnd, flags);
                shortcutFile = linkFile;
            }
            else if (linkA != null && linkW == null)
            {
                ((IPersistFile)linkA).Load(linkFile, 0); //STGM_DIRECT)
                linkA.Resolve(hWnd, flags);
                shortcutFile = linkFile;
            }
            else
            {
                throw new InvalidOperationException("Unable to load shortcut");
            }
        }
        #endregion
    }
}
