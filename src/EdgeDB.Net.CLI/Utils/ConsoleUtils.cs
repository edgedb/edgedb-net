using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{
    /// <summary>
    ///     A utility class containing methods related to the console.
    /// </summary>
    internal class ConsoleUtils
    {
        /// <summary>
        ///     Reads a secret input from STDIN.
        /// </summary>
        /// <returns>
        ///     The entered input received from STDIN.
        /// </returns>
        public static string ReadSecretInput()
        {
            string input = "";
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace)
                    input = input.Length > 0 ? input[..^1] : "";
                else if(!char.IsControl(keyInfo.KeyChar))
                    input += keyInfo.KeyChar;
            }
            while (key != ConsoleKey.Enter);

            return input;
        }
    }
}
