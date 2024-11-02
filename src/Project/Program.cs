using System;

namespace ChessGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize the CLI
            CLI cli = new CLI();

            // Start the CLI loop
            cli.Run();
        }
    }
}