using System;
using System.Collections.Generic;

namespace Chess.Models {
    /// <summary>
    /// Base class for every piece on the board. A piece knows its colour, its type and index,
    /// where it is, and which of its special operations have already been used.
    /// </summary>
    public class Piece {
        /// <summary>
        /// Symbol of the piece: colour (W/B) + type (P/R/H/B/Q/K) + index, e.g. "BP1" or "WQ10".
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Colour of the piece, 'W' for white and 'B' for black.
        /// </summary>
        public char Color { get; }

        /// <summary>
        /// Type of the piece: 'P', 'R', 'H', 'B', 'Q' or 'K' (second character of the symbol).
        /// </summary>
        public char Type { get; }

        /// <summary>
        /// Positive index of the piece inside the set of pieces of the same colour and type.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Current row (0 based, 0 is the top row of the printed board).
        /// </summary>
        public int Row { get; protected set; }

        /// <summary>
        /// Current column (0 based, 0 is column A).
        /// </summary>
        public int Column { get; protected set; }

        /// <summary>
        /// True once the piece has been moved in the current game. Used by castling and by the
        /// pawn double step.
        /// </summary>
        public bool HasMoved { get; protected set; }

        /// <summary>
        /// True once the piece has used its once-per-game special operation (pawn, rook, bishop).
        /// </summary>
        public bool SpecialUsed { get; protected set; }

        /// <summary>
        /// Grid the piece belongs to, shared with every other piece of the same board.
        /// </summary>
        public Piece?[][] Board { get; }

        /// <summary>
        /// Creates a piece positioned on the given square of the given grid.
        /// </summary>
        /// <param name="board">Grid shared by all the pieces of the game.</param>
        /// <param name="color">Colour of the piece ('W' or 'B').</param>
        /// <param name="symbol">Symbol of the piece, e.g. "BP1".</param>
        /// <param name="row">Initial row (0 based).</param>
        /// <param name="column">Initial column (0 based).</param>
        public Piece(Piece?[][] board, char color, string symbol, int row, int column) {
            Board = board;
            Color = color;
            Symbol = symbol;
            Row = row;
            Column = column;
            Type = symbol.Length > 1 ? symbol[1] : ' ';
            Index = ParseIndex(symbol);
            HasMoved = false;
            SpecialUsed = false;
        }

        /// <summary>
        /// Reads the numeric part of a symbol (everything after the colour and the type).
        /// </summary>
        private static int ParseIndex(string symbol) {
            int value = 0;
            for (int i = 2; i < symbol.Length; i++) {
                if (symbol[i] < '0' || symbol[i] > '9') {
                    break;
                }
                value = value * 10 + (symbol[i] - '0');
            }
            return value;
        }

        /// <summary>
        /// Checks whether a move target is a pseudo-legal normal move for this piece: the geometry
        /// of the piece fits, the path is free and the target is empty or holds an opponent piece.
        /// Check, castling, en passant, promotion and special operations are NOT handled here.
        /// </summary>
        /// <param name="endRow">Target row (0 based).</param>
        /// <param name="endColumn">Target column (0 based).</param>
        /// <returns>True when the normal move geometry allows the move.</returns>
        public virtual bool CanMove(int endRow, int endColumn) {
            return false;
        }

        /// <summary>
        /// Lists every square this piece could move to with a normal move. It is built from
        /// <see cref="CanMove"/>, so subclasses only have to implement the move geometry.
        /// Castling and en passant are recognised by the game controller, not here.
        /// </summary>
        /// <returns>List of (row, column) targets.</returns>
        public virtual List<(int Row, int Column)> GetValidMoves() {
            List<(int, int)> moves = new List<(int, int)>();
            for (int row = 0; row < 8; row++) {
                for (int column = 0; column < 8; column++) {
                    if (CanMove(row, column)) {
                        moves.Add((row, column));
                    }
                }
            }
            return moves;
        }

        /// <summary>
        /// Applies a move that was already validated with <see cref="CanMove"/>: clears the origin,
        /// captures whatever is on the target and updates the position of the piece.
        /// Promotion, en passant and castling are applied by the game controller.
        /// </summary>
        /// <param name="endRow">Target row (0 based).</param>
        /// <param name="endColumn">Target column (0 based).</param>
        /// <returns>The captured piece, or null when the target was empty.</returns>
        public Piece? Move(int endRow, int endColumn) {
            if (!CanMove(endRow, endColumn)) {
                throw new InvalidOperationException("Invalid move for this piece.");
            }

            Board[Row][Column] = null;
            Piece? captured = Board[endRow][endColumn];
            Board[endRow][endColumn] = this;
            Row = endRow;
            Column = endColumn;
            HasMoved = true;
            return captured;
        }

        /// <summary>
        /// Changes the recorded position of the piece without touching the board or the HasMoved
        /// flag. It is used by the move simulation of the game controller, which changes the board
        /// itself, tests the move and then restores both the board and the recorded position.
        /// </summary>
        /// <param name="row">New row (0 based).</param>
        /// <param name="column">New column (0 based).</param>
        public void SetPosition(int row, int column) {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Marks the piece as moved without changing its position. The special operation of the
        /// queen uses it on the two pieces that swap places.
        /// </summary>
        public void MarkAsMoved() {
            HasMoved = true;
        }

        /// <summary>
        /// Puts back a saved state of the piece: its position and its two movement flags. The game
        /// controller uses it to undo a special operation that would leave the own king in check.
        /// </summary>
        /// <param name="row">Row to put back (0 based).</param>
        /// <param name="column">Column to put back (0 based).</param>
        /// <param name="hasMoved">Value of HasMoved to put back.</param>
        /// <param name="specialUsed">Value of SpecialUsed to put back.</param>
        public void RestoreState(int row, int column, bool hasMoved, bool specialUsed) {
            Row = row;
            Column = column;
            HasMoved = hasMoved;
            SpecialUsed = specialUsed;
        }

        /// <summary>
        /// Moves the piece to a new square without checking the chess rules: it clears the origin,
        /// replaces whatever was on the target and marks the piece as moved. It is used by the
        /// special moves (castling, en passant, promotion) that <see cref="Move"/> does not accept.
        /// A captured piece is simply lost, so callers that need it must read the target first.
        /// </summary>
        /// <param name="endRow">Target row (0 based).</param>
        /// <param name="endColumn">Target column (0 based).</param>
        public void MoveTo(int endRow, int endColumn) {
            Board[Row][Column] = null;
            Board[endRow][endColumn] = this;
            Row = endRow;
            Column = endColumn;
            HasMoved = true;
        }

        /// <summary>
        /// Checks whether a row and column are inside the 8x8 board.
        /// </summary>
        public bool IsWithinBounds(int row, int column) {
            return row >= 0 && row < 8 && column >= 0 && column < 8;
        }

        /// <summary>
        /// Checks whether the square holds a piece of the same colour as this one.
        /// </summary>
        public bool IsFriendlyPiece(int row, int column) {
            Piece? piece = Board[row][column];
            return piece != null && piece.Color == Color;
        }

        /// <summary>
        /// Checks whether the square holds a piece of the opponent's colour.
        /// </summary>
        public bool IsOpponentPiece(int row, int column) {
            Piece? piece = Board[row][column];
            return piece != null && piece.Color != Color;
        }

        /// <summary>
        /// True when the special operation of this piece uses a final position in the instruction.
        /// The rook, the horse and the queen need one; the pawn computes its own target and the
        /// bishop ignores it. The king has no special operation at all.
        /// </summary>
        public virtual bool SpecialOperationNeedsEndPosition => false;

        /// <summary>
        /// Checks whether this piece may use its special operation towards a square, following
        /// only the rules of the piece itself. The board is left unchanged. The game controller
        /// still refuses an operation that would leave the own king in check.
        /// </summary>
        /// <param name="endRow">Final row (0 based), used by the pieces that need it.</param>
        /// <param name="endColumn">Final column (0 based), used by the pieces that need it.</param>
        /// <returns>True when the rules of the piece allow the operation.</returns>
        public virtual bool CanUseSpecialOperation(int endRow, int endColumn) {
            return false;
        }

        /// <summary>
        /// Applies the special operation, which must have been accepted by
        /// <see cref="CanUseSpecialOperation"/> first, and returns the success line. A piece whose
        /// operation can only be used once sets its <see cref="SpecialUsed"/> flag here. The base
        /// piece has no operation, so it never reaches this method with a true answer.
        /// </summary>
        /// <param name="endRow">Final row (0 based).</param>
        /// <param name="endColumn">Final column (0 based).</param>
        /// <returns>The success line of the operation.</returns>
        public virtual string UseSpecialOperation(int endRow, int endColumn) {
            return "Movimento inválido.";
        }
    }
}
