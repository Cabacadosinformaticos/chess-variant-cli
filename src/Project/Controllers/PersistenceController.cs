using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chess.Models;

namespace Chess.Controllers {
    /// <summary>
    /// Saves and reads the registry of players and the game in progress to and from a plain text
    /// file.
    ///
    /// File format (UTF-8 without BOM, one record per line, '\n' line endings):
    ///
    ///     CHESS-SAVE 1                       magic and version, always the first line
    ///     PLAYER name wins draws losses      one line per registered player
    ///     ...
    ///     GAME                               present only when a game is in progress
    ///     WHITE name                         player with the white pieces
    ///     BLACK name                         player with the black pieces
    ///     TURN W|B                           colour that has to play
    ///     PIECE symbol square hasMoved specialUsed
    ///     ...                                one line per piece, 0/1 flags, top row first
    ///     ENPASSANT target pawn              only when the last move created an en passant
    ///                                        opportunity; target is the square the double
    ///                                        stepped pawn skipped and pawn is its own square
    ///
    /// The player lines come first and the game lines after them; a file without a game simply
    /// stops after the players. Reading a file replaces every registered player and the game in
    /// progress, and the players of the loaded game are the registry's own objects.
    /// </summary>
    public class PersistenceController {
        private const string FileHeader = "CHESS-SAVE 1";
        private const string GameHeader = "GAME";

        // UTF-8 without BOM, the same encoding as the console output.
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private readonly PlayerController playerController;
        private readonly GameController gameController;

        /// <summary>
        /// Creates a persistence controller that reads and writes the state of the given
        /// controllers.
        /// </summary>
        /// <param name="playerController">Registry of the players.</param>
        /// <param name="gameController">Controller of the game in progress.</param>
        public PersistenceController(PlayerController playerController, GameController gameController) {
            this.playerController = playerController;
            this.gameController = gameController;
        }

        /// <summary>
        /// Writes the registry and, when there is one, the game in progress to a file.
        /// </summary>
        /// <param name="fileName">Name of the file to write.</param>
        /// <returns>The success or error message.</returns>
        public string Save(string fileName) {
            string text = BuildSaveText();
            try {
                File.WriteAllText(fileName, text, Utf8NoBom);
            } catch (Exception exception) when (IsFileError(exception)) {
                return "Erro ao gravar o ficheiro.";
            }
            return "Jogo gravado com sucesso.";
        }

        /// <summary>
        /// Replaces every registered player and the whole game state with the contents of a file.
        /// It is refused while a game is in progress and when the file cannot be read.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <returns>The success or error message.</returns>
        public string Load(string fileName) {
            if (gameController.GameInProgress) {
                return "Existe um jogo em curso.";
            }

            string text;
            try {
                text = File.ReadAllText(fileName, Utf8NoBom);
            } catch (Exception exception) when (IsFileError(exception)) {
                return "Erro ao ler o ficheiro.";
            }

            SavedState state;
            try {
                state = ParseSaveText(text);
            } catch (FormatException) {
                return "Erro ao ler o ficheiro.";
            }

            ApplyState(state);
            return "Jogo lido com sucesso.";
        }

        /// <summary>
        /// Builds the whole text of a save file: the header, one line per registered player and
        /// the game in progress when there is one.
        /// </summary>
        private string BuildSaveText() {
            StringBuilder builder = new StringBuilder();
            builder.Append(FileHeader).Append('\n');

            foreach (Player player in playerController.Players) {
                builder.Append("PLAYER ").Append(player.Name).Append(' ')
                    .Append(player.Wins).Append(' ').Append(player.Draws).Append(' ')
                    .Append(player.Losses).Append('\n');
            }

            if (gameController.GameInProgress) {
                AppendGame(builder);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Adds the lines of the game in progress to the save text: the two player names, the
        /// turn, one line per piece and the en passant state when there is one.
        /// </summary>
        private void AppendGame(StringBuilder builder) {
            Board board = gameController.Board!;
            builder.Append(GameHeader).Append('\n');
            builder.Append("WHITE ").Append(gameController.WhitePlayer!.Name).Append('\n');
            builder.Append("BLACK ").Append(gameController.BlackPlayer!.Name).Append('\n');
            builder.Append("TURN ").Append(gameController.CurrentTurn).Append('\n');

            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    Piece? piece = board.GetPiece(row, column);
                    if (piece == null) {
                        continue;
                    }
                    builder.Append("PIECE ").Append(piece.Symbol).Append(' ')
                        .Append(new Position(piece.Row, piece.Column)).Append(' ')
                        .Append(piece.HasMoved ? '1' : '0').Append(' ')
                        .Append(piece.SpecialUsed ? '1' : '0').Append('\n');
                }
            }

            if (gameController.EnPassantTarget.HasValue && gameController.EnPassantPawn != null) {
                builder.Append("ENPASSANT ").Append(gameController.EnPassantTarget.Value).Append(' ')
                    .Append(new Position(gameController.EnPassantPawn.Row, gameController.EnPassantPawn.Column))
                    .Append('\n');
            }
        }

        /// <summary>
        /// Replaces the registry and the game with a state read from a file. The whole state is
        /// already parsed and validated, so nothing is changed when the file is malformed.
        /// </summary>
        private void ApplyState(SavedState state) {
            playerController.ReplacePlayers(state.Players);

            if (state.Game == null) {
                gameController.ClearGame();
                return;
            }

            // The game has to use the registry's own Player objects.
            Player white = playerController.FindPlayer(state.Game.WhiteName)!;
            Player black = playerController.FindPlayer(state.Game.BlackName)!;
            Board board = BuildBoard(state.Game);
            Piece? enPassantPawn = state.Game.EnPassantPawn.HasValue
                ? board.GetPiece(state.Game.EnPassantPawn.Value.Row, state.Game.EnPassantPawn.Value.Column)
                : null;
            gameController.RestoreGame(white, black, board, state.Game.Turn,
                state.Game.EnPassantTarget, enPassantPawn);
        }

        /// <summary>
        /// Builds an empty board and puts the saved pieces on it, with their flags.
        /// </summary>
        private static Board BuildBoard(SavedGame game) {
            Board board = new Board();
            foreach (SavedPiece saved in game.Pieces) {
                Piece? piece = board.CreatePiece(saved.Symbol, saved.Position.Row, saved.Position.Column);
                if (piece == null) {
                    throw new FormatException("Invalid piece symbol.");
                }
                piece.RestoreState(saved.Position.Row, saved.Position.Column, saved.HasMoved, saved.SpecialUsed);
                board.SetPiece(saved.Position.Row, saved.Position.Column, piece);
            }
            return board;
        }

        /// <summary>
        /// Reads the lines of a save file into a state object, without touching the controllers.
        /// </summary>
        /// <param name="text">Whole text of the file.</param>
        /// <returns>The parsed state.</returns>
        /// <exception cref="FormatException">The text is not a valid save file.</exception>
        private static SavedState ParseSaveText(string text) {
            List<string> lines = new List<string>();
            foreach (string rawLine in text.Split('\n')) {
                string line = rawLine.TrimEnd('\r');
                if (line.Length > 0) {
                    lines.Add(line);
                }
            }

            if (lines.Count == 0 || lines[0] != FileHeader) {
                throw new FormatException("Missing file header.");
            }

            SavedState state = new SavedState();
            SavedGame? game = null;
            int index = 1;
            while (index < lines.Count) {
                string[] tokens = SplitTokens(lines[index]);
                index++;
                if (tokens.Length == 0) {
                    continue;
                }

                switch (tokens[0]) {
                    case "PLAYER":
                        state.Players.Add(ParsePlayer(tokens));
                        break;
                    case GameHeader:
                        if (game != null) {
                            throw new FormatException("More than one game section.");
                        }
                        game = new SavedGame();
                        break;
                    case "WHITE":
                        RequireGame(game).WhiteName = ParseName(tokens);
                        break;
                    case "BLACK":
                        RequireGame(game).BlackName = ParseName(tokens);
                        break;
                    case "TURN":
                        RequireGame(game).Turn = ParseTurn(tokens);
                        break;
                    case "PIECE":
                        RequireGame(game).Pieces.Add(ParsePiece(tokens));
                        break;
                    case "ENPASSANT":
                        RequireGame(game).EnPassantTarget = ParsePosition(tokens, 1);
                        RequireGame(game).EnPassantPawn = ParsePosition(tokens, 2);
                        break;
                    default:
                        throw new FormatException("Unknown record.");
                }
            }

            if (game != null) {
                ValidateGame(game, state.Players);
            }
            state.Game = game;
            return state;
        }

        /// <summary>
        /// Checks that a parsed game section is complete and consistent.
        /// </summary>
        private static void ValidateGame(SavedGame game, List<Player> players) {
            if (game.WhiteName.Length == 0 || game.BlackName.Length == 0) {
                throw new FormatException("Game without players.");
            }
            if (game.Turn != 'W' && game.Turn != 'B') {
                throw new FormatException("Invalid turn.");
            }
            if (FindByName(players, game.WhiteName) == null || FindByName(players, game.BlackName) == null) {
                throw new FormatException("Game player not registered.");
            }
            if (game.EnPassantTarget.HasValue != game.EnPassantPawn.HasValue) {
                throw new FormatException("Incomplete en passant state.");
            }
        }

        /// <summary>
        /// Builds a player record: "PLAYER name wins draws losses".
        /// </summary>
        private static Player ParsePlayer(string[] tokens) {
            if (tokens.Length != 5) {
                throw new FormatException("Invalid player record.");
            }
            Player player = new Player(tokens[1]);
            player.RestoreStats(ParseInt(tokens[2]), ParseInt(tokens[3]), ParseInt(tokens[4]));
            return player;
        }

        /// <summary>
        /// Builds a piece record: "PIECE symbol square hasMoved specialUsed".
        /// </summary>
        private static SavedPiece ParsePiece(string[] tokens) {
            if (tokens.Length != 5) {
                throw new FormatException("Invalid piece record.");
            }
            if (!Board.TryParseSymbol(tokens[1], out _, out _, out _)) {
                throw new FormatException("Invalid piece symbol.");
            }
            return new SavedPiece {
                Symbol = tokens[1],
                Position = ParsePosition(tokens, 2),
                HasMoved = ParseFlag(tokens[3]),
                SpecialUsed = ParseFlag(tokens[4]),
            };
        }

        /// <summary>
        /// Reads the name of a record that holds exactly a keyword and a name.
        /// </summary>
        private static string ParseName(string[] tokens) {
            if (tokens.Length != 2) {
                throw new FormatException("Invalid name record.");
            }
            return tokens[1];
        }

        /// <summary>
        /// Reads the turn of a "TURN W|B" record.
        /// </summary>
        private static char ParseTurn(string[] tokens) {
            if (tokens.Length != 2 || (tokens[1] != "W" && tokens[1] != "B")) {
                throw new FormatException("Invalid turn record.");
            }
            return tokens[1][0];
        }

        /// <summary>
        /// Reads a square token of a record, e.g. the first one after "ENPASSANT".
        /// </summary>
        private static Position ParsePosition(string[] tokens, int index) {
            if (index >= tokens.Length || !Position.TryParse(tokens[index], out Position position)) {
                throw new FormatException("Invalid position.");
            }
            return position;
        }

        /// <summary>
        /// Reads a 0/1 flag.
        /// </summary>
        private static bool ParseFlag(string token) {
            if (token == "1") {
                return true;
            }
            if (token == "0") {
                return false;
            }
            throw new FormatException("Invalid flag.");
        }

        /// <summary>
        /// Reads a non negative integer of a record.
        /// </summary>
        private static int ParseInt(string token) {
            if (!int.TryParse(token, out int value) || value < 0) {
                throw new FormatException("Invalid number.");
            }
            return value;
        }

        /// <summary>
        /// Returns the game section of a record, which has to have been opened by "GAME" first.
        /// </summary>
        private static SavedGame RequireGame(SavedGame? game) {
            if (game == null) {
                throw new FormatException("Record outside the game section.");
            }
            return game;
        }

        /// <summary>
        /// Returns the player with the given name inside a list, or null when there is none.
        /// </summary>
        private static Player? FindByName(List<Player> players, string name) {
            foreach (Player player in players) {
                if (player.Name == name) {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Splits a line into tokens on single spaces, dropping the empty ones.
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
        /// Tells whether an exception is a file access error that the instructions report as
        /// "Erro ao gravar o ficheiro." or "Erro ao ler o ficheiro.".
        /// </summary>
        private static bool IsFileError(Exception exception) {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is ArgumentException
                || exception is NotSupportedException;
        }

        /// <summary>
        /// Everything a save file holds: the registry and, optionally, the game in progress.
        /// </summary>
        private sealed class SavedState {
            public List<Player> Players { get; } = new List<Player>();
            public SavedGame? Game { get; set; }
        }

        /// <summary>
        /// The game in progress as it is written in a save file.
        /// </summary>
        private sealed class SavedGame {
            public string WhiteName { get; set; } = string.Empty;
            public string BlackName { get; set; } = string.Empty;
            public char Turn { get; set; } = 'W';
            public List<SavedPiece> Pieces { get; } = new List<SavedPiece>();
            public Position? EnPassantTarget { get; set; }
            public Position? EnPassantPawn { get; set; }
        }

        /// <summary>
        /// One piece of a saved game: its symbol, its square and its two flags.
        /// </summary>
        private sealed class SavedPiece {
            public string Symbol { get; set; } = string.Empty;
            public Position Position { get; set; }
            public bool HasMoved { get; set; }
            public bool SpecialUsed { get; set; }
        }
    }
}
