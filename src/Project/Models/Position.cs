namespace Chess.Models {
    /// <summary>
    /// A square of the board written the way the assignment does: a column letter A-H followed
    /// by a row number 1-8, e.g. "E4". Stored as 0 based row and column indices.
    /// </summary>
    public readonly struct Position {
        /// <summary>
        /// Row index, 0 for printed row 1 (top) up to 7 for printed row 8 (bottom).
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Column index, 0 for column A up to 7 for column H.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Creates a position from 0 based indices.
        /// </summary>
        public Position(int row, int column) {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Parses a position token. The token is valid only when it has exactly two characters:
        /// an uppercase letter A-H and a digit 1-8.
        /// </summary>
        /// <param name="token">Token read from the input, e.g. "E4".</param>
        /// <param name="position">Parsed position, valid only when the method returns true.</param>
        /// <returns>True when the token is a valid position.</returns>
        public static bool TryParse(string? token, out Position position) {
            position = default;

            if (token == null || token.Length != 2) {
                return false;
            }

            char letter = token[0];
            char digit = token[1];
            if (letter < 'A' || letter > 'H') {
                return false;
            }
            if (digit < '1' || digit > '8') {
                return false;
            }

            position = new Position(digit - '1', letter - 'A');
            return true;
        }

        /// <summary>
        /// Writes the position back as a board token, e.g. "E4".
        /// </summary>
        public override string ToString() {
            return string.Concat((char)('A' + Column), (char)('1' + Row));
        }
    }
}
