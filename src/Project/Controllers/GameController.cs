using System.Collections.Generic;
using Chess.Models;

namespace Chess.Controllers {
    /// <summary>
    /// Holds the state of the game in progress: the two players, the board, whose turn it is and
    /// whether a game is running. It answers the instructions with plain strings; only the view
    /// writes to the console.
    /// </summary>
    public class GameController {
        private readonly PlayerController playerController;

        /// <summary>
        /// Player with the white pieces, null when no game was started yet.
        /// </summary>
        public Player? WhitePlayer { get; private set; }

        /// <summary>
        /// Player with the black pieces, null when no game was started yet.
        /// </summary>
        public Player? BlackPlayer { get; private set; }

        /// <summary>
        /// Board of the game in progress, null when no game was started yet.
        /// </summary>
        public Board? Board { get; private set; }

        /// <summary>
        /// Colour that has to play now: 'W' or 'B'. White always plays first.
        /// </summary>
        public char CurrentTurn { get; private set; }

        /// <summary>
        /// True while a game is running.
        /// </summary>
        public bool GameInProgress { get; private set; }

        /// <summary>
        /// Creates a game controller that looks players up in the given registry.
        /// </summary>
        public GameController(PlayerController playerController) {
            this.playerController = playerController;
            GameInProgress = false;
        }

        /// <summary>
        /// Starts a new game with the standard starting position. Player A plays white.
        /// </summary>
        /// <param name="playerAName">Name of the player with the white pieces.</param>
        /// <param name="playerBName">Name of the player with the black pieces.</param>
        /// <returns>The success or error message.</returns>
        public string StartNewGame(string playerAName, string playerBName) {
            if (GameInProgress) {
                return "Existe um jogo em curso.";
            }

            Player? playerA = playerController.FindPlayer(playerAName);
            Player? playerB = playerController.FindPlayer(playerBName);
            if (playerA == null || playerB == null) {
                return "Jogador inexistente.";
            }

            Board board = new Board();
            board.InitializeStandardSetup();
            return BeginGame(playerA, playerB, board);
        }

        /// <summary>
        /// Starts a game from the 8 board lines of an "IJ Continuação". Any board configuration is
        /// accepted; white still plays first and all the pieces start as never moved.
        /// </summary>
        /// <param name="playerAName">Name of the player with the white pieces.</param>
        /// <param name="playerBName">Name of the player with the black pieces.</param>
        /// <param name="boardLines">The 8 board lines read from the input.</param>
        /// <returns>The success or error message.</returns>
        public string StartContinuedGame(string playerAName, string playerBName, string[] boardLines) {
            if (GameInProgress) {
                return "Existe um jogo em curso.";
            }

            Player? playerA = playerController.FindPlayer(playerAName);
            Player? playerB = playerController.FindPlayer(playerBName);
            if (playerA == null || playerB == null) {
                return "Jogador inexistente.";
            }

            Board board = new Board();
            board.LoadFromLines(boardLines);
            return BeginGame(playerA, playerB, board);
        }

        /// <summary>
        /// Shared start of a game: stores the players and the board and hands the first turn
        /// to the white player.
        /// </summary>
        private string BeginGame(Player whitePlayer, Player blackPlayer, Board board) {
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
            Board = board;
            CurrentTurn = 'W';
            GameInProgress = true;
            return "Jogo iniciado com sucesso.";
        }

        /// <summary>
        /// Renders the board and tells whose turn it is, for the "DJ" instruction.
        /// </summary>
        /// <returns>The board lines plus the turn line, or the error message.</returns>
        public string GetGameDetails() {
            if (!GameInProgress || Board == null) {
                return "Não existe jogo em curso.";
            }

            List<string> lines = Board.GetDisplayLines();
            lines.Add($"Vez de {CurrentPlayer().Name}.");
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Validates the part of a move that does not depend on the chess rules and then applies
        /// the move. The rule checks (geometry, check, castling, en passant, promotion) are added
        /// in a later stage.
        /// </summary>
        /// <param name="playerName">Name of the player asking for the move.</param>
        /// <param name="startToken">Position token of the piece, e.g. "E2".</param>
        /// <param name="endToken">Position token of the target, e.g. "E4".</param>
        /// <returns>The success or error message.</returns>
        public string MovePiece(string playerName, string startToken, string endToken) {
            if (!GameInProgress || Board == null) {
                return "Não existe jogo em curso.";
            }
            if (!IsPlayerInGame(playerName)) {
                return "Jogador não participa no jogo em curso.";
            }
            if (CurrentPlayer().Name != playerName) {
                return "Não é a vez do jogador.";
            }
            if (!Position.TryParse(startToken, out Position start)
                || !Position.TryParse(endToken, out _)) {
                return "Posição inválida.";
            }
            if (Board.GetPiece(start.Row, start.Column) == null) {
                return "Não existe peça na posição inicial.";
            }

            // TODO (later stage): validate the move of the piece and apply it, together with
            // capture, check, checkmate, castling, en passant and promotion.
            return "Funcionalidade não implementada.";
        }

        /// <summary>
        /// Validates the part of a special operation that does not depend on the chess rules and
        /// then performs it. The operations themselves are added in a later stage.
        /// </summary>
        /// <param name="playerName">Name of the player asking for the operation.</param>
        /// <param name="startToken">Position token of the piece, e.g. "E2".</param>
        /// <param name="endToken">Optional position token used by the operation.</param>
        /// <returns>The success or error message.</returns>
        public string SpecialOperation(string playerName, string startToken, string? endToken) {
            if (!GameInProgress || Board == null) {
                return "Não existe jogo em curso.";
            }
            if (!IsPlayerInGame(playerName)) {
                return "Jogador não participa no jogo em curso.";
            }
            if (CurrentPlayer().Name != playerName) {
                return "Não é a vez do jogador.";
            }
            if (!Position.TryParse(startToken, out Position start)) {
                return "Posição inválida.";
            }
            if (Board.GetPiece(start.Row, start.Column) == null) {
                return "Não existe peça na posição inicial.";
            }
            if (endToken != null && !Position.TryParse(endToken, out _)) {
                return "Posição final inválida.";
            }

            // TODO (later stage): check whether the piece needs an end position, then perform the
            // special operation of the piece (pawn, rook, horse, bishop or queen).
            return "Funcionalidade não implementada.";
        }

        /// <summary>
        /// Ends the game by resignation. A single resigning player loses and the opponent wins;
        /// when both players resign the game is a draw for both.
        /// </summary>
        /// <param name="playerName1">Name of the first player resigning.</param>
        /// <param name="playerName2">Optional name of the second player resigning.</param>
        /// <returns>The success or error message.</returns>
        public string Resign(string playerName1, string? playerName2) {
            if (!GameInProgress) {
                return "Não existe jogo em curso.";
            }
            if (playerController.FindPlayer(playerName1) == null) {
                return "Jogador inexistente.";
            }
            if (playerName2 != null && playerController.FindPlayer(playerName2) == null) {
                return "Jogador inexistente.";
            }
            if (!IsPlayerInGame(playerName1)) {
                return "Jogador não participa no jogo em curso.";
            }
            if (playerName2 != null && !IsPlayerInGame(playerName2)) {
                return "Jogador não participa no jogo em curso.";
            }

            if (playerName2 == null || playerName2 == playerName1) {
                // Only one player resigns, the other one wins.
                Player resigning = FindInGamePlayer(playerName1);
                Player winner = resigning == WhitePlayer ? BlackPlayer! : WhitePlayer!;
                resigning.IncrementLosses();
                winner.IncrementWins();
            } else {
                // Both players resign, the game is a draw.
                WhitePlayer!.IncrementDraws();
                BlackPlayer!.IncrementDraws();
            }

            GameInProgress = false;
            return "Jogo terminado com sucesso.";
        }

        /// <summary>
        /// Tells whether a player name takes part in the game in progress.
        /// </summary>
        private bool IsPlayerInGame(string playerName) {
            return WhitePlayer != null && BlackPlayer != null
                && (WhitePlayer.Name == playerName || BlackPlayer.Name == playerName);
        }

        /// <summary>
        /// Returns the registered player object that plays in the current game.
        /// </summary>
        private Player FindInGamePlayer(string playerName) {
            return WhitePlayer!.Name == playerName ? WhitePlayer! : BlackPlayer!;
        }

        /// <summary>
        /// Returns the player whose turn it is.
        /// </summary>
        private Player CurrentPlayer() {
            return CurrentTurn == 'W' ? WhitePlayer! : BlackPlayer!;
        }

        // TODO (later stage): save and load (G and L), through a PersistenceController class.
    }
}
