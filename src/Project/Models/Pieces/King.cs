using System;

namespace Chess.Models.Pieces {
    /// <summary>
    /// The king. He moves a single square in any direction.
    /// </summary>
    public class King : Piece {
        /// <summary>
        /// Creates a king placed on the given square.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the king ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the king, e.g. "WK1".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public King(Piece?[][] board, char color, string symbol, int row, int column)
            : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Normal king moves: one square in any direction, landing on an empty square or on an
        /// opponent piece. Castling and the rule that the king cannot move into check are
        /// handled by the game controller, which sees the whole board.
        /// </summary>
        public override bool CanMove(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            int rowDelta = Math.Abs(endRow - Row);
            int columnDelta = Math.Abs(endColumn - Column);
            bool isOneSquare = rowDelta <= 1 && columnDelta <= 1 && (rowDelta + columnDelta) > 0;

            return isOneSquare && !IsFriendlyPiece(endRow, endColumn);
        }

        // The king has no special operation: CanUseSpecialOperation keeps its base answer (false),
        // so the game controller answers "Movimento inválido." for an "OS" on a king.
    }
}
