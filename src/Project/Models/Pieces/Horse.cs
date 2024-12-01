using System;
using System.Collections.Generic;

namespace Chess.Models.Pieces {
    /// <summary>
    /// Representa a peça Cavalo no jogo de xadrez.
    /// </summary>
    public class Horse : Piece {
        /// <summary>
        /// Construtor do Cavalo.
        /// </summary>
        /// <param name="board">Tabuleiro onde o Cavalo está localizado.</param>
        /// <param name="color">Cor do Cavalo (Branco ou Preto).</param>
        /// <param name="symbol">Símbolo que representa o Cavalo.</param>
        /// <param name="row">Linha inicial do Cavalo.</param>
        /// <param name="column">Coluna inicial do Cavalo.</param>
        public Horse(Piece[][] board, char color, string symbol, int row, int column) : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Verifica se o movimento proposto é válido para o Cavalo.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public override bool CanMove(int endRow, int endColumn) {
            // Verifica se o movimento está dentro dos limites do tabuleiro.
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // Diferença absoluta entre as linhas e colunas.
            int rowDelta = Math.Abs(endRow - Row);
            int colDelta = Math.Abs(endColumn - Column);

            // Verifica se o movimento é em forma de "L".
            return (rowDelta == 2 && colDelta == 1) || (rowDelta == 1 && colDelta == 2);
        }

        /// <summary>
        /// Gera todos os movimentos válidos para o Cavalo a partir da posição atual.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public override List<(int, int)> GetValidMoves() {
            var validMoves = new List<(int, int)>();

            // Todas as combinações possíveis de movimentos em "L".
            int[][] moves = {
                new[] { 2, 1 }, new[] { 2, -1 },
                new[] { -2, 1 }, new[] { -2, -1 },
                new[] { 1, 2 }, new[] { 1, -2 },
                new[] { -1, 2 }, new[] { -1, -2 }
            };

            foreach (var move in moves) {
                int newRow = Row + move[0];
                int newCol = Column + move[1];
                if (IsWithinBounds(newRow, newCol) && (Board[newRow][newCol] == null || IsOpponentPiece(newRow, newCol))) {
                    validMoves.Add((newRow, newCol));
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Realiza a operação especial do Cavalo, permitindo que ele avance quatro casas
        /// em linha reta caso não esteja adjacente a outras peças.
        /// </summary>
        public void PerformSpecialOperation() {
            // Verifica se o Cavalo está adjacente a outra peça.
            for (int r = Row - 1; r <= Row + 1; r++) {
                for (int c = Column - 1; c <= Column + 1; c++) {
                    if (IsWithinBounds(r, c) && Board[r][c] != null && !(r == Row && c == Column)) {
                        throw new InvalidOperationException("O Cavalo está adjacente a outra peça e não pode usar a operação especial.");
                    }
                }
            }

            // Tenta avançar até quatro casas em linha reta.
            foreach (var dir in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) {
                int currentRow = Row + dir.Item1;
                int currentCol = Column + dir.Item2;
                for (int i = 1; i <= 4; i++) {
                    if (!IsWithinBounds(currentRow, currentCol)) {
                        break; // Fora dos limites.
                    }
                    if (Board[currentRow][currentCol] != null) {
                        if (IsOpponentPiece(currentRow, currentCol)) {
                            Board[currentRow][currentCol] = null; // Captura a peça adversária.
                        } else {
                            break; // Bloqueado por peça amiga.
                        }
                    }

                    // Move o Cavalo para a nova posição.
                    Row = currentRow;
                    Column = currentCol;
                    return;
                }
            }

            throw new InvalidOperationException("Não foi possível realizar a operação especial.");
        }
    }
}
