using System;
using System.Collections.Generic;
using Chess.Models.Pieces;

namespace Chess.Models {
    /// <summary>
    /// The 8x8 board. Row 0 is the top row of the printed board (black back rank, printed row 1)
    /// and row 7 is the bottom row (white back rank, printed row 8).
    /// </summary>
    public class Board {
        private const int Size = 8;

        /// <summary>
        /// Grid with every square of the board. A square is null when it is empty.
        /// </summary>
        public Piece?[][] Grid { get; private set; }

        /// <summary>
        /// Creates an empty board.
        /// </summary>
        public Board() {
            Grid = new Piece?[Size][];
            for (int i = 0; i < Size; i++) {
                Grid[i] = new Piece?[Size];
            }
        }

        /// <summary>
        /// Places the 32 pieces of a new game on the board, in the standard starting position
        /// given in the assignment.
        /// </summary>
        public void InitializeStandardSetup() {
            // Black pieces on row 0.
            Grid[0][0] = new Rook(Grid, 'B', "BR1", 0, 0);
            Grid[0][1] = new Horse(Grid, 'B', "BH1", 0, 1);
            Grid[0][2] = new Bishop(Grid, 'B', "BB1", 0, 2);
            Grid[0][3] = new Queen(Grid, 'B', "BQ1", 0, 3);
            Grid[0][4] = new King(Grid, 'B', "BK1", 0, 4);
            Grid[0][5] = new Bishop(Grid, 'B', "BB2", 0, 5);
            Grid[0][6] = new Horse(Grid, 'B', "BH2", 0, 6);
            Grid[0][7] = new Rook(Grid, 'B', "BR2", 0, 7);

            for (int column = 0; column < Size; column++) {
                Grid[1][column] = new Pawn(Grid, 'B', $"BP{column + 1}", 1, column);
            }

            // White pieces on row 7.
            Grid[7][0] = new Rook(Grid, 'W', "WR1", 7, 0);
            Grid[7][1] = new Horse(Grid, 'W', "WH1", 7, 1);
            Grid[7][2] = new Bishop(Grid, 'W', "WB1", 7, 2);
            Grid[7][3] = new Queen(Grid, 'W', "WQ1", 7, 3);
            Grid[7][4] = new King(Grid, 'W', "WK1", 7, 4);
            Grid[7][5] = new Bishop(Grid, 'W', "WB2", 7, 5);
            Grid[7][6] = new Horse(Grid, 'W', "WH2", 7, 6);
            Grid[7][7] = new Rook(Grid, 'W', "WR2", 7, 7);

            for (int column = 0; column < Size; column++) {
                Grid[6][column] = new Pawn(Grid, 'W', $"WP{column + 1}", 6, column);
            }
        }

        /// <summary>
        /// Fills the board from the 8 lines of an "IJ Continuação" board. Every line holds 8
        /// comma separated cells; a cell that is not a valid symbol is left empty.
        /// </summary>
        /// <param name="lines">The 8 board lines, top line first.</param>
        public void LoadFromLines(IReadOnlyList<string> lines) {
            for (int row = 0; row < Size; row++) {
                string line = row < lines.Count ? lines[row] : string.Empty;
                string[] cells = line.Split(',');
                for (int column = 0; column < Size; column++) {
                    string text = column < cells.Length ? cells[column] : string.Empty;
                    Grid[row][column] = CreatePiece(text, row, column);
                }
            }
        }

        /// <summary>
        /// Builds a piece of the right type from a symbol, or null when the text is not a valid
        /// symbol. It is used by a continued game and by the loading of a save file; pieces built
        /// this way start with HasMoved and SpecialUsed false and are placed on the given square.
        /// </summary>
        /// <param name="text">Symbol of the piece, e.g. "WP3".</param>
        /// <param name="row">Row where the piece is created (0 based).</param>
        /// <param name="column">Column where the piece is created (0 based).</param>
        /// <returns>The new piece, or null when the text is not a valid symbol.</returns>
        public Piece? CreatePiece(string text, int row, int column) {
            if (!TryParseSymbol(text, out char color, out char type, out _)) {
                return null;
            }

            switch (type) {
                case 'P': return new Pawn(Grid, color, text, row, column);
                case 'R': return new Rook(Grid, color, text, row, column);
                case 'H': return new Horse(Grid, color, text, row, column);
                case 'B': return new Bishop(Grid, color, text, row, column);
                case 'Q': return new Queen(Grid, color, text, row, column);
                case 'K': return new King(Grid, color, text, row, column);
                default: return null;
            }
        }

        /// <summary>
        /// Checks the symbol format: [WB][PRHBQK][1-9][0-9]*.
        /// </summary>
        /// <param name="text">Text of a board cell.</param>
        /// <param name="color">Colour read from the symbol.</param>
        /// <param name="type">Type read from the symbol.</param>
        /// <param name="index">Index read from the symbol.</param>
        /// <returns>True when the text is a valid piece symbol.</returns>
        public static bool TryParseSymbol(string text, out char color, out char type, out int index) {
            color = ' ';
            type = ' ';
            index = 0;

            if (text == null || text.Length < 3) {
                return false;
            }
            if (text[0] != 'W' && text[0] != 'B') {
                return false;
            }
            if ("PRHBQK".IndexOf(text[1]) < 0) {
                return false;
            }
            if (text[2] < '1' || text[2] > '9') {
                return false;
            }

            int value = 0;
            for (int i = 2; i < text.Length; i++) {
                if (text[i] < '0' || text[i] > '9') {
                    return false;
                }
                int digit = text[i] - '0';
                if (value > (int.MaxValue - digit) / 10) {
                    return false; // Index would not fit in an int.
                }
                value = value * 10 + digit;
            }

            color = text[0];
            type = text[1];
            index = value;
            return true;
        }

        /// <summary>
        /// Checks whether a row and column are inside the board.
        /// </summary>
        public bool IsWithinBounds(int row, int column) {
            return row >= 0 && row < Size && column >= 0 && column < Size;
        }

        /// <summary>
        /// Returns the piece on a square, or null when the square is empty or outside the board.
        /// </summary>
        public Piece? GetPiece(int row, int column) {
            return IsWithinBounds(row, column) ? Grid[row][column] : null;
        }

        /// <summary>
        /// Places a piece on a square. Used when setting up a game.
        /// </summary>
        public void SetPiece(int row, int column, Piece? piece) {
            if (IsWithinBounds(row, column)) {
                Grid[row][column] = piece;
            }
        }

        /// <summary>
        /// Empties a square.
        /// </summary>
        public void RemovePiece(int row, int column) {
            if (IsWithinBounds(row, column)) {
                Grid[row][column] = null;
            }
        }

        /// <summary>
        /// Renders the board exactly as the "DJ" instruction describes: column letters header,
        /// then one line per row with the printed row number, two spaces and the padded cells.
        /// </summary>
        /// <returns>The lines of the board, ready to be joined and printed.</returns>
        public List<string> GetDisplayLines() {
            List<string> lines = new List<string>();
            lines.Add("    A    B    C    D    E    F    G    H");

            for (int row = 0; row < Size; row++) {
                string[] cells = new string[Size];
                for (int column = 0; column < Size; column++) {
                    Piece? piece = Grid[row][column];
                    cells[column] = (piece != null ? piece.Symbol : string.Empty).PadRight(3);
                }
                lines.Add((row + 1) + "  " + string.Join("  ", cells).TrimEnd());
            }

            return lines;
        }
    }
}
