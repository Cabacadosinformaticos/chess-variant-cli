using System;

namespace Chess.Models.Pieces {
    /// <summary>
    /// The queen. She moves any number of squares along a row, a column or a diagonal.
    /// </summary>
    public class Queen : Piece {
        /// <summary>
        /// Creates a queen placed on the given square.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the queen ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the queen, e.g. "WQ1".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public Queen(Piece?[][] board, char color, string symbol, int row, int column)
            : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Normal queen moves: straight like a rook or diagonal like a bishop, with a free path
        /// and a target that is empty or holds an opponent piece.
        /// </summary>
        public override bool CanMove(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }
            if (endRow == Row && endColumn == Column) {
                return false;
            }

            int rowDelta = Math.Abs(endRow - Row);
            int columnDelta = Math.Abs(endColumn - Column);
            bool isStraight = rowDelta == 0 || columnDelta == 0;
            bool isDiagonal = rowDelta == columnDelta;
            if (!isStraight && !isDiagonal) {
                return false;
            }
            if (IsFriendlyPiece(endRow, endColumn)) {
                return false;
            }

            int rowStep = Math.Sign(endRow - Row);
            int columnStep = Math.Sign(endColumn - Column);

            int row = Row + rowStep;
            int column = Column + columnStep;
            while (row != endRow || column != endColumn) {
                if (Board[row][column] != null) {
                    return false; // A piece blocks the line.
                }
                row += rowStep;
                column += columnStep;
            }

            return true;
        }

        // TODO (later stage): special operation, any number of times: swap places with a king of
        // the same colour, only when the king is not in check before or after the swap. Prints
        // "Rainha trocou de posição com o rei.".
    }
}
