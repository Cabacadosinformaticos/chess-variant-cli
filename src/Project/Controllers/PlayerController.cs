using System.Collections.Generic;
using Chess.Models;

namespace Chess.Controllers {
    /// <summary>
    /// Keeps the registry of the players. The game controller uses the same Player objects held
    /// here, so the results of a game update the registry directly.
    /// </summary>
    public class PlayerController {
        private readonly List<Player> players;

        /// <summary>
        /// Creates an empty registry.
        /// </summary>
        public PlayerController() {
            players = new List<Player>();
        }

        /// <summary>
        /// Registers a new player, unless a player with the same name already exists.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The success or error message.</returns>
        public string RegisterPlayer(string playerName) {
            if (FindPlayer(playerName) != null) {
                return "Jogador existente.";
            }

            players.Add(new Player(playerName));
            return "Jogador registado com sucesso.";
        }

        /// <summary>
        /// Searches the registry for a player with the given name.
        /// </summary>
        /// <param name="playerName">Name to search for.</param>
        /// <returns>The registered player, or null when there is none with that name.</returns>
        public Player? FindPlayer(string playerName) {
            foreach (Player player in players) {
                if (player.Name == playerName) {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Lists the registered players, sorted by number of wins (descending) and then by name
        /// (ordinal comparison). One line per player: name, games, wins, draws, losses.
        /// </summary>
        /// <returns>The lines of the list, or the error message when there are no players.</returns>
        public string ListPlayers() {
            if (players.Count == 0) {
                return "Sem jogadores registados.";
            }

            // Sort a copy so the registry order is not changed by listing.
            List<Player> sorted = new List<Player>(players);
            sorted.Sort(ComparePlayers);

            List<string> lines = new List<string>();
            foreach (Player player in sorted) {
                lines.Add($"{player.Name} {player.Games} {player.Wins} {player.Draws} {player.Losses}");
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Orders two players for the listing: more wins first, then name in ordinal order.
        /// </summary>
        private static int ComparePlayers(Player first, Player second) {
            int byWins = second.Wins.CompareTo(first.Wins);
            if (byWins != 0) {
                return byWins;
            }
            return string.CompareOrdinal(first.Name, second.Name);
        }
    }
}
