using System;
using System.Collections.Generic;

namespace Chess.Models {
    /// <summary>
    /// Classe base para todas as peças no jogo de xadrez.
    /// </summary>
    public class Piece {
        /// <summary>
        /// Representação simbólica da peça (e.g., "BP1" para um peão preto).
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Linha atual da peça no tabuleiro (baseada em índices 0-7).
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Coluna atual da peça no tabuleiro (baseada em índices 0-7).
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Cor da peça ('W' para branco, 'B' para preto).
        /// </summary>
        public char Color { get; }

        /// <summary>
        /// Referência ao tabuleiro onde a peça está localizada.
        /// </summary>
        public Piece[][] Board { get; }

        /// <summary>
        /// Indica se a peça já foi movida no jogo.
        /// </summary>
        public bool HasMoved { get; private set; }

        /// <summary>
        /// Construtor que inicializa uma nova peça.
        /// </summary>
        /// <param name="board">Referência ao tabuleiro de jogo.</param>
        /// <param name="color">Cor da peça ('W' ou 'B').</param>
        /// <param name="symbol">Símbolo representando a peça.</param>
        /// <param name="row">Linha inicial da peça.</param>
        /// <param name="column">Coluna inicial da peça.</param>
        public Piece(Piece[][] board, char color, string symbol, int row, int column) {
            Symbol = symbol;
            Row = row;
            Column = column;
            Board = board;
            Color = color;
            HasMoved = false; // Inicialmente, a peça não se moveu.
        }

        /// <summary>
        /// Valida se a peça pode se mover para uma posição específica.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>True se o movimento for válido; caso contrário, False.</returns>
        public virtual bool CanMove(int endRow, int endColumn) {
            return false; // Placeholder para validação nas subclasses.
        }

        /// <summary>
        /// Gera todos os movimentos válidos para a peça. Deve ser sobrescrito pelas subclasses.
        /// </summary>
        /// <returns>Lista de coordenadas representando os movimentos válidos.</returns>
        public virtual List<(int, int)> GetValidMoves() {
            return new List<(int, int)>();
        }

        /// <summary>
        /// Move a peça para uma nova posição no tabuleiro.
        /// </summary>
        /// <param name="endRow">Linha de destino.</param>
        /// <param name="endColumn">Coluna de destino.</param>
        /// <returns>A peça capturada na posição de destino (ou null se não houver).</returns>
        public Piece Move(int endRow, int endColumn) {
            if (!CanMove(endRow, endColumn)) {
                throw new InvalidOperationException("Movimento inválido para esta peça.");
            }

            // Remove a peça da posição atual.
            Board[Row][Column] = null;

            // Captura a peça na posição de destino, se existir.
            Piece target = Board[endRow][endColumn];

            // Move a peça para a nova posição.
            Board[endRow][endColumn] = this;

            // Atualiza a posição da peça.
            Row = endRow;
            Column = endColumn;
            HasMoved = true; // Marca que a peça foi movida.

            return target; // Retorna a peça capturada ou null.
        }

        /// <summary>
        /// Verifica se uma posição está dentro dos limites do tabuleiro.
        /// </summary>
        /// <param name="row">Linha da posição.</param>
        /// <param name="column">Coluna da posição.</param>
        /// <returns>True se estiver dentro dos limites; caso contrário, False.</returns>
        public bool IsWithinBounds(int row, int column) {
            return row >= 0 && row < 8 && column >= 0 && column < 8;
        }

        /// <summary>
        /// Verifica se a peça na posição especificada pertence ao mesmo jogador.
        /// </summary>
        /// <param name="row">Linha da posição.</param>
        /// <param name="column">Coluna da posição.</param>
        /// <returns>True se a peça for amiga; caso contrário, False.</returns>
        public bool IsFriendlyPiece(int row, int column) {
            return Board[row][column] != null && Board[row][column].Color == this.Color;
        }

        /// <summary>
        /// Verifica se a peça na posição especificada pertence ao adversário.
        /// </summary>
        /// <param name="row">Linha da posição.</param>
        /// <param name="column">Coluna da posição.</param>
        /// <returns>True se a peça for adversária; caso contrário, False.</returns>
        public bool IsOpponentPiece(int row, int column) {
            return Board[row][column] != null && Board[row][column].Color != this.Color;
        }
    }
}
