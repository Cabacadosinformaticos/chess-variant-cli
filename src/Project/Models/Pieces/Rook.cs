using System;

namespace Chess.Models.Pieces {
    /// <summary>
    /// The rook. It moves any number of squares along a row or a column.
    /// </summary>
    public class Rook : Piece {
        /// <summary>
        /// Creates a rook placed on the given square.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the rook ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the rook, e.g. "WR1".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public Rook(Piece?[][] board, char color, string symbol, int row, int column)
            : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Normal rook moves: straight along a row or a column, with a free path and a target
        /// that is empty or holds an opponent piece. Castling is handled by the king, not here.
        /// </summary>
        public override bool CanMove(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }
            if (endRow == Row && endColumn == Column) {
                return false;
            }

            // A rook only moves straight.
            if (Row != endRow && Column != endColumn) {
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
                    return false; // A piece blocks the path.
                }
                row += rowStep;
                column += columnStep;
            }

            return true;
        }

        /// <summary>
        /// The special operation of the rook needs a final position: the square of the second
        /// captured piece.
        /// </summary>
        public override bool SpecialOperationNeedsEndPosition => true;

        /// <summary>
        /// Special operation, once per game: the rook captures two opponent pieces in a straight
        /// line. The first piece has to be on the square next to the rook, every square after it
        /// has to be empty and the final square has to hold the second piece. A king is never a
        /// valid target of the operation.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>True when the rook may capture the two pieces.</returns>
        public override bool CanUseSpecialOperation(int endRow, int endColumn) {
            if (SpecialUsed) {
                return false;
            }
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // The final square has to be on the same row or column, and at least two squares
            // away so that the two captured pieces fit in between.
            if (Row != endRow && Column != endColumn) {
                return false;
            }
            if (Math.Abs(endRow - Row) + Math.Abs(endColumn - Column) < 2) {
                return false;
            }

            int rowStep = Math.Sign(endRow - Row);
            int columnStep = Math.Sign(endColumn - Column);

            // The piece next to the rook is the first one captured.
            if (!IsCapturablePiece(Board[Row + rowStep][Column + columnStep])) {
                return false;
            }

            // Every square between the first piece and the final square has to be empty.
            int row = Row + 2 * rowStep;
            int column = Column + 2 * columnStep;
            while (row != endRow || column != endColumn) {
                if (Board[row][column] != null) {
                    return false;
                }
                row += rowStep;
                column += columnStep;
            }

            // The final square holds the second captured piece.
            return IsCapturablePiece(Board[endRow][endColumn]);
        }

        /// <summary>
        /// Tells whether a piece may be captured by the special operation: it has to be an
        /// opponent piece and it may not be a king.
        /// </summary>
        /// <param name="piece">Piece to test, or null for an empty square.</param>
        /// <returns>True when the piece can be captured.</returns>
        private bool IsCapturablePiece(Piece? piece) {
            return piece != null && piece.Color != Color && piece.Type != 'K';
        }

        /// <summary>
        /// Applies the special operation of the rook: removes the adjacent piece, lands on the
        /// final square (removing the second piece) and uses up its single operation.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>The success line of the operation.</returns>
        public override string UseSpecialOperation(int endRow, int endColumn) {
            int rowStep = Math.Sign(endRow - Row);
            int columnStep = Math.Sign(endColumn - Column);

            Board[Row + rowStep][Column + columnStep] = null; // Takes the first captured piece away.
            MoveTo(endRow, endColumn); // Lands on the final square, replacing the second piece.
            SpecialUsed = true;
            return $"Torre {Symbol} capturou duas peças com sucesso.";
        }
    }
}
