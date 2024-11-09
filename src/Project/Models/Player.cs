using System;
using System.Collections.Generic;

namespace Chess.Models {
    /// <summary>
    /// Representa um jogador no jogo de xadrez.
    /// </summary>
    public class Player {
        /// <summary>
        /// Nome do jogador.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Cor das peças do jogador ('W' para branco, 'B' para preto).
        /// </summary>
        public char Color { get; private set; }

        /// <summary>
        /// Lista de peças pertencentes ao jogador.
        /// </summary>
        public List<Piece> Pieces { get; private set; }

        /// <summary>
        /// Número de jogos vencidos pelo jogador.
        /// </summary>
        public int Wins { get; private set; }

        /// <summary>
        /// Número de jogos perdidos pelo jogador.
        /// </summary>
        public int Losses { get; private set; }

        /// <summary>
        /// Número de jogos empatados pelo jogador.
        /// </summary>
        public int Draws { get; private set; }

        /// <summary>
        /// Construtor para inicializar um novo jogador.
        /// </summary>
        /// <param name="name">Nome do jogador.</param>
        /// <param name="color">Cor das peças do jogador ('W' ou 'B').</param>
        public Player(string name, char color) {
            Name = name;
            Color = color;
            Pieces = new List<Piece>();
            Wins = 0;
            Losses = 0;
            Draws = 0;
        }

        /// <summary>
        /// Adiciona uma peça ao jogador.
        /// </summary>
        /// <param name="piece">A peça a ser adicionada.</param>
        public void AddPiece(Piece piece) {
            if (piece.Color != this.Color) {
                throw new InvalidOperationException("A peça não pertence a este jogador.");
            }
            Pieces.Add(piece);
        }

        /// <summary>
        /// Remove uma peça do jogador.
        /// </summary>
        /// <param name="piece">A peça a ser removida.</param>
        public void RemovePiece(Piece piece) {
            Pieces.Remove(piece);
        }

        /// <summary>
        /// Incrementa o número de vitórias do jogador.
        /// </summary>
        public void IncrementWins() {
            Wins++;
        }

        /// <summary>
        /// Incrementa o número de derrotas do jogador.
        /// </summary>
        public void IncrementLosses() {
            Losses++;
        }

        /// <summary>
        /// Incrementa o número de empates do jogador.
        /// </summary>
        public void IncrementDraws() {
            Draws++;
        }
    }
}
