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

        // TODO (later stage): special operation, once per game: capture two opponent pieces in a
        // straight line, the first one adjacent to the rook and the second one on the target
        // square, printing "Torre NomePeça capturou duas peças com sucesso.".
    }
}
