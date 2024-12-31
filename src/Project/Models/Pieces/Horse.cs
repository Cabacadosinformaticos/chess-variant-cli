using System;

namespace Chess.Models.Pieces {
    /// <summary>
    /// The horse (knight). It moves in an L: two squares in one direction and one square in a
    /// perpendicular direction, jumping over any piece in between.
    /// </summary>
    public class Horse : Piece {
        /// <summary>
        /// Creates a horse placed on the given square.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the horse ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the horse, e.g. "WH1".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public Horse(Piece?[][] board, char color, string symbol, int row, int column)
            : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Normal horse moves: an L shape, landing on an empty square or on an opponent piece.
        /// The squares in between are ignored because the horse jumps.
        /// </summary>
        public override bool CanMove(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            int rowDelta = Math.Abs(endRow - Row);
            int columnDelta = Math.Abs(endColumn - Column);
            bool isLShape = (rowDelta == 2 && columnDelta == 1) || (rowDelta == 1 && columnDelta == 2);

            return isLShape && !IsFriendlyPiece(endRow, endColumn);
        }

        /// <summary>
        /// The special operation of the horse needs a final position: the square it jumps to.
        /// </summary>
        public override bool SpecialOperationNeedsEndPosition => true;

        /// <summary>
        /// Special operation, any number of times: the horse may only jump when none of the eight
        /// squares around it holds a piece. It then advances exactly four squares in one of the
        /// eight straight directions (row, column or diagonal), jumping over anything, and lands
        /// on an empty square or on an opponent piece that is not a king.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>True when the horse may jump.</returns>
        public override bool CanUseSpecialOperation(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // The horse may not be touching any piece, of either colour.
            for (int row = Row - 1; row <= Row + 1; row++) {
                for (int column = Column - 1; column <= Column + 1; column++) {
                    if ((row != Row || column != Column) && IsWithinBounds(row, column)
                        && Board[row][column] != null) {
                        return false;
                    }
                }
            }

            // The jump is four squares in a straight line, horizontal, vertical or diagonal.
            int rowDelta = endRow - Row;
            int columnDelta = endColumn - Column;
            bool straight = (rowDelta == 0 && Math.Abs(columnDelta) == 4)
                || (Math.Abs(rowDelta) == 4 && columnDelta == 0);
            bool diagonal = Math.Abs(rowDelta) == 4 && Math.Abs(columnDelta) == 4;
            if (!straight && !diagonal) {
                return false;
            }

            // The final square has to be empty or hold an opponent piece that is not a king.
            Piece? target = Board[endRow][endColumn];
            return target == null || (target.Color != Color && target.Type != 'K');
        }

        /// <summary>
        /// Applies the special operation of the horse: it lands on the final square, capturing an
        /// opponent piece if there is one. The horse keeps its operation for the rest of the game.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>The success line of the operation.</returns>
        public override string UseSpecialOperation(int endRow, int endColumn) {
            MoveTo(endRow, endColumn); // Captures the piece on the final square, if there is one.
            return $"Cavalo {Symbol} avançou com sucesso.";
        }
    }
}
