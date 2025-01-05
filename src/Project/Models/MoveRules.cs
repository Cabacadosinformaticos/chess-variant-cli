using System;

namespace Chess.Models {
    /// <summary>
    /// Rules that need to look at the whole board: which squares are attacked, whether a colour is
    /// in check and whether a colour still has a king. The game controller uses them to reject the
    /// moves that would leave the own king in check and to detect check, checkmate and stalemate.
    /// </summary>
    public static class MoveRules {
        /// <summary>
        /// Tells whether a square is attacked by any piece of the given colour. A pawn attacks only
        /// the two diagonal squares in front of it (exactly the squares it captures on), and an
        /// empty square can be attacked, which is what the castling rule needs.
        /// </summary>
        /// <param name="board">Board to inspect.</param>
        /// <param name="row">Row of the square (0 based).</param>
        /// <param name="column">Column of the square (0 based).</param>
        /// <param name="attackerColor">Colour whose pieces are attacking.</param>
        /// <returns>True when at least one piece of that colour attacks the square.</returns>
        public static bool IsSquareAttacked(Board board, int row, int column, char attackerColor) {
            return IsSquareAttacked(board.Grid, row, column, attackerColor);
        }

        /// <summary>
        /// Tells whether a square is attacked by any piece of the given colour, working directly
        /// on the grid. Pieces only know the grid they live on, so this overload lets a piece ask
        /// the same question without a Board object.
        /// </summary>
        /// <param name="grid">Grid of the board to inspect.</param>
        /// <param name="row">Row of the square (0 based).</param>
        /// <param name="column">Column of the square (0 based).</param>
        /// <param name="attackerColor">Colour whose pieces are attacking.</param>
        /// <returns>True when at least one piece of that colour attacks the square.</returns>
        public static bool IsSquareAttacked(Piece?[][] grid, int row, int column, char attackerColor) {
            for (int pieceRow = 0; pieceRow < 8; pieceRow++) {
                for (int pieceColumn = 0; pieceColumn < 8; pieceColumn++) {
                    Piece? piece = grid[pieceRow][pieceColumn];
                    if (piece != null && piece.Color == attackerColor
                        && AttacksSquare(grid, piece, row, column)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tells whether a colour is in check: at least one of its kings is attacked. A colour
        /// without a king is never in check, because there is nothing to attack.
        /// </summary>
        /// <param name="board">Board to inspect.</param>
        /// <param name="color">Colour to test ('W' or 'B').</param>
        /// <returns>True when any king of that colour is attacked.</returns>
        public static bool IsColorInCheck(Board board, char color) {
            return IsColorInCheck(board.Grid, color);
        }

        /// <summary>
        /// Tells whether a colour is in check, working directly on the grid. The pieces use this
        /// overload to test their own king before and after a special operation.
        /// </summary>
        /// <param name="grid">Grid of the board to inspect.</param>
        /// <param name="color">Colour to test ('W' or 'B').</param>
        /// <returns>True when any king of that colour is attacked.</returns>
        public static bool IsColorInCheck(Piece?[][] grid, char color) {
            char enemy = EnemyOf(color);
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    Piece? piece = grid[row][column];
                    if (piece != null && piece.Color == color && piece.Type == 'K'
                        && IsSquareAttacked(grid, row, column, enemy)) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Tells whether a colour still has at least one king on the board. Without a king the
        /// check filter is skipped, since a board of a continued game may have no king.
        /// </summary>
        /// <param name="board">Board to inspect.</param>
        /// <param name="color">Colour to test ('W' or 'B').</param>
        /// <returns>True when the colour has a king.</returns>
        public static bool HasKing(Board board, char color) {
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    Piece? piece = board.GetPiece(row, column);
                    if (piece != null && piece.Color == color && piece.Type == 'K') {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the colour that plays against the given one.
        /// </summary>
        private static char EnemyOf(char color) {
            return color == 'W' ? 'B' : 'W';
        }

        /// <summary>
        /// Tells whether a piece attacks a square, using the capture geometry of its type. The
        /// square may be empty or hold any piece: this is not a legal move test.
        /// </summary>
        private static bool AttacksSquare(Piece?[][] grid, Piece piece, int row, int column) {
            if (row == piece.Row && column == piece.Column) {
                return false;
            }

            switch (piece.Type) {
                case 'P':
                    return IsPawnAttack(piece, row, column);
                case 'R':
                    return IsStraight(piece, row, column) && IsPathClear(grid, piece, row, column);
                case 'B':
                    return IsDiagonal(piece, row, column) && IsPathClear(grid, piece, row, column);
                case 'Q':
                    return (IsStraight(piece, row, column) || IsDiagonal(piece, row, column))
                        && IsPathClear(grid, piece, row, column);
                case 'H':
                    return IsHorseMove(piece, row, column);
                case 'K':
                    return Math.Abs(row - piece.Row) <= 1 && Math.Abs(column - piece.Column) <= 1;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Tells whether a pawn attacks a square: the two diagonal squares in front of it. White
        /// pawns go towards row 1 (decreasing row), black pawns towards row 8.
        /// </summary>
        private static bool IsPawnAttack(Piece pawn, int row, int column) {
            int direction = pawn.Color == 'W' ? -1 : 1;
            return row == pawn.Row + direction && Math.Abs(column - pawn.Column) == 1;
        }

        /// <summary>
        /// Tells whether a square lies on the same row or the same column as the piece.
        /// </summary>
        private static bool IsStraight(Piece piece, int row, int column) {
            return piece.Row == row || piece.Column == column;
        }

        /// <summary>
        /// Tells whether a square lies on a diagonal of the piece.
        /// </summary>
        private static bool IsDiagonal(Piece piece, int row, int column) {
            return Math.Abs(row - piece.Row) == Math.Abs(column - piece.Column);
        }

        /// <summary>
        /// Tells whether a square is an L shaped horse move away from the piece.
        /// </summary>
        private static bool IsHorseMove(Piece piece, int row, int column) {
            int rowDelta = Math.Abs(row - piece.Row);
            int columnDelta = Math.Abs(column - piece.Column);
            return (rowDelta == 2 && columnDelta == 1) || (rowDelta == 1 && columnDelta == 2);
        }

        /// <summary>
        /// Tells whether every square strictly between a piece and a target square is empty. The
        /// square of the piece and the target square itself are not checked.
        /// </summary>
        private static bool IsPathClear(Piece?[][] grid, Piece piece, int row, int column) {
            int rowStep = Math.Sign(row - piece.Row);
            int columnStep = Math.Sign(column - piece.Column);

            int currentRow = piece.Row + rowStep;
            int currentColumn = piece.Column + columnStep;
            while (currentRow != row || currentColumn != column) {
                if (grid[currentRow][currentColumn] != null) {
                    return false;
                }
                currentRow += rowStep;
                currentColumn += columnStep;
            }
            return true;
        }
    }
}
