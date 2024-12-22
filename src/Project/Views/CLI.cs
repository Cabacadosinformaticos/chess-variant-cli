using System;
using System.Collections.Generic;
using Chess.Controllers;

namespace Chess.Views {
    /// <summary>
    /// Reads the instructions from the standard input, sends them to the controllers and writes
    /// the answers to the standard output. This is the only class that writes to the console.
    /// </summary>
    public class CLI {
        private readonly PlayerController playerController;
        private readonly GameController gameController;

        /// <summary>
        /// Creates the view and the controllers it uses.
        /// </summary>
        public CLI() {
            playerController = new PlayerController();
            gameController = new GameController(playerController);
        }

        /// <summary>
        /// Runs the read loop until an empty line or the end of the input.
        /// </summary>
        public void Run() {
            while (true) {
                string? line = Console.ReadLine();
                if (line == null) {
                    break; // End of the input.
                }
                line = line.TrimEnd('\r');
                if (line.Length == 0) {
                    break; // Empty line ends the program.
                }

                string[] tokens = SplitTokens(line);
                if (tokens.Length == 0) {
                    WriteLine("Instrução inválida.");
                    continue;
                }
                Dispatch(tokens);
            }
        }

        /// <summary>
        /// Splits a line into tokens: single spaces separate them and empty tokens are dropped.
        /// </summary>
        private static string[] SplitTokens(string line) {
            List<string> tokens = new List<string>();
            foreach (string part in line.Split(' ')) {
                if (part.Length > 0) {
                    tokens.Add(part);
                }
            }
            return tokens.ToArray();
        }

        /// <summary>
        /// Sends one instruction line to the right controller. Every instruction checks its own
        /// number of tokens first and answers "Instrução inválida." when it does not match.
        /// </summary>
        private void Dispatch(string[] tokens) {
            switch (tokens[0]) {
                case "RJ":
                    if (tokens.Length == 2) {
                        WriteLine(playerController.RegisterPlayer(tokens[1]));
                    } else {
                        WriteLine("Instrução inválida.");
                    }
                    break;

                case "LJ":
                    if (tokens.Length == 1) {
                        WriteLine(playerController.ListPlayers());
                    } else {
                        WriteLine("Instrução inválida.");
                    }
                    break;

                case "IJ":
                    HandleStartGame(tokens);
                    break;

                case "MP":
                    if (tokens.Length == 4) {
                        WriteLine(gameController.MovePiece(tokens[1], tokens[2], tokens[3]));
                    } else {
                        WriteLine("Instrução inválida.");
                    }
                    break;

                case "OS":
                    if (tokens.Length == 3 || tokens.Length == 4) {
                        string? endToken = tokens.Length == 4 ? tokens[3] : null;
                        WriteLine(gameController.SpecialOperation(tokens[1], tokens[2], endToken));
                    } else {
                        WriteLine("Instrução inválida.");
                    }
                    break;

                case "DJ":
                    if (tokens.Length == 1) {
                        WriteLine(gameController.GetGameDetails());
                    } else {
                        WriteLine("Instrução inválida.");
                    }
                    break;

                case "D":
                    if (tokens.Length == 2 || tokens.Length == 3) {
                        string? secondName = tokens.Length == 3 ? tokens[2] : null;
                        WriteLine(gameController.Resign(tokens[1], secondName));
                    } else {
                        WriteLine("Instrução inválida.");
                    }
                    break;

                case "G":
                    // TODO (later stage): write the registry and the game to tokens[1].
                    WriteLine(tokens.Length == 2 ? "Funcionalidade não implementada." : "Instrução inválida.");
                    break;

                case "L":
                    // TODO (later stage): read the registry and the game from tokens[1].
                    WriteLine(tokens.Length == 2 ? "Funcionalidade não implementada." : "Instrução inválida.");
                    break;

                default:
                    WriteLine("Instrução inválida.");
                    break;
            }
        }

        /// <summary>
        /// Handles "IJ": starts a new game, or reads the 8 board lines of a continued game before
        /// asking the controller to start it. The lines are always consumed, even when the
        /// instruction then fails, so that the input stays in sync.
        /// </summary>
        private void HandleStartGame(string[] tokens) {
            if (tokens.Length != 4) {
                WriteLine("Instrução inválida.");
                return;
            }

            if (tokens[1] == "Novo") {
                WriteLine(gameController.StartNewGame(tokens[2], tokens[3]));
            } else if (tokens[1] == "Continuação") {
                string[] boardLines = ReadBoardLines();
                WriteLine(gameController.StartContinuedGame(tokens[2], tokens[3], boardLines));
            } else {
                WriteLine("Instrução inválida.");
            }
        }

        /// <summary>
        /// Reads exactly 8 board lines from the input. At the end of the input the missing lines
        /// are treated as empty lines.
        /// </summary>
        private static string[] ReadBoardLines() {
            string[] lines = new string[8];
            for (int i = 0; i < 8; i++) {
                string? line = Console.ReadLine();
                lines[i] = line == null ? string.Empty : line.TrimEnd('\r');
            }
            return lines;
        }

        /// <summary>
        /// Writes one line of output. It writes the '\n' explicitly so that the output is always
        /// the same, whatever the platform line ending is.
        /// </summary>
        private static void WriteLine(string text) {
            Console.Out.Write(text);
            Console.Out.Write('\n');
            Console.Out.Flush();
        }
    }
}
