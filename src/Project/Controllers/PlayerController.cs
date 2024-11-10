using System;
using System.Collections.Generic;
using Chess.Models;

namespace Chess.Controllers
{
    /// <summary>
    /// Controlador para gerir jogadores.
    /// </summary>
    public class PlayerController
    {
        private readonly List<Player> players;

        /// <summary>
        /// Inicializa um novo controlador de jogadores.
        /// </summary>
        public PlayerController()
        {
            players = new List<Player>();
        }

        /// <summary>
        /// Regista um novo jogador.
        /// </summary>
        /// <param name="playerName">Nome do jogador.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        public string RegisterPlayer(string playerName)
        {
            if (players.Exists(p => p.Name == playerName))
            {
                return "Jogador existente.";
            }

            players.Add(new Player(playerName, ' ')); // Cor será atribuída ao iniciar o jogo.
            return "Jogador registado com sucesso.";
        }

        /// <summary>
        /// Lista todos os jogadores registados.
        /// </summary>
        /// <returns>Lista formatada de jogadores ou mensagem de erro.</returns>
        public string ListPlayers()
        {
            if (players.Count == 0)
            {
                return "Sem jogadores registados.";
            }

            players.Sort((a, b) =>
            {
                int winComparison = b.Wins.CompareTo(a.Wins);
                return winComparison != 0 ? winComparison : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            string result = "";
            foreach (var player in players)
            {
                result += $"{player.Name} {player.Wins + player.Draws + player.Losses} {player.Wins} {player.Draws} {player.Losses}\n";
            }

            return result.TrimEnd();
        }
    }
}
