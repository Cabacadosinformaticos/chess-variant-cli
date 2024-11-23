using System;
using Chess.Models.Pieces;


namespace Chess.Models {
    /// <summary>
    /// Representa o tabuleiro de xadrez, responsável por inicializar e gerir o estado do jogo.
    /// </summary>
    public class Board {
        private const int Size = 8; // Tamanho fixo do tabuleiro (8x8).
        public Piece[][] Grid { get; private set; } // Matriz que armazena as peças no tabuleiro.

        /// <summary>
        /// Construtor que inicializa o tabuleiro vazio.
        /// </summary>
        public Board() {
            Grid = new Piece[Size][];
            for (int i = 0; i < Size; i++) {
                Grid[i] = new Piece[Size];
            }
        }

        /// <summary>
        /// Configura o tabuleiro com as peças na posição inicial padrão do xadrez.
        /// </summary>
        public void InitializeStandardSetup() {
            // Inicializa as peças do tabuleiro com os símbolos exatos do enunciado

            // Peças pretas
            Grid[0][0] = new Rook(Grid, 'B', "BR1", 0, 0);
            Grid[0][1] = new Horse(Grid, 'B', "BH1", 0, 1);
            Grid[0][2] = new Bishop(Grid, 'B', "BB1", 0, 2);
            Grid[0][3] = new Queen(Grid, 'B', "BQ1", 0, 3);
            Grid[0][4] = new King(Grid, 'B', "BK1", 0, 4);
            Grid[0][5] = new Bishop(Grid, 'B', "BB2", 0, 5);
            Grid[0][6] = new Horse(Grid, 'B', "BH2", 0, 6);
            Grid[0][7] = new Rook(Grid, 'B', "BR2", 0, 7);

            for (int col = 0; col < Size; col++) {
                Grid[1][col] = new Pawn(Grid, 'B', $"BP{col + 1}", 1, col);
            }

            // Peças brancas
            Grid[7][0] = new Rook(Grid, 'W', "WR1", 7, 0);
            Grid[7][1] = new Horse(Grid, 'W', "WH1", 7, 1);
            Grid[7][2] = new Bishop(Grid, 'W', "WB1", 7, 2);
            Grid[7][3] = new Queen(Grid, 'W', "WQ1", 7, 3);
            Grid[7][4] = new King(Grid, 'W', "WK1", 7, 4);
            Grid[7][5] = new Bishop(Grid, 'W', "WB2", 7, 5);
            Grid[7][6] = new Horse(Grid, 'W', "WH2", 7, 6);
            Grid[7][7] = new Rook(Grid, 'W', "WR2", 7, 7);

            for (int col = 0; col < Size; col++) {
                Grid[6][col] = new Pawn(Grid, 'W', $"WP{col + 1}", 6, col);
            }
        }

        /// <summary>
        /// Verifica se uma posição está dentro dos limites do tabuleiro.
        /// </summary>
        /// <param name="row">Linha da posição.</param>
        /// <param name="column">Coluna da posição.</param>
        /// <returns>True se a posição estiver dentro dos limites; caso contrário, False.</returns>
        public bool IsWithinBounds(int row, int column) {
            return row >= 0 && row < Size && column >= 0 && column < Size;
        }

        /// <summary>
        /// Retorna a peça localizada em uma posição específica.
        /// </summary>
        /// <param name="row">Linha da posição.</param>
        /// <param name="column">Coluna da posição.</param>
        /// <returns>A peça na posição ou null se não houver peça.</returns>
        public Piece GetPiece(int row, int column) {
            return IsWithinBounds(row, column) ? Grid[row][column] : null;
        }

        /// <summary>
        /// Coloca uma peça em uma posição específica do tabuleiro.
        /// </summary>
        /// <param name="row">Linha de destino.</param>
        /// <param name="column">Coluna de destino.</param>
        /// <param name="piece">Peça a ser colocada.</param>
        public void SetPiece(int row, int column, Piece piece) {
            if (IsWithinBounds(row, column)) {
                Grid[row][column] = piece;
            }
        }

        /// <summary>
        /// Remove uma peça de uma posição específica do tabuleiro.
        /// </summary>
        /// <param name="row">Linha da posição.</param>
        /// <param name="column">Coluna da posição.</param>
        public void RemovePiece(int row, int column) {
            if (IsWithinBounds(row, column)) {
                Grid[row][column] = null;
            }
        }

        /// <summary>
        /// Exibe o estado atual do tabuleiro na consola.
        /// </summary>
        public void Display() {
            Console.WriteLine("    A    B    C    D    E    F    G    H");
            for (int row = 0; row < Size; row++) {
                Console.Write(" " + (8 - row) + " "); // Exibe o número da linha invertido (1 no topo, 8 embaixo).
                for (int col = 0; col < Size; col++) {
                    Piece piece = Grid[row][col];
                    Console.Write(piece != null ? piece.Symbol.PadRight(5) : "     ");
                }
                Console.WriteLine();
            }
        }
    }
}
