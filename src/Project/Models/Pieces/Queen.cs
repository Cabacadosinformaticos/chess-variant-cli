using System;
using System.Collections.Generic;

namespace Chess.Models.Pieces {
    /// <summary>
    /// Representa a peça Rainha no jogo de xadrez.
    /// </summary>
    public class Queen : Piece {
        /// <summary>
        /// Construtor da Rainha.
        /// </summary>
        /// <param name="board">Tabuleiro onde a Rainha está localizada.</param>
        /// <param name="color">Cor da Rainha (Branco ou Preto).</param>
        /// <param name="symbol">Símbolo que representa a Rainha.</param>
        /// <param name="row">Linha inicial da Rainha.</param>
        /// <param name="column">Coluna inicial da Rainha.</param>
        public Queen(Piece[][] board, char color, string symbol, int row, int column) : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Verifica se o movimento proposto é válido para a Rainha.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public override bool CanMove(int endRow, int endColumn) {
            // Verifica se o movimento está dentro dos limites do tabuleiro.
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            int rowDelta = Math.Abs(endRow - Row);
            int colDelta = Math.Abs(endColumn - Column);

            // A Rainha combina movimentos de Torre e Bispo.
            bool isDiagonal = rowDelta == colDelta;
            bool isStraight = Row == endRow || Column == endColumn;

            if (!isDiagonal && !isStraight) {
                return false;
            }

            // Determina o passo das linhas e colunas.
            int rowStep = endRow > Row ? 1 : (endRow < Row ? -1 : 0);
            int colStep = endColumn > Column ? 1 : (endColumn < Column ? -1 : 0);

            // Verifica se o caminho está livre.
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
        /// Gera todos os movimentos válidos para a Rainha a partir da posição atual.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public override List<(int, int)> GetValidMoves() {
            var validMoves = new List<(int, int)>();

            // Movimentos em linha reta (Torre).
            int[] directions = { -1, 1 };
            foreach (int dir in directions) {
                // Movimentos verticais.
                int currentRow = Row + dir;
                while (IsWithinBounds(currentRow, Column)) {
                    if (Board[currentRow][Column] == null) {
                        validMoves.Add((currentRow, Column));
                    } else if (IsOpponentPiece(currentRow, Column)) {
                        validMoves.Add((currentRow, Column));
                        break;
                    } else {
                        break;
                    }
                    currentRow += dir;
                }

                // Movimentos horizontais.
                int currentCol = Column + dir;
                while (IsWithinBounds(Row, currentCol)) {
                    if (Board[Row][currentCol] == null) {
                        validMoves.Add((Row, currentCol));
                    } else if (IsOpponentPiece(Row, currentCol)) {
                        validMoves.Add((Row, currentCol));
                        break;
                    } else {
                        break;
                    }
                    currentCol += dir;
                }
            }

            // Movimentos diagonais (Bispo).
            foreach (int rowDir in directions) {
                foreach (int colDir in directions) {
                    int currentRow = Row + rowDir;
                    int currentCol = Column + colDir;

                    while (IsWithinBounds(currentRow, currentCol)) {
                        if (Board[currentRow][currentCol] == null) {
                            validMoves.Add((currentRow, currentCol));
                        } else if (IsOpponentPiece(currentRow, currentCol)) {
                            validMoves.Add((currentRow, currentCol));
                            break;
                        } else {
                            break;
                        }
                        currentRow += rowDir;
                        currentCol += colDir;
                    }
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Realiza a operação especial da Rainha, trocando de posição com o Rei.
        /// </summary>
        public void PerformSpecialOperation(King king) {
            // Verifica se o Rei está no mesmo tabuleiro e não está em xeque.
            if (king == null || king.Board != this.Board) {
                throw new InvalidOperationException("O Rei não está no mesmo tabuleiro.");
            }

            if (king.IsInCheck()) {
                throw new InvalidOperationException("O Rei está em xeque e não pode trocar de posição.");
            }

            // Troca de posição entre a Rainha e o Rei.
            int queenRow = this.Row;
            int queenCol = this.Column;
            this.Row = king.Row;
            this.Column = king.Column;
            king.Row = queenRow;
            king.Column = queenCol;

            // Atualiza o tabuleiro.
            Board[this.Row][this.Column] = this;
            Board[king.Row][king.Column] = king;
        }
    }
}
