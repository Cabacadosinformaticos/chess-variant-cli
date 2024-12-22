using System;
using System.IO;
using System.Text;
using Chess.Views;

namespace Chess {
    /// <summary>
    /// Entry point of the program: sets up the console and runs the command line interface.
    /// </summary>
    public class Program {
        /// <summary>
        /// Starts the chess variant command line interface.
        /// </summary>
        public static void Main(string[] args) {
            Console.OutputEncoding = new UTF8Encoding(false);
            try {
                Console.InputEncoding = new UTF8Encoding(false);
            } catch (IOException) {
                // When the input is redirected there is no console to change, the default is fine.
            }

            CLI cli = new CLI();
            cli.Run();
        }
    }
}
