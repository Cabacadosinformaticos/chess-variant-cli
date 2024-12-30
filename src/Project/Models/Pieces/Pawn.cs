using System;

namespace Chess.Models.Pieces {
    /// <summary>
    /// The pawn. White pawns move towards row 1 (decreasing row index), black pawns towards row 8.
    /// </summary>
    public class Pawn : Piece {
        /// <summary>
        /// Creates a pawn placed on the given square.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the pawn ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the pawn, e.g. "WP3".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public Pawn(Piece?[][] board, char color, string symbol, int row, int column)
            : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Normal pawn moves: one square forward to an empty square, two squares forward from the
        /// starting row when both squares are empty, or one square diagonally forward to capture.
        /// En passant is NOT handled here, it is added in a later stage.
        /// </summary>
        public override bool CanMove(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }
            if (endRow == Row && endColumn == Column) {
                return false;
            }

            // White moves up the board (row index decreases), black moves down.
            int direction = Color == 'W' ? -1 : 1;
            int startRow = Color == 'W' ? 6 : 1;
            int rowDelta = endRow - Row;

            // One square forward, only onto an empty square.
            if (endColumn == Column && rowDelta == direction && Board[endRow][endColumn] == null) {
                return true;
            }

            // Two squares forward, only from the starting row and with both squares empty.
            if (endColumn == Column && rowDelta == 2 * direction && !HasMoved && Row == startRow
                && Board[Row + direction][Column] == null && Board[endRow][endColumn] == null) {
                return true;
            }

            // Diagonal capture of an opponent piece.
            if (Math.Abs(endColumn - Column) == 1 && rowDelta == direction
                && IsOpponentPiece(endRow, endColumn)) {
                return true;
            }

            return false;
        }

        // TODO (later stage): en passant (capture a pawn that has just moved two squares) and
        // promotion (a pawn reaching the last row becomes a queen).

        /// <summary>
        /// Special operation, once per game: the pawn steps one square backwards, towards its own
        /// side, and only onto an empty square. The pawn of a continued game starts with the
        /// operation unused.
        /// </summary>
        /// <param name="endRow">Final row of the instruction, which the pawn ignores.</param>
        /// <param name="endColumn">Final column of the instruction, which the pawn ignores.</param>
        /// <returns>True when the pawn may step back.</returns>
        public override bool CanUseSpecialOperation(int endRow, int endColumn) {
            if (SpecialUsed) {
                return false;
            }
            return Board[BackRow()][Column] == null;
        }

        /// <summary>
        /// Applies the special operation of the pawn: one square backwards, using up its single
        /// operation. The target square is empty, so nothing is captured.
        /// </summary>
        /// <param name="endRow">Final row of the instruction, which the pawn ignores.</param>
        /// <param name="endColumn">Final column of the instruction, which the pawn ignores.</param>
        /// <returns>The success line of the operation.</returns>
        public override string UseSpecialOperation(int endRow, int endColumn) {
            MoveTo(BackRow(), Column);
            SpecialUsed = true;
            return $"Peão {Symbol} recuou com sucesso.";
        }

        /// <summary>
        /// Returns the row one square backwards: the pawn moves towards its own side, which is
        /// the opposite of its normal direction.
        /// </summary>
        private int BackRow() {
            return Row + (Color == 'W' ? 1 : -1);
        }
    }
}
