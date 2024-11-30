using System;
using System.Collections.Generic;

namespace Chess.Models.Pieces {
    /// <summary>
    /// Representa a peça Torre no jogo de xadrez.
    /// </summary>
    public class Rook : Piece {
        private bool hasUsedSpecial; // Indica se a Torre já realizou a sua operação especial.

        /// <summary>
        /// Construtor da Torre.
        /// </summary>
        /// <param name="board">Tabuleiro onde a Torre está localizada.</param>
        /// <param name="color">Cor da Torre (Branco ou Preto).</param>
        /// <param name="symbol">Símbolo que representa a Torre.</param>
        /// <param name="row">Linha inicial da Torre.</param>
        /// <param name="column">Coluna inicial da Torre.</param>
        public Rook(Piece[][] board, char color, string symbol, int row, int column) : base(board, color, symbol, row, column) {
            hasUsedSpecial = false;
        }

        /// <summary>
        /// Verifica se o movimento proposto é válido para a Torre.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public override bool CanMove(int endRow, int endColumn) {
            // Verifica se o movimento está dentro dos limites do tabuleiro.
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // Verifica se o movimento é horizontal ou vertical.
            if (Row != endRow && Column != endColumn) {
                return false;
            }

            // Determina a direção do movimento.
            int rowStep = (endRow > Row) ? 1 : (endRow < Row ? -1 : 0);
            int colStep = (endColumn > Column) ? 1 : (endColumn < Column ? -1 : 0);

            // Verifica se o caminho até o destino está livre.
            int currentRow = Row + rowStep;
            int currentCol = Column + colStep;
            while (currentRow != endRow || currentCol != endColumn) {
                if (Board[currentRow][currentCol] != null) {
                    return false; // Caminho bloqueado.
                }
                currentRow += rowStep;
                currentCol += colStep;
            }

            // Verifica se a casa de destino está vazia ou contém uma peça adversária.
            return Board[endRow][endColumn] == null || IsOpponentPiece(endRow, endColumn);
        }

        /// <summary>
        /// Gera todos os movimentos válidos para a Torre a partir da posição atual.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public override List<(int, int)> GetValidMoves() {
            var validMoves = new List<(int, int)>();

            // Direções possíveis (vertical e horizontal).
            int[] directions = { -1, 1 };

            // Movimentos verticais
            foreach (int dir in directions) {
                int currentRow = Row + dir;
                while (IsWithinBounds(currentRow, Column)) {
                    if (Board[currentRow][Column] == null) {
                        validMoves.Add((currentRow, Column));
                    } else if (IsOpponentPiece(currentRow, Column)) {
                        validMoves.Add((currentRow, Column)); // Captura possível.
                        break;
                    } else {
                        break; // Caminho bloqueado por peça amiga.
                    }
                    currentRow += dir;
                }
            }

            // Movimentos horizontais
            foreach (int dir in directions) {
                int currentCol = Column + dir;
                while (IsWithinBounds(Row, currentCol)) {
                    if (Board[Row][currentCol] == null) {
                        validMoves.Add((Row, currentCol));
                    } else if (IsOpponentPiece(Row, currentCol)) {
                        validMoves.Add((Row, currentCol)); // Captura possível.
                        break;
                    } else {
                        break; // Caminho bloqueado por peça amiga.
                    }
                    currentCol += dir;
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Realiza a operação especial da Torre, capturando duas peças adversárias
        /// em uma linha reta.
        /// </summary>
        public void PerformSpecialOperation() {
            if (hasUsedSpecial) {
                throw new InvalidOperationException("A Torre já usou a sua operação especial.");
            }

            // Tenta capturar duas peças adversárias em uma linha reta (horizontal ou vertical).
            foreach (var dir in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) { // Direções possíveis.
                int capturedCount = 0;
                int currentRow = Row + dir.Item1;
                int currentCol = Column + dir.Item2;

                while (IsWithinBounds(currentRow, currentCol)) {
                    if (IsOpponentPiece(currentRow, currentCol)) {
                        Board[currentRow][currentCol] = null; // Captura a peça adversária.
                        capturedCount++;
                        if (capturedCount == 2) {
                            hasUsedSpecial = true; // Marca que a operação especial foi usada.
                            return;
                        }
                    } else if (Board[currentRow][currentCol] != null) {
                        break; // Caminho bloqueado por peça amiga.
                    }
                    currentRow += dir.Item1;
                    currentCol += dir.Item2;
                }
            }

            throw new InvalidOperationException("Não foi possível capturar duas peças.");
        }
    }
}