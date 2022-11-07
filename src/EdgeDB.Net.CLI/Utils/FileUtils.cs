using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    /// <summary>
    ///     A utility class containing methods related to file operations.
    /// </summary>
    internal static class FileUtils
    {
        /// <summary>
        ///     Waits synchronously for a file to be released.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns><see langword="true"/> if the file was released; otherwise <see langword="false"/>.</returns>
        public static bool WaitForHotFile(string path, int timeout = 5000)
        {
            var start = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    using var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    return true;
                }
                catch
                {
                    if ((DateTime.UtcNow - start).TotalMilliseconds >= timeout)
                        return false;

                    Thread.Sleep(200);
                }
            }
        }
    }
}
