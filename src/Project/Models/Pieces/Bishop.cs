using System;

namespace Chess.Models.Pieces {
    /// <summary>
    /// The bishop. It moves any number of squares along a diagonal.
    /// </summary>
    public class Bishop : Piece {
        /// <summary>
        /// Creates a bishop placed on the given square.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the bishop ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the bishop, e.g. "WB1".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public Bishop(Piece?[][] board, char color, string symbol, int row, int column)
            : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Normal bishop moves: along a diagonal, with a free path and a target that is empty
        /// or holds an opponent piece.
        /// </summary>
        public override bool CanMove(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            int rowDelta = Math.Abs(endRow - Row);
            int columnDelta = Math.Abs(endColumn - Column);
            if (rowDelta != columnDelta || rowDelta == 0) {
                return false;
            }
            if (IsFriendlyPiece(endRow, endColumn)) {
                return false;
            }

            int rowStep = Math.Sign(endRow - Row);
            int columnStep = Math.Sign(endColumn - Column);

            int row = Row + rowStep;
            int column = Column + columnStep;
            while (row != endRow) {
                if (Board[row][column] != null) {
                    return false; // A piece blocks the diagonal.
                }
                row += rowStep;
                column += columnStep;
            }

            return true;
        }

        // TODO (later stage): special operation, once per game: capture every opponent PAWN in the
        // 3x3 square centred on the bishop; the bishop does not move. It is only valid when at
        // least one pawn is captured, and prints "Bispo NomePeça capturou N peões.".
    }
}
