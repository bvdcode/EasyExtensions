using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EasyExtensions.Windows
{
    /// <summary>
    /// Summary description for ShellLink.
    /// </summary>
    public class ShellLink : IDisposable
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

            //[helpstring("Sets the name of the shell link working directory")]
            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPStr)] string pszDir);

            //[helpstring("Retrieves the shell link command-line arguments")]
            void GetArguments(
                [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs,
                int cchMaxPath);

            //[helpstring("Sets the shell link command-line arguments")]
            void SetArguments(
                [MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            //[propget, helpstring("Retrieves or sets the shell link hot key")]
            void GetHotkey(out short pwHotkey);
            //[propput, helpstring("Retrieves or sets the shell link hot key")]
            void SetHotkey(short pwHotkey);

            //[propget, helpstring("Retrieves or sets the shell link show command")]
            void GetShowCmd(out uint piShowCmd);
            //[propput, helpstring("Retrieves or sets the shell link show command")]
            void SetShowCmd(uint piShowCmd);

            //[helpstring("Retrieves the location (path and index) of the shell link icon")]
            void GetIconLocation(
                [Out(), MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            //[helpstring("Sets the location (path and index) of the shell link icon")]
            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPStr)] string pszIconPath,
                int iIcon);

            //[helpstring("Sets the shell link relative path")]
            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPStr)] string pszPathRel,
                uint dwReserved);

            //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
            void Resolve(
                IntPtr hWnd,
                uint fFlags);

            //[helpstring("Sets the shell link path and filename")]
            void SetPath(
                [MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }


        [ComImport()]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            //[helpstring("Retrieves the path and filename of a shell link object")]
            void GetPath(
                [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxPath,
                ref WIN32_FIND_DATAW pfd,
                uint fFlags);

            //[helpstring("Retrieves the list of shell link item identifiers")]
            void GetIDList(out IntPtr ppidl);

            //[helpstring("Sets the list of shell link item identifiers")]
            void SetIDList(IntPtr pidl);

            //[helpstring("Retrieves the shell link description string")]
            void GetDescription(
                [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxName);

            //[helpstring("Sets the shell link description string")]
            void SetDescription(
                [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            //[helpstring("Retrieves the name of the shell link working directory")]
            void GetWorkingDirectory(
                [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
                int cchMaxPath);

            //[helpstring("Sets the name of the shell link working directory")]
            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            //[helpstring("Retrieves the shell link command-line arguments")]
            void GetArguments(
                [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
                int cchMaxPath);

            //[helpstring("Sets the shell link command-line arguments")]
            void SetArguments(
                [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            //[propget, helpstring("Retrieves or sets the shell link hot key")]
            void GetHotkey(out short pwHotkey);
            //[propput, helpstring("Retrieves or sets the shell link hot key")]
            void SetHotkey(short pwHotkey);

            //[propget, helpstring("Retrieves or sets the shell link show command")]
            void GetShowCmd(out uint piShowCmd);
            //[propput, helpstring("Retrieves or sets the shell link show command")]
            void SetShowCmd(uint piShowCmd);

            //[helpstring("Retrieves the location (path and index) of the shell link icon")]
            void GetIconLocation(
                [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            //[helpstring("Sets the location (path and index) of the shell link icon")]
            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
                int iIcon);

            //[helpstring("Sets the shell link relative path")]
            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
                uint dwReserved);

            //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
            void Resolve(
                IntPtr hWnd,
                uint fFlags);

            //[helpstring("Sets the shell link path and filename")]
            void SetPath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
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
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }
        #endregion

        #region IShellLink Private structs

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0, CharSet = CharSet.Ansi)]
        private struct WIN32_FIND_DATAA
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
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

        #region UnManaged Methods
        private class UnManagedMethods
        {
            [DllImport("Shell32", CharSet = CharSet.Auto)]
            internal extern static int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)]
                string lpszFile, int nIconIndex, IntPtr[] phIconLarge, IntPtr[] phIconSmall, int nIcons);

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
        public enum LinkDisplayMode : uint
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
        public Icon LargeIcon => GetIcon(true);

        /// <summary>
        /// Gets a System.Drawing.Icon containing the icon for this
        /// ShellLink object.
        /// </summary>
        public Icon SmallIcon => GetIcon(false);

        private Icon GetIcon(bool large)
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
                    _ = UnManagedMethods.ExtractIconEx(
                        iconFile,
                        iconIndex,
                        hIconEx,
                        null,
                        1);
                }
                else
                {
                    _ = UnManagedMethods.ExtractIconEx(
                        iconFile,
                        iconIndex,
                        null,
                        hIconEx,
                        1);
                }
                // If success then return as a GDI+ object
                Icon icon = null;
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
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out _);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out _);
                }
                return iconPath.ToString();
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                int iconIndex;
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                if (linkA == null)
                {
                    linkW.SetIconLocation(value, iconIndex);
                }
                else
                {
                    linkA.SetIconLocation(value, iconIndex);
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
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
                }
                return iconIndex;
            }
            set
            {
                StringBuilder iconPath = new StringBuilder(260, 260);
                if (linkA == null)
                {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out _);
                }
                else
                {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out _);
                }
                if (linkA == null)
                {
                    linkW.SetIconLocation(iconPath.ToString(), value);
                }
                else
                {
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
                if (linkA == null)
                {
                    WIN32_FIND_DATAW fd = new WIN32_FIND_DATAW();
                    linkW.GetPath(target, target.Capacity, ref fd, (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                else
                {
                    WIN32_FIND_DATAA fd = new WIN32_FIND_DATAA();
                    linkA.GetPath(target, target.Capacity, ref fd, (uint)EShellLinkGP.SLGP_UNCPRIORITY);
                }
                return target.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetPath(value);
                }
                else
                {
                    linkA.SetPath(value);
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
                if (linkA == null)
                {
                    linkW.GetWorkingDirectory(path, path.Capacity);
                }
                else
                {
                    linkA.GetWorkingDirectory(path, path.Capacity);
                }
                return path.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetWorkingDirectory(value);
                }
                else
                {
                    linkA.SetWorkingDirectory(value);
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
                if (linkA == null)
                {
                    linkW.GetDescription(description, description.Capacity);
                }
                else
                {
                    linkA.GetDescription(description, description.Capacity);
                }
                return description.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetDescription(value);
                }
                else
                {
                    linkA.SetDescription(value);
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
                if (linkA == null)
                {
                    linkW.GetArguments(arguments, arguments.Capacity);
                }
                else
                {
                    linkA.GetArguments(arguments, arguments.Capacity);
                }
                return arguments.ToString();
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetArguments(value);
                }
                else
                {
                    linkA.SetArguments(value);
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
        public void Save(
            string linkFile
            )
        {
            // Save the object to disk
            if (linkA == null)
            {
                ((IPersistFile)linkW).Save(linkFile, true);
                shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile)linkA).Save(linkFile, true);
                shortcutFile = linkFile;
            }
        }

        /// <summary>
        /// Loads a shortcut from the specified file
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        public void Open(
            string linkFile
            )
        {
            Open(linkFile,
                IntPtr.Zero,
                (EShellLinkResolveFlags.SLR_ANY_MATCH | EShellLinkResolveFlags.SLR_NO_UI),
                1);
        }

        /// <summary>
        /// Loads a shortcut from the specified file, and allows flags controlling
        /// the UI behaviour if the shortcut's target isn't found to be set.
        /// </summary>
        /// <param name="linkFile">The shortcut file (.lnk) to load</param>
        /// <param name="hWnd">The window handle of the application's UI, if any</param>
        /// <param name="resolveFlags">Flags controlling resolution behaviour</param>
        public void Open(
            string linkFile,
            IntPtr hWnd,
            EShellLinkResolveFlags resolveFlags
            )
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
        public void Open(
            string linkFile,
            IntPtr hWnd,
            EShellLinkResolveFlags resolveFlags,
            ushort timeOut
            )
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
                this.shortcutFile = linkFile;
            }
            else if (linkA != null && linkW == null)
            {
                ((IPersistFile)linkA).Load(linkFile, 0); //STGM_DIRECT)
                linkA.Resolve(hWnd, flags);
                this.shortcutFile = linkFile;
            }
            else
            {
                throw new InvalidOperationException("Unable to load shortcut");
            }
        }
        #endregion
    }
}