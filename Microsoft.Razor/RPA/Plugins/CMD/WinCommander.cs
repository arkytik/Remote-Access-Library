using System.Diagnostics;

namespace Microsoft.Razor.RPA.Plugins.CMD
{
    /// <summary>
    /// Provides Functions To Work With Win32 CMD
    /// </summary>
    public static class WinCommander
    {
        /// <summary>
        /// Execute Some Bat Code
        /// </summary>
        /// <param name="code">Bat Code</param>
        /// <returns>False If Error, Otherwise True</returns>
        public static bool Execute(string code)
        {
            bool result = true;

            try
            {
                Process.Start("cmd.exe", code);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Execute Some Bat Code Async
        /// </summary>
        /// <param name="code">Bat Code</param>
        /// <returns>False If Error, Otherwise True</returns>
        public static ValueTask<bool> ExecuteAsync(string code) => ValueTask.FromResult(Execute(code));
    }
}
