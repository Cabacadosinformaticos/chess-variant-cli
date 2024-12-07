using System;
using System.Collections.Generic;

namespace Chess.Models.Pieces {
    /// <summary>
    /// Representa a peça Rei no jogo de xadrez.
    /// </summary>
    public class King : Piece {
        /// <summary>
        /// Construtor do Rei.
        /// </summary>
        /// <param name="board">Tabuleiro onde o Rei está localizado.</param>
        /// <param name="color">Cor do Rei (Branco ou Preto).</param>
        /// <param name="symbol">Símbolo que representa o Rei.</param>
        /// <param name="row">Linha inicial do Rei.</param>
        /// <param name="column">Coluna inicial do Rei.</param>
        public King(Piece[][] board, char color, string symbol, int row, int column) : base(board, color, symbol, row, column) {
        }

        /// <summary>
        /// Verifica se o movimento proposto é válido para o Rei.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public override bool CanMove(int endRow, int endColumn) {
            // Verifica se o movimento está dentro dos limites do tabuleiro.
            if (!IsWithinBounds(endRow, endColumn)) {
                return false;
            }

            // Diferença absoluta entre a linha e a coluna.
            int rowDelta = Math.Abs(endRow - Row);
            int colDelta = Math.Abs(endColumn - Column);

            // O Rei pode mover-se apenas uma casa em qualquer direção.
            return (rowDelta <= 1 && colDelta <= 1) && !(rowDelta == 0 && colDelta == 0);
        }

        /// <summary>
        /// Gera todos os movimentos válidos para o Rei a partir da posição atual.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public override List<(int, int)> GetValidMoves() {
            var validMoves = new List<(int, int)>();

            // Todas as combinações possíveis de movimentos do Rei.
            int[] directions = { -1, 0, 1 };
            foreach (int rowDir in directions) {
                foreach (int colDir in directions) {
                    if (rowDir == 0 && colDir == 0) {
                        continue; // Ignora a posição atual.
                    }

                    int newRow = Row + rowDir;
                    int newCol = Column + colDir;

                    if (IsWithinBounds(newRow, newCol) && (Board[newRow][newCol] == null || IsOpponentPiece(newRow, newCol))) {
                        validMoves.Add((newRow, newCol));
                    }
                }
            }

            return validMoves;
        }

        /// <summary>
        /// Verifica se o Rei está em xeque, ou seja, ameaçado por peças adversárias.
        /// </summary>
        /// <returns>True se o Rei estiver em xeque; caso contrário, False.</returns>
        public bool IsInCheck() {
            foreach (var row in Board) {
                foreach (var piece in row) {
                    if (piece != null && piece.Color != this.Color) {
                        if (piece.CanMove(this.Row, this.Column)) {
                            return true; // O Rei está sob ameaça.
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Verifica se o Rei pode realizar o roque com uma torre.
        /// </summary>
        /// <param name="rook">A torre com a qual o Rei tentará fazer o roque.</param>
        /// <returns>True se o roque for possível; caso contrário, False.</returns>
        public bool CanCastle(Rook rook) {
            // Verifica se a torre é válida e do mesmo lado.
            if (rook == null || rook.Color != this.Color || rook.Board != this.Board) {
                return false;
            }

            // O roque só é possível se o Rei e a Torre não se moveram.
            if (HasMoved || rook.HasMoved) {
                return false;
            }

            // Verifica se o caminho entre o Rei e a Torre está livre.
            int direction = rook.Column > this.Column ? 1 : -1;
            int currentCol = this.Column + direction;
            while (currentCol != rook.Column) {
                if (Board[this.Row][currentCol] != null) {
                    return false; // Caminho bloqueado.
                }
                currentCol += direction;
            }

            // Verifica se o Rei está em xeque ou passa por casas ameaçadas.
            for (int col = this.Column; col != rook.Column + direction; col += direction) {
                this.Column = col; // Simula a posição do Rei.
                if (IsInCheck()) {
                    return false; // O Rei não pode passar por casas em xeque.
                }
            }

            return true;
        }
    }
}
