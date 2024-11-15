using System.IO;

namespace EasyExtensions.Windows.Extensions
{
    /// <summary>
    /// <see cref="FileSystemInfo"/> extension methods, which can be used to manipulate files and directories.
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Move directory or file to recycle bin.
        /// </summary>
        /// <param name="fileOrDirectory">Directory or file to move.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool MoveToRecycleBin(this FileSystemInfo fileOrDirectory)
        {
            fileOrDirectory?.Refresh();
            if (fileOrDirectory is null || !fileOrDirectory.Exists)
            {
                return false;
            }
            else
            {
                return WinApi.DeleteFileOrFolder(fileOrDirectory.FullName);
            }
        }
    }
}
