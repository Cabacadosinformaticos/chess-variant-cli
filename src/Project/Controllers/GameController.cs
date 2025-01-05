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
        /// Square the double stepped pawn of the last move skipped, or null when the last move did
        /// not create an en passant opportunity. The persistence controller uses it to save the
        /// game in progress.
        /// </summary>
        public Position? EnPassantTarget => enPassantTarget;

        /// <summary>
        /// Pawn that has just stepped two squares and may be captured en passant, or null when
        /// there is no such pawn. The persistence controller uses it to save the game in progress.
        /// </summary>
        public Piece? EnPassantPawn => enPassantPawn;

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
        /// Replaces the game in progress with a game read from a save file. The two players have
        /// to be the registry's own objects, so that later results update the registry.
        /// </summary>
        /// <param name="whitePlayer">Player with the white pieces.</param>
        /// <param name="blackPlayer">Player with the black pieces.</param>
        /// <param name="board">Board of the loaded game.</param>
        /// <param name="turn">Colour that has to play, 'W' or 'B'.</param>
        /// <param name="enPassantTarget">Skipped square of the en passant opportunity, or null.</param>
        /// <param name="enPassantPawn">Pawn that may be captured en passant, or null.</param>
        public void RestoreGame(Player whitePlayer, Player blackPlayer, Board board, char turn,
                Position? enPassantTarget, Piece? enPassantPawn) {
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
            Board = board;
            CurrentTurn = turn;
            GameInProgress = true;
            this.enPassantTarget = enPassantTarget;
            this.enPassantPawn = enPassantPawn;
        }

        /// <summary>
        /// Forgets the game in progress. It is used when a loaded save file has no game.
        /// </summary>
        public void ClearGame() {
            WhitePlayer = null;
            BlackPlayer = null;
            Board = null;
            GameInProgress = false;
            ClearEnPassant();
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
            ClearEnPassant();

            Piece? captured;
            if (isEnPassant) {
                captured = Board.GetPiece(startRow, end.Column);
                Board.RemovePiece(startRow, end.Column);
                piece.MoveTo(end.Row, end.Column);
            } else if (isCastling) {
                captured = null;
                int rookFromColumn = FindCastlingRookColumn(piece, end.Column > startColumn ? 1 : -1);
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
            SwitchTurn();
            return DescribeMoveResult(mover, piece.Symbol, captured, isPromotion, isCastling, isEnPassant);
        }

        /// <summary>
        /// Performs the special operation of a piece: a pawn steps one square backwards, a rook
        /// captures two opponent pieces in a straight line, a horse jumps four squares, a bishop
        /// captures the opponent pawns around it and a queen swaps places with its king. The
        /// operation is refused when the rules of the piece do not allow it or when it would leave
        /// the mover's own king in check. On success the turn passes and, as after a normal move,
        /// a checkmate or a check adds a second line to the answer.
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

            Piece? piece = Board.GetPiece(start.Row, start.Column);
            if (piece == null) {
                return "Não existe peça na posição inicial.";
            }

            // A token that is given always has to be a valid position, and the pieces whose
            // operation uses it may not be called without one.
            bool hasEnd = endToken != null;
            Position end = default;
            if (hasEnd && !Position.TryParse(endToken, out end)) {
                return "Posição final inválida.";
            }
            if (!hasEnd && piece.SpecialOperationNeedsEndPosition) {
                return "Posição final inválida.";
            }

            if (piece.Color != CurrentTurn) {
                return "Movimento inválido."; // A player can only use his own pieces.
            }
            if (!piece.CanUseSpecialOperation(end.Row, end.Column)) {
                return "Movimento inválido.";
            }

            Player mover = CurrentPlayer();
            string? line = TryPerformSpecialOperation(piece, end.Row, end.Column);
            if (line == null) {
                return "Movimento inválido."; // The operation would leave the own king in check.
            }

            // A turn is passing, so the previous en passant opportunity is gone.
            ClearEnPassant();
            SwitchTurn();
            return AddCheckResult(mover, line);
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
        /// Finds the column of the first piece on the king's row in the given direction, which
        /// is the rook a castling would use. Returns -1 when the row is empty up to the edge.
        /// </summary>
        /// <param name="king">The king that castles.</param>
        /// <param name="step">+1 to look towards column H, -1 towards column A.</param>
        /// <returns>The column of that piece, or -1.</returns>
        private int FindCastlingRookColumn(Piece king, int step) {
            for (int column = king.Column + step; column >= 0 && column < 8; column += step) {
                if (Board!.GetPiece(king.Row, column) != null) {
                    return column;
                }
            }
            return -1;
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

            // The rook is the first piece the king meets in that direction, so every square
            // between them is empty. The king must stop before reaching it.
            int step = columnDelta > 0 ? 1 : -1;
            int rookColumn = FindCastlingRookColumn(piece, step);
            if (rookColumn < 0 || Math.Abs(rookColumn - piece.Column) <= Math.Abs(columnDelta)) {
                return false;
            }
            Piece? rook = board.GetPiece(piece.Row, rookColumn);
            if (rook == null || rook.Type != 'R' || rook.Color != piece.Color || rook.HasMoved) {
                return false;
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
                rookFromColumn = FindCastlingRookColumn(piece, endColumn > startColumn ? 1 : -1);
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
            string? ending = DescribeCheckAndCheckmate(mover);
            if (ending != null) {
                return ending;
            }

            string line = DescribeNormalMove(pieceSymbol, captured, isPromotion, isCastling, isEnPassant);
            if (!HasAnyLegalMove(CurrentTurn)) {
                // Stalemate: the move is still reported, but the game ends as a draw.
                WhitePlayer!.IncrementDraws();
                BlackPlayer!.IncrementDraws();
                GameInProgress = false;
            }
            return line;
        }

        /// <summary>
        /// Looks at the player who has just received the turn: he has no king, or is in check
        /// without any legal move, means checkmate and the game ends in favour of the mover; being
        /// only in check means "Check.". Returns null while there is nothing special to report.
        /// </summary>
        /// <param name="mover">Player who made the move or the operation.</param>
        /// <returns>The extra line, or null when the opponent is not in check.</returns>
        private string? DescribeCheckAndCheckmate(Player mover) {
            Board board = Board!;
            char opponentColor = CurrentTurn;

            if (!MoveRules.HasKing(board, opponentColor)) {
                return EndWithCheckmate(mover); // A missing king is also a loss.
            }
            if (!MoveRules.IsColorInCheck(board, opponentColor)) {
                return null;
            }
            if (!HasAnyLegalMove(opponentColor)) {
                return EndWithCheckmate(mover);
            }
            return "Check.";
        }

        /// <summary>
        /// Adds the second line that a successful special operation may need: "Checkmate. Nome
        /// venceu." (which also ends the game) when the opponent has no answer, or "Check." when
        /// he is only in check.
        /// </summary>
        /// <param name="mover">Player who used the operation.</param>
        /// <param name="firstLine">Success line of the operation.</param>
        /// <returns>The answer, with a second line when there is one.</returns>
        private string AddCheckResult(Player mover, string firstLine) {
            string? checkLine = DescribeCheckAndCheckmate(mover);
            return checkLine == null ? firstLine : firstLine + "\n" + checkLine;
        }

        /// <summary>
        /// Forgets the en passant opportunity of the last move, because a turn is passing.
        /// </summary>
        private void ClearEnPassant() {
            enPassantTarget = null;
            enPassantPawn = null;
        }

        /// <summary>
        /// Gives the turn to the other player.
        /// </summary>
        private void SwitchTurn() {
            CurrentTurn = CurrentTurn == 'W' ? 'B' : 'W';
        }

        /// <summary>
        /// Performs a special operation that the rules of the piece already accepted, but only
        /// when the operation does not leave the mover's own king attacked. The operation is
        /// applied, tested and undone when the king would be in check, so a refused operation
        /// leaves the board and the flags of the piece exactly as they were.
        /// </summary>
        /// <param name="piece">Piece using the operation.</param>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>The success line, or null when the operation had to be undone.</returns>
        private string? TryPerformSpecialOperation(Piece piece, int endRow, int endColumn) {
            Board board = Board!;
            if (!MoveRules.HasKing(board, piece.Color)) {
                // Without a king there is nothing to check, so the operation is always accepted.
                return piece.UseSpecialOperation(endRow, endColumn);
            }

            BoardBackup backup = new BoardBackup(board);
            string line = piece.UseSpecialOperation(endRow, endColumn);
            if (!MoveRules.IsColorInCheck(board, piece.Color)) {
                return line;
            }

            backup.Restore(board);
            return null;
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

        /// <summary>
        /// Copy of everything a special operation can change: the squares of the board and the
        /// state of every piece. It is taken before an operation is applied and used to undo it
        /// when the operation would leave the mover's own king in check.
        /// </summary>
        private sealed class BoardBackup {
            private readonly Piece?[][] grid;
            private readonly List<PieceState> pieces;

            /// <summary>
            /// Copies the squares and the state of the pieces of a board.
            /// </summary>
            /// <param name="board">Board to copy.</param>
            public BoardBackup(Board board) {
                grid = new Piece?[8][];
                pieces = new List<PieceState>();
                for (int row = 0; row < 8; row++) {
                    grid[row] = new Piece?[8];
                    for (int column = 0; column < 8; column++) {
                        Piece? piece = board.Grid[row][column];
                        grid[row][column] = piece;
                        if (piece != null) {
                            pieces.Add(new PieceState(piece));
                        }
                    }
                }
            }

            /// <summary>
            /// Puts the saved squares and piece states back on the board.
            /// </summary>
            /// <param name="board">Board to restore.</param>
            public void Restore(Board board) {
                for (int row = 0; row < 8; row++) {
                    for (int column = 0; column < 8; column++) {
                        board.Grid[row][column] = grid[row][column];
                    }
                }
                foreach (PieceState state in pieces) {
                    state.Restore();
                }
            }
        }

        /// <summary>
        /// Saved state of one piece: where it is and which of its flags are set. The game
        /// controller stores it before simulating a special operation, so that the operation can
        /// be undone without leaving any trace.
        /// </summary>
        private readonly struct PieceState {
            private readonly Piece piece;
            private readonly int row;
            private readonly int column;
            private readonly bool hasMoved;
            private readonly bool specialUsed;

            /// <summary>
            /// Saves the current state of a piece.
            /// </summary>
            /// <param name="piece">Piece to save.</param>
            public PieceState(Piece piece) {
                this.piece = piece;
                row = piece.Row;
                column = piece.Column;
                hasMoved = piece.HasMoved;
                specialUsed = piece.SpecialUsed;
            }

            /// <summary>
            /// Puts the saved position and flags back into the piece.
            /// </summary>
            public void Restore() {
                piece.RestoreState(row, column, hasMoved, specialUsed);
            }
        }
    }
}
