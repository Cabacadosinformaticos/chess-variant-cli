using System;
using System.Collections.Generic;

namespace Chess.Models.Pieces {
    /// <summary>
    /// Representa a peça Bispo no jogo de xadrez.
    /// </summary>
    public class Bishop : Piece {
        private bool hasUsedSpecial; // Indica se o Bispo já realizou a sua operação especial.

        /// <summary>
        /// Construtor do Bispo.
        /// </summary>
        /// <param name="board">Tabuleiro onde o Bispo está localizado.</param>
        /// <param name="color">Cor do Bispo (Branco ou Preto).</param>
        /// <param name="symbol">Símbolo que representa o Bispo.</param>
        /// <param name="row">Linha inicial do Bispo.</param>
        /// <param name="column">Coluna inicial do Bispo.</param>
        public Bishop(Piece[][] board, char color, string symbol, int row, int column) : base(board, color, symbol, row, column) {
            hasUsedSpecial = false; // Inicialmente, o Bispo não usou a operação especial.
        }

        /// <summary>
        /// Verifica se o movimento proposto é válido para o Bispo.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public override bool CanMove(int endRow, int endColumn) {
            // Verifica se o movimento está dentro dos limites do tabuleiro.
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // Verifica se o movimento é diagonal.
            int rowDelta = Math.Abs(endRow - Row);
            int colDelta = Math.Abs(endColumn - Column);
            if (rowDelta != colDelta) {
                return false;
            }

            // Determina a direção do movimento.
            int rowStep = (endRow > Row) ? 1 : -1;
            int colStep = (endColumn > Column) ? 1 : -1;

            // Verifica se o caminho até o destino está livre.
            int currentRow = Row + rowStep;
            int currentCol = Column + colStep;
            while (currentRow != endRow && currentCol != endColumn) {
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
        /// Gera todos os movimentos válidos para o Bispo a partir da posição atual.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public override List<(int, int)> GetValidMoves() {
            var validMoves = new List<(int, int)>();

            // Direções diagonais para o Bispo.
            int[] directions = { -1, 1 };
            foreach (int rowDir in directions) {
                foreach (int colDir in directions) {
                    int currentRow = Row + rowDir;
                    int currentCol = Column + colDir;

                    while (IsWithinBounds(currentRow, currentCol)) {
                        if (Board[currentRow][currentCol] == null) {
                            validMoves.Add((currentRow, currentCol)); // Casa vazia.
                        } else if (IsOpponentPiece(currentRow, currentCol)) {
                            validMoves.Add((currentRow, currentCol)); // Captura possível.
                            break; // Caminho bloqueado após capturar.
                        } else {
                            break; // Caminho bloqueado por peça amiga.
                        }

                        // Continua na direção.
                        currentRow += rowDir;
                        currentCol += colDir;
                    }
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Realiza a operação especial do Bispo, capturando todas as peças adversárias
        /// em uma área de 3x3 ao redor dele.
        /// </summary>
        public void PerformSpecialOperation() {
            if (hasUsedSpecial) {
                throw new InvalidOperationException("O Bispo já usou a sua operação especial.");
            }

            // Define os limites da área de 3x3.
            int startRow = Math.Max(Row - 1, 0);
            int endRow = Math.Min(Row + 1, 7);
            int startCol = Math.Max(Column - 1, 0);
            int endCol = Math.Min(Column + 1, 7);

            // Captura todas as peças adversárias na área de 3x3.
            for (int r = startRow; r <= endRow; r++) {
                for (int c = startCol; c <= endCol; c++) {
                    if (IsOpponentPiece(r, c)) {
                        Board[r][c] = null; // Remove a peça adversária.
                    }
                }
            }

            hasUsedSpecial = true; // Marca que a operação especial foi usada.
        }
    }
}
