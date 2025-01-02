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

        /// <summary>
        /// Special operation, once per game: the bishop captures every opponent PAWN in the 3x3
        /// square centred on it and does not move. Without at least one such pawn the operation is
        /// not allowed and its single use is not consumed. A final position is not needed and, if
        /// the instruction gives one, it is ignored.
        /// </summary>
        /// <param name="endRow">Final row (0 based), ignored by this piece.</param>
        /// <param name="endColumn">Final column (0 based), ignored by this piece.</param>
        /// <returns>True when there is at least one opponent pawn to capture.</returns>
        public override bool CanUseSpecialOperation(int endRow, int endColumn) {
            return !SpecialUsed && CapturePawnsAround(false) > 0;
        }

        /// <summary>
        /// Applies the special operation of the bishop: removes the opponent pawns around it and
        /// uses up its single operation. The bishop stays where it is.
        /// </summary>
        /// <param name="endRow">Final row (0 based), ignored by this piece.</param>
        /// <param name="endColumn">Final column (0 based), ignored by this piece.</param>
        /// <returns>The success line of the operation, with the number of pawns captured.</returns>
        public override string UseSpecialOperation(int endRow, int endColumn) {
            int captured = CapturePawnsAround(true);
            SpecialUsed = true;
            return $"Bispo {Symbol} capturou {captured} peões.";
        }

        /// <summary>
        /// Counts the opponent pawns in the 3x3 square around the bishop and, when asked, removes
        /// them from the board. Own pieces, other types of piece and the bishop's own square are
        /// left alone.
        /// </summary>
        /// <param name="capture">True to remove the pawns, false only to count them.</param>
        /// <returns>Number of opponent pawns in that square.</returns>
        private int CapturePawnsAround(bool capture) {
            int count = 0;
            for (int row = Row - 1; row <= Row + 1; row++) {
                for (int column = Column - 1; column <= Column + 1; column++) {
                    if ((row == Row && column == Column) || !IsWithinBounds(row, column)) {
                        continue;
                    }

                    Piece? piece = Board[row][column];
                    if (piece != null && piece.Color != Color && piece.Type == 'P') {
                        if (capture) {
                            Board[row][column] = null;
                        }
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
