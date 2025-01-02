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

        /// <summary>
        /// The special operation of the queen needs a final position: the square of the king she
        /// swaps with.
        /// </summary>
        public override bool SpecialOperationNeedsEndPosition => true;

        /// <summary>
        /// Special operation, any number of times: the queen swaps places with a king of the same
        /// colour on the final square. It is only allowed when that king is not in check before
        /// the swap; the game controller still refuses a swap that would leave the own king in
        /// check afterwards.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>True when the queen may swap with the king.</returns>
        public override bool CanUseSpecialOperation(int endRow, int endColumn) {
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // The final square has to hold a king of the same colour.
            Piece? target = Board[endRow][endColumn];
            if (target == null || target.Type != 'K' || target.Color != Color) {
                return false;
            }

            // The king may not be in check when the swap starts.
            return !MoveRules.IsColorInCheck(Board, Color);
        }

        /// <summary>
        /// Applies the special operation of the queen: queen and king change squares, on the board
        /// and in their recorded positions. Both count as moved, so a king that swapped can no
        /// longer castle. The queen keeps her operation for the rest of the game.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>The success line of the operation.</returns>
        public override string UseSpecialOperation(int endRow, int endColumn) {
            Piece king = Board[endRow][endColumn]!;
            int queenRow = Row;
            int queenColumn = Column;

            Board[queenRow][queenColumn] = king;
            Board[endRow][endColumn] = this;
            king.SetPosition(queenRow, queenColumn);
            SetPosition(endRow, endColumn);
            king.MarkAsMoved();
            MarkAsMoved();
            return "Rainha trocou de posição com o rei.";
        }
    }
}
