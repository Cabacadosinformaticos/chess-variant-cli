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

        // TODO (later stage): special operation, any number of times: allowed only when none of the
        // 8 surrounding squares holds a piece; then advance exactly 4 squares in one of the 8
        // straight directions, jumping over anything, capturing an opponent piece on the target.
        // Prints "Cavalo NomePeça avançou com sucesso.".
    }
}
