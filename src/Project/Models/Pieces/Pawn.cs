using System;
using System.Collections.Generic;

namespace Chess.Models.Pieces {
    /// <summary>
    /// Representa a peça Peão no jogo de xadrez.
    /// </summary>
    public class Pawn : Piece {
        private bool hasMoved; // Indica se o Peão já se moveu no jogo.
        private bool hasUsedSpecial; // Indica se o Peão já realizou sua operação especial.

        /// <summary>
        /// Construtor do Peão.
        /// </summary>
        /// <param name="board">Tabuleiro onde o Peão está localizado.</param>
        /// <param name="color">Cor do Peão (Branco ou Preto).</param>
        /// <param name="symbol">Símbolo que representa o Peão.</param>
        /// <param name="row">Linha inicial do Peão.</param>
        /// <param name="column">Coluna inicial do Peão.</param>
        public Pawn(Piece[][] board, char color, string symbol, int row, int column) : base(board, color, symbol, row, column) {
            hasMoved = false;
            hasUsedSpecial = false;
        }

        /// <summary>
        /// Verifica se o movimento proposto é válido para o Peão.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public override bool CanMove(int endRow, int endColumn) {
            // Verifica se o movimento está dentro dos limites do tabuleiro.
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            int rowDelta = endRow - Row;
            int colDelta = Math.Abs(endColumn - Column);

            if (Color == 'W') {
                // Movimento normal para frente
                if (rowDelta == -1 && colDelta == 0 && Board[endRow][endColumn] == null) {
                    return true;
                }
                // Movimento inicial de duas casas
                if (!hasMoved && rowDelta == -2 && colDelta == 0 && Board[endRow][endColumn] == null && Board[Row - 1][Column] == null) {
                    return true;
                }
                // Captura diagonal
                if (rowDelta == -1 && colDelta == 1 && IsOpponentPiece(endRow, endColumn)) {
                    return true;
                }
            } else if (Color == 'B') {
                // Movimento normal para frente
                if (rowDelta == 1 && colDelta == 0 && Board[endRow][endColumn] == null) {
                    return true;
                }
                // Movimento inicial de duas casas
                if (!hasMoved && rowDelta == 2 && colDelta == 0 && Board[endRow][endColumn] == null && Board[Row + 1][Column] == null) {
                    return true;
                }
                // Captura diagonal
                if (rowDelta == 1 && colDelta == 1 && IsOpponentPiece(endRow, endColumn)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Realiza a operação especial do Peão, permitindo que ele recue uma casa.
        /// </summary>
        public void PerformSpecialOperation() {
            if (hasUsedSpecial) {
                throw new InvalidOperationException("O Peão já usou a sua operação especial.");
            }

            if (Color == 'W') {
                if (Row + 1 < 8 && Board[Row + 1][Column] == null) {
                    Row += 1; // Recua uma casa.
                } else {
                    throw new InvalidOperationException("Não é possível recuar.");
                }
            } else if (Color == 'B') {
                if (Row - 1 >= 0 && Board[Row - 1][Column] == null) {
                    Row -= 1; // Recua uma casa.
                } else {
                    throw new InvalidOperationException("Não é possível recuar.");
                }
            }

            hasUsedSpecial = true; // Marca que a operação especial foi usada.
        }

        /// <summary>
        /// Gera todos os movimentos válidos para o Peão a partir da posição atual.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public override List<(int, int)> GetValidMoves() {
            var validMoves = new List<(int, int)>();

            int direction = (Color == 'W') ? -1 : 1; // Define a direção do movimento baseado na cor.

            // Movimento normal
            if (IsWithinBounds(Row + direction, Column) && Board[Row + direction][Column] == null) {
                validMoves.Add((Row + direction, Column));
            }

            // Movimento inicial de duas casas
            if (!hasMoved && IsWithinBounds(Row + 2 * direction, Column) && Board[Row + direction][Column] == null && Board[Row + 2 * direction][Column] == null) {
                validMoves.Add((Row + 2 * direction, Column));
            }

            // Captura diagonal esquerda
            if (IsWithinBounds(Row + direction, Column - 1) && IsOpponentPiece(Row + direction, Column - 1)) {
                validMoves.Add((Row + direction, Column - 1));
            }

            // Captura diagonal direita
            if (IsWithinBounds(Row + direction, Column + 1) && IsOpponentPiece(Row + direction, Column + 1)) {
                validMoves.Add((Row + direction, Column + 1));
            }

            return validMoves;
        }
    }
}
