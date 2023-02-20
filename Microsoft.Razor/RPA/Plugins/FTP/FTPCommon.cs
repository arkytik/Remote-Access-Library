namespace Microsoft.Razor.RPA.Plugins.FTP
{
    /// <summary>
    /// Common FTP Format Builder
    /// </summary>
    public struct FTPCommon
    {
        public const string FTPFlag = "#FTP#";

        public const string FTPFilesAction = "#FILES#";
        public const string FTPDownloadAction = "#DOWNLOAD#";

        /// <summary>
        /// Parse FTP Builder Data
        /// </summary>
        /// <param name="fileparts"></param>
        /// <param name="action"></param>
        /// <param name="args"></param>
        public static void GetAction(string? fileparts, ref string? action, ref string? args)
        {
            if (!string.IsNullOrEmpty(fileparts))
            {
                string[] filedata = fileparts.Split(':');

                if (filedata != null && filedata.Length >= 2)
                {
                    string flag = filedata[0].Trim();

                    if (flag == FTPFlag)
                    {
                        action = filedata[1];

                        if (filedata.Length == 3) args = filedata[2];
                    }
                }
            }
        }

        /// <summary>
        /// Make FTP Format
        /// </summary>
        /// <param name="action"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string? PackAction(string? action, string? args)
        {
            if (string.IsNullOrEmpty(action)) return null;

            action = action.Trim();

            if (string.IsNullOrEmpty(args)) return $"{FTPFlag}:{action}";
            else
            {
                args = args.Trim();

                return $"{FTPFlag}:{action}:{args}";
            }
        }
    }
}
