using System;
using System.Collections.Generic;
using Chess.Models;
using Chess.Models.Pieces;

namespace Chess.Controllers {
    /// <summary>
    /// Holds the state of the game in progress: the two players, the board, whose turn it is and
    /// whether a game is running. It answers the instructions with plain strings; only the view
    /// writes to the console.
    /// </summary>
    public class GameController {
        private readonly PlayerController playerController;

        // En passant opportunity created by the last move: the pawn that has just stepped two
        // squares and the square it skipped. It lasts for exactly one opponent turn.
        private Position? enPassantTarget;
        private Piece? enPassantPawn;

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
            enPassantTarget = null;
            enPassantPawn = null;
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
        /// Moves a piece for the player whose turn it is, applying every chess rule: normal moves,
        /// castling, en passant and promotion, always rejecting a move that would leave the own
        /// king in check. It reports the result with the single success line of the highest
        /// priority that applies and ends the game on checkmate or stalemate.
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
                || !Position.TryParse(endToken, out Position end)) {
                return "Posição inválida.";
            }

            Piece? piece = Board.GetPiece(start.Row, start.Column);
            if (piece == null) {
                return "Não existe peça na posição inicial.";
            }
            if (piece.Color != CurrentTurn) {
                return "Movimento inválido."; // A player can only move his own pieces.
            }
            if (!IsLegalMove(piece, end.Row, end.Column)) {
                return "Movimento inválido.";
            }

            bool isEnPassant = IsEnPassantCapture(piece, end.Row, end.Column);
            bool isCastling = IsCastlingMove(piece, end.Row, end.Column);
            Player mover = CurrentPlayer();

            int startRow = piece.Row;
            int startColumn = piece.Column;
            bool isDoubleStep = piece.Type == 'P' && Math.Abs(end.Row - startRow) == 2;
            bool isPromotion = piece.Type == 'P' && end.Row == (piece.Color == 'W' ? 0 : 7);

            // A turn is passing, so the previous en passant opportunity is gone.
            enPassantTarget = null;
            enPassantPawn = null;

            Piece? captured;
            if (isEnPassant) {
                captured = Board.GetPiece(startRow, end.Column);
                Board.RemovePiece(startRow, end.Column);
                piece.MoveTo(end.Row, end.Column);
            } else if (isCastling) {
                captured = null;
                int rookFromColumn = end.Column > startColumn ? 7 : 0;
                int rookToColumn = end.Column > startColumn ? end.Column - 1 : end.Column + 1;
                Piece rook = Board.GetPiece(startRow, rookFromColumn)!;
                piece.MoveTo(end.Row, end.Column);
                rook.MoveTo(startRow, rookToColumn);
            } else {
                captured = piece.Move(end.Row, end.Column);
            }

            if (isDoubleStep) {
                // The opponent has exactly one turn to capture this pawn on the skipped square.
                enPassantPawn = piece;
                enPassantTarget = new Position(startRow + (piece.Color == 'W' ? -1 : 1), startColumn);
            }

            if (isPromotion) {
                PromotePawn(piece);
            }

            // The turn passes to the opponent, who may now be in check or have no move at all.
            CurrentTurn = CurrentTurn == 'W' ? 'B' : 'W';
            return DescribeMoveResult(mover, piece.Symbol, captured, isPromotion, isCastling, isEnPassant);
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
        /// Tells whether a move of a piece to a square is legal: its geometry has to be accepted by
        /// the piece (or by castling or en passant) and the move must not leave the mover's own
        /// king in check.
        /// </summary>
        private bool IsLegalMove(Piece piece, int endRow, int endColumn) {
            bool isEnPassant = IsEnPassantCapture(piece, endRow, endColumn);
            bool isCastling = IsCastlingMove(piece, endRow, endColumn);
            if (!isEnPassant && !isCastling && !piece.CanMove(endRow, endColumn)) {
                return false;
            }
            return !LeavesOwnKingInCheck(piece, endRow, endColumn, isEnPassant, isCastling);
        }

        /// <summary>
        /// Tells whether a pawn move captures en passant: the pawn is next to the opponent pawn
        /// that has just stepped two squares and lands on the square that pawn skipped.
        /// </summary>
        private bool IsEnPassantCapture(Piece piece, int endRow, int endColumn) {
            Board board = Board!;
            if (enPassantPawn == null || enPassantTarget == null) {
                return false;
            }
            if (piece.Type != 'P' || piece.Color == enPassantPawn.Color) {
                return false;
            }
            if (endRow != enPassantTarget.Value.Row || endColumn != enPassantTarget.Value.Column) {
                return false;
            }
            if (piece.Row != enPassantPawn.Row || Math.Abs(piece.Column - enPassantPawn.Column) != 1) {
                return false;
            }
            // The pawn may have been captured in the meantime, then there is nothing to take.
            return board.GetPiece(enPassantPawn.Row, enPassantPawn.Column) == enPassantPawn;
        }

        /// <summary>
        /// Tells whether a king move is a legal castling: the king moves two squares on its row
        /// towards a rook of the same colour that has never moved, every square between them is
        /// empty and the king is not in check, does not cross and does not land on an attacked
        /// square. The rook jumps to the square the king crossed.
        /// </summary>
        private bool IsCastlingMove(Piece piece, int endRow, int endColumn) {
            Board board = Board!;
            if (piece.Type != 'K' || piece.HasMoved) {
                return false;
            }
            if (endRow != piece.Row) {
                return false;
            }

            int columnDelta = endColumn - piece.Column;
            if (columnDelta != 2 && columnDelta != -2) {
                return false;
            }
            if (!board.IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            int rookColumn = columnDelta > 0 ? 7 : 0;
            if (endColumn == rookColumn) {
                return false; // The king would land on the rook itself.
            }
            Piece? rook = board.GetPiece(piece.Row, rookColumn);
            if (rook == null || rook.Type != 'R' || rook.Color != piece.Color || rook.HasMoved) {
                return false;
            }

            // Every square between the king and its rook must be empty, the king's target included.
            int step = columnDelta > 0 ? 1 : -1;
            for (int column = piece.Column + step; column != rookColumn; column += step) {
                if (board.GetPiece(piece.Row, column) != null) {
                    return false;
                }
            }

            // The king may not be in check, cross an attacked square or land on an attacked one.
            char enemy = piece.Color == 'W' ? 'B' : 'W';
            if (MoveRules.IsColorInCheck(board, piece.Color)) {
                return false;
            }
            if (MoveRules.IsSquareAttacked(board, piece.Row, piece.Column + step, enemy)) {
                return false;
            }
            return !MoveRules.IsSquareAttacked(board, piece.Row, endColumn, enemy);
        }

        /// <summary>
        /// Tells whether applying a move would leave any king of the mover's colour attacked. The
        /// move is applied on the board, tested and then undone, so the board is left unchanged.
        /// When the mover has no king the test is skipped and the move is accepted.
        /// </summary>
        private bool LeavesOwnKingInCheck(Piece piece, int endRow, int endColumn, bool isEnPassant, bool isCastling) {
            Board board = Board!;
            if (!MoveRules.HasKing(board, piece.Color)) {
                return false;
            }

            int startRow = piece.Row;
            int startColumn = piece.Column;
            Piece? captured = board.GetPiece(endRow, endColumn);
            Piece? capturedEnPassant = isEnPassant ? board.GetPiece(startRow, endColumn) : null;
            if (capturedEnPassant != null) {
                board.RemovePiece(startRow, endColumn);
            }

            board.RemovePiece(startRow, startColumn);
            board.SetPiece(endRow, endColumn, piece);
            piece.SetPosition(endRow, endColumn);

            // Castling also moves the rook to the square the king crossed.
            Piece? rook = null;
            int rookFromColumn = 0;
            int rookToColumn = 0;
            if (isCastling) {
                rookFromColumn = endColumn > startColumn ? 7 : 0;
                rookToColumn = endColumn > startColumn ? endColumn - 1 : endColumn + 1;
                rook = board.GetPiece(startRow, rookFromColumn);
                if (rook != null) {
                    board.RemovePiece(startRow, rookFromColumn);
                    board.SetPiece(startRow, rookToColumn, rook);
                    rook.SetPosition(startRow, rookToColumn);
                }
            }

            bool inCheck = MoveRules.IsColorInCheck(board, piece.Color);

            // Undo everything in the reverse order.
            if (rook != null) {
                rook.SetPosition(startRow, rookFromColumn);
                board.SetPiece(startRow, rookFromColumn, rook);
                board.SetPiece(startRow, rookToColumn, null);
            }
            piece.SetPosition(startRow, startColumn);
            board.SetPiece(startRow, startColumn, piece);
            board.SetPiece(endRow, endColumn, captured);
            if (capturedEnPassant != null) {
                board.SetPiece(startRow, endColumn, capturedEnPassant);
            }

            return inCheck;
        }

        /// <summary>
        /// Tells whether a colour has at least one legal move, used to detect checkmate and
        /// stalemate. The test uses the en passant opportunity of the moment.
        /// </summary>
        private bool HasAnyLegalMove(char color) {
            Board board = Board!;
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    Piece? piece = board.GetPiece(row, column);
                    if (piece != null && piece.Color == color && HasLegalMoveFrom(piece)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tells whether a piece can move to at least one square of the board.
        /// </summary>
        private bool HasLegalMoveFrom(Piece piece) {
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    if (IsLegalMove(piece, row, column)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Builds the single success line of a move that is not a check or a checkmate, following
        /// the priority of the assignment: capture, promotion, castling, en passant, plain move.
        /// An en passant capture is reported as "En passant efetuado." and not as a normal
        /// capture; otherwise that message could never be shown.
        /// </summary>
        private static string DescribeNormalMove(string pieceSymbol, Piece? captured,
            bool isPromotion, bool isCastling, bool isEnPassant) {
            if (captured != null && !isEnPassant) {
                return $"Peça {captured.Symbol} capturada.";
            }
            if (isPromotion) {
                return "Peão promovido.";
            }
            if (isCastling) {
                return "Roque efetuado.";
            }
            if (isEnPassant) {
                return "En passant efetuado.";
            }
            return $"{pieceSymbol} movida com sucesso.";
        }

        /// <summary>
        /// Looks at the position of the opponent after a move and builds the answer: checkmate when
        /// he is in check without a legal move (or has no king at all), "Check." when he is in
        /// check, otherwise the normal move line, ending the game as a draw on stalemate.
        /// </summary>
        private string DescribeMoveResult(Player mover, string pieceSymbol, Piece? captured,
            bool isPromotion, bool isCastling, bool isEnPassant) {
            Board board = Board!;
            char opponentColor = CurrentTurn;

            if (!MoveRules.HasKing(board, opponentColor)) {
                return EndWithCheckmate(mover); // A missing king is also a loss.
            }

            bool opponentInCheck = MoveRules.IsColorInCheck(board, opponentColor);
            bool opponentHasMove = HasAnyLegalMove(opponentColor);
            if (opponentInCheck && !opponentHasMove) {
                return EndWithCheckmate(mover);
            }
            if (opponentInCheck) {
                return "Check.";
            }

            string line = DescribeNormalMove(pieceSymbol, captured, isPromotion, isCastling, isEnPassant);
            if (!opponentHasMove) {
                // Stalemate: the move is still reported, but the game ends as a draw.
                WhitePlayer!.IncrementDraws();
                BlackPlayer!.IncrementDraws();
                GameInProgress = false;
            }
            return line;
        }

        /// <summary>
        /// Ends the game in favour of the mover, registering the win and the loss of the opponent.
        /// </summary>
        private string EndWithCheckmate(Player mover) {
            Player opponent = mover == WhitePlayer ? BlackPlayer! : WhitePlayer!;
            mover.IncrementWins();
            opponent.IncrementLosses();
            GameInProgress = false;
            return $"Checkmate. {mover.Name} venceu.";
        }

        /// <summary>
        /// Replaces a pawn that has reached the last row by a queen of the same colour. The index
        /// of the new queen is the highest index among the queens of that colour plus one, or 1
        /// when there is none.
        /// </summary>
        private void PromotePawn(Piece pawn) {
            Board board = Board!;
            int row = pawn.Row;
            int column = pawn.Column;
            board.RemovePiece(row, column);

            int index = NextQueenIndex(pawn.Color);
            Queen queen = new Queen(board.Grid, pawn.Color, $"{pawn.Color}Q{index}", row, column);
            board.SetPiece(row, column, queen);
        }

        /// <summary>
        /// Returns the index the next queen of a colour has to use: the highest index among the
        /// queens of that colour on the board plus one, or 1 when there is no queen.
        /// </summary>
        private int NextQueenIndex(char color) {
            Board board = Board!;
            int highest = 0;
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    Piece? piece = board.GetPiece(row, column);
                    if (piece != null && piece.Color == color && piece.Type == 'Q' && piece.Index > highest) {
                        highest = piece.Index;
                    }
                }
            }
            return highest + 1;
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
