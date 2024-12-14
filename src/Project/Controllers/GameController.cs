using System;
using System.IO;
using Chess.Models;
using Chess.Models.Pieces;
using System.Collections.Generic;

namespace Chess.Controllers {
    /// <summary>
    /// Controlador principal do jogo de xadrez, gerencia as regras, turnos e interações entre jogadores.
    /// </summary>
    public class GameController {
        public Player WhitePlayer { get; private set; }
        public Player BlackPlayer { get; private set; }
        public Board Board { get; private set; }
        public char CurrentTurn { get; private set; }
        private bool gameInProgress;

        public GameController() {
            gameInProgress = false;
        }

        /// <summary>
        /// Inicia um novo jogo entre dois jogadores.
        /// </summary>
        public string StartNewGame(string whitePlayerName, string blackPlayerName) {
            if (gameInProgress) {
                return "Existe um jogo em curso.";
            }

            WhitePlayer = new Player(whitePlayerName, 'W');
            BlackPlayer = new Player(blackPlayerName, 'B');
            Board = new Board();
            Board.InitializeStandardSetup();
            CurrentTurn = 'W';
            gameInProgress = true;
            return "Jogo iniciado com sucesso.";
        }

        /// <summary>
        /// Continua um jogo a partir de um estado pré-definido.
        /// </summary>
        public string StartContinuedGame(string whitePlayerName, string blackPlayerName, string boardState) {
            if (gameInProgress) {
                return "Existe um jogo em curso.";
            }

            WhitePlayer = new Player(whitePlayerName, 'W');
            BlackPlayer = new Player(blackPlayerName, 'B');
            Board = new Board();

            // Configurar o tabuleiro com o estado fornecido.
            string[] rows = boardState.Split('\n');
            for (int i = 0; i < rows.Length; i++) {
                string[] pieces = rows[i].Split(',');
                for (int j = 0; j < pieces.Length; j++) {
                    switch (pieces[j]) {
                        case string symbol when symbol.StartsWith("P"):
                            Board.SetPiece(i, j, new Pawn(Board.Grid, symbol[0], symbol, i, j));
                            break;
                        case string symbol when symbol.StartsWith("R"):
                            Board.SetPiece(i, j, new Rook(Board.Grid, symbol[0], symbol, i, j));
                            break;
                        case string symbol when symbol.StartsWith("H"):
                            Board.SetPiece(i, j, new Horse(Board.Grid, symbol[0], symbol, i, j));
                            break;
                        case string symbol when symbol.StartsWith("B"):
                            Board.SetPiece(i, j, new Bishop(Board.Grid, symbol[0], symbol, i, j));
                            break;
                        case string symbol when symbol.StartsWith("Q"):
                            Board.SetPiece(i, j, new Queen(Board.Grid, symbol[0], symbol, i, j));
                            break;
                        case string symbol when symbol.StartsWith("K"):
                            Board.SetPiece(i, j, new King(Board.Grid, symbol[0], symbol, i, j));
                            break;
                        default:
                            Board.SetPiece(i, j, null);
                            break;
                    }
                }
            }

            CurrentTurn = 'W';
            gameInProgress = true;
            return "Jogo iniciado com sucesso.";
        }

        /// <summary>
        /// Mostra o estado atual do tabuleiro e o jogador atual.
        /// </summary>
        public string GetBoardDetails() {
            if (!gameInProgress) {
                return "Não existe jogo em curso.";
            }

            Console.WriteLine("Estado do tabuleiro:");
            Board.Display();
            return $"Vez do jogador: {(CurrentTurn == 'W' ? WhitePlayer.Name : BlackPlayer.Name)}";
        }

        /// <summary>
        /// Registra a desistência de um ou ambos os jogadores.
        /// </summary>
        public string Resign(string playerName1, string? playerName2 = null) {
            if (!gameInProgress) {
                return "Não existe jogo em curso.";
            }

            if (playerName2 == null) {
                Player winner = WhitePlayer.Name == playerName1 ? BlackPlayer : WhitePlayer;
                winner.IncrementWins();
                gameInProgress = false;
                return $"{playerName1} desistiu. {winner.Name} venceu.";
            } else {
                WhitePlayer.IncrementDraws();
                BlackPlayer.IncrementDraws();
                gameInProgress = false;
                return "Ambos os jogadores desistiram. Jogo terminou em empate.";
            }
        }

        /// <summary>
        /// Grava o estado atual do jogo em um arquivo.
        /// </summary>
        public string SaveGame(string fileName) {
            try {
                using (StreamWriter writer = new StreamWriter(fileName)) {
                    writer.WriteLine(WhitePlayer.Name);
                    writer.WriteLine(BlackPlayer.Name);
                    writer.WriteLine(CurrentTurn);
                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 8; j++) {
                            var piece = Board.GetPiece(i, j);
                            writer.Write(piece != null ? piece.Symbol : "");
                            writer.Write(j < 7 ? "," : "\n");
                        }
                    }
                }
                return "Jogo gravado com sucesso.";
            } catch (Exception ex) {
                return $"Erro ao gravar o jogo: {ex.Message}";
            }
        }

        /// <summary>
        /// Carrega o estado de um jogo de um arquivo.
        /// </summary>
        public string LoadGame(string fileName) {
            if (gameInProgress) {
                return "Existe um jogo em curso.";
            }

            try {
                using (StreamReader reader = new StreamReader(fileName)) {
                    string whitePlayerName = reader.ReadLine();
                    string blackPlayerName = reader.ReadLine();
                    CurrentTurn = reader.ReadLine()[0];

                    WhitePlayer = new Player(whitePlayerName, 'W');
                    BlackPlayer = new Player(blackPlayerName, 'B');
                    Board = new Board();

                    string[] rows = new string[8];
                    for (int i = 0; i < 8; i++) {
                        rows[i] = reader.ReadLine();
                    }

                    // Configurar o tabuleiro com o estado lido.
                    for (int i = 0; i < rows.Length; i++) {
                        string[] pieces = rows[i].Split(',');
                        for (int j = 0; j < pieces.Length; j++) {
                            switch (pieces[j]) {
                                case string symbol when symbol.StartsWith("P"):
                                    Board.SetPiece(i, j, new Pawn(Board.Grid, symbol[0], symbol, i, j));
                                    break;
                                case string symbol when symbol.StartsWith("R"):
                                    Board.SetPiece(i, j, new Rook(Board.Grid, symbol[0], symbol, i, j));
                                    break;
                                case string symbol when symbol.StartsWith("H"):
                                    Board.SetPiece(i, j, new Horse(Board.Grid, symbol[0], symbol, i, j));
                                    break;
                                case string symbol when symbol.StartsWith("B"):
                                    Board.SetPiece(i, j, new Bishop(Board.Grid, symbol[0], symbol, i, j));
                                    break;
                                case string symbol when symbol.StartsWith("Q"):
                                    Board.SetPiece(i, j, new Queen(Board.Grid, symbol[0], symbol, i, j));
                                    break;
                                case string symbol when symbol.StartsWith("K"):
                                    Board.SetPiece(i, j, new King(Board.Grid, symbol[0], symbol, i, j));
                                    break;
                                default:
                                    Board.SetPiece(i, j, null);
                                    break;
                            }
                        }
                    }
                }
                gameInProgress = true;
                return "Jogo lido com sucesso.";
            } catch (Exception ex) {
                return $"Erro ao carregar o jogo: {ex.Message}";
            }
        }

        /// <summary>
        /// Alterna o turno entre os jogadores.
        /// </summary>
        private void SwitchTurn() {
            CurrentTurn = (CurrentTurn == 'W') ? 'B' : 'W';
        }

        /// <summary>
        /// Move uma peça no tabuleiro.
        /// </summary>
        public void MovePiece(int startRow, int startCol, int endRow, int endCol) {
            Piece piece = Board.GetPiece(startRow, startCol);

            if (piece == null) {
                throw new InvalidOperationException("Não há nenhuma peça na posição de origem.");
            }

            if (piece.Color != CurrentTurn) {
                throw new InvalidOperationException("Não é o turno desse jogador.");
            }

            if (!piece.CanMove(endRow, endCol)) {
                throw new InvalidOperationException("Movimento inválido para esta peça.");
            }

            // Realiza o movimento.
            Piece capturedPiece = piece.Move(endRow, endCol);

            // Remove a peça capturada, se existir.
            if (capturedPiece != null) {
                Player opponent = (CurrentTurn == 'W') ? BlackPlayer : WhitePlayer;
                opponent.RemovePiece(capturedPiece);
            }

            // Alterna o turno após o movimento.
            SwitchTurn();
        }

        /// <summary>
        /// Verifica se o jogador atual está em xeque.
        /// </summary>
        public bool IsInCheck() {
            Player currentPlayer = (CurrentTurn == 'W') ? WhitePlayer : BlackPlayer;
            Player opponent = (CurrentTurn == 'W') ? BlackPlayer : WhitePlayer;

            Piece king = currentPlayer.Pieces.Find(piece => piece is King);
            if (king == null) {
                throw new InvalidOperationException("O Rei do jogador atual não foi encontrado.");
            }

            foreach (var piece in opponent.Pieces) {
                if (piece.CanMove(king.Row, king.Column)) {
                    return true; // O rei está em xeque.
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se o jogador atual está em xeque-mate.
        /// </summary>
        public bool IsInCheckmate() {
            if (!IsInCheck()) {
                return false; // Não está em xeque, logo não pode estar em xeque-mate.
            }

            Player currentPlayer = (CurrentTurn == 'W') ? WhitePlayer : BlackPlayer;

            // Verifica se o jogador atual tem algum movimento válido.
            foreach (var piece in currentPlayer.Pieces) {
                foreach (var move in piece.GetValidMoves()) {
                    int targetRow = move.Item1;
                    int targetCol = move.Item2;

                    // Simula o movimento.
                    Piece originalPiece = Board.GetPiece(targetRow, targetCol);
                    int originalRow = piece.Row;
                    int originalCol = piece.Column;

                    Board.SetPiece(targetRow, targetCol, piece);
                    Board.SetPiece(originalRow, originalCol, null);

                    if (!IsInCheck()) {
                        // Reverte o movimento.
                        Board.SetPiece(originalRow, originalCol, piece);
                        Board.SetPiece(targetRow, targetCol, originalPiece);
                        return false; // Existe pelo menos um movimento que evita o xeque-mate.
                    }

                    // Reverte o movimento.
                    Board.SetPiece(originalRow, originalCol, piece);
                    Board.SetPiece(targetRow, targetCol, originalPiece);
                }
            }

            return true; // Nenhum movimento válido disponível, xeque-mate.
        }

        /// <summary>
        /// Executa uma operação especial com uma peça.
        /// </summary>
        public string PerformSpecialMove(string playerName, int startRow, int startCol, int? endRow = null, int? endCol = null) {
            if (!gameInProgress) {
                return "Não existe jogo em curso.";
            }

            Player currentPlayer = (CurrentTurn == 'W') ? WhitePlayer : BlackPlayer;
            if (currentPlayer.Name != playerName) {
                return "Não é a vez deste jogador.";
            }

            Piece piece = Board.GetPiece(startRow, startCol);
            if (piece == null) {
                return "Não existe peça na posição inicial.";
            }

            if (piece.Color != CurrentTurn) {
                return "A peça selecionada não pertence ao jogador atual.";
            }

            // Verifica se a peça suporta operações especiais
            if (!(piece is SpecialMove specialPiece)) {
                return "A peça selecionada não suporta operações especiais.";
            }

            // Executa a operação especial
            if (endRow.HasValue && endCol.HasValue) {
                if (specialPiece.CanPerformSpecialMove(endRow.Value, endCol.Value)) {
                    specialPiece.PerformSpecialMove(endRow.Value, endCol.Value);
                    return "Operação especial realizada com sucesso.";
                } else {
                    return "Movimento especial inválido.";
                }
            } else {
                specialPiece.PerformSpecialMove();
                return "Operação especial realizada com sucesso.";
            }
        }
    }
}
