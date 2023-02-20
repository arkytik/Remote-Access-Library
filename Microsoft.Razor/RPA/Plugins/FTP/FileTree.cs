namespace Microsoft.Razor.RPA.Plugins.FTP
{
    /// <summary>
    /// Provides Function To Get File Entries Or Drivers List
    /// </summary>
    public class FileTree
    {
        /// <summary>
        /// Get Drivers Of User
        /// </summary>
        /// <returns>Drivers Names</returns>
        public static string[] GetDrivers() => DriveInfo.GetDrives().Select(d => d.Name).ToArray();

        /// <summary>
        /// Get Drivers Names As String
        /// </summary>
        /// <returns>Drivers Names As String</returns>
        public static string GetDrivesAsString() => string.Join(":", GetDrivers());

        /// <summary>
        /// Get Entries Of Root Directory
        /// </summary>
        /// <param name="root">Root Folder Path</param>
        /// <returns>Entries Of Root Folder Or Null If Error</returns>
        public static string[]? GetEntities(string root)
        {
            try
            {
                if (string.IsNullOrEmpty(root)) return null;

                if (Directory.Exists(root))
                {
                    return Directory.EnumerateFileSystemEntries(root).ToArray();
                }
                else return null;
            } catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get Root Folder Entries As String With Split By ':'
        /// </summary>
        /// <param name="root">Root Folder Path</param>
        /// <returns>Entries Of Root Folder As String Or Null If Error</returns>
        public static string GetEntitiesAsString(string root) 
        {
            string[]? entities = GetEntities(root);

            if(entities != null && entities.Length > 0) return string.Join(":", entities);

            return string.Empty;
        }
    }
}
