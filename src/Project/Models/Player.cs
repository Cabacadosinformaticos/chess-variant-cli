namespace Chess.Models {
    /// <summary>
    /// A registered player: a name and the counters of the games he played.
    /// The same object is used by the registry and by the running game, so that results
    /// update the registry directly.
    /// </summary>
    public class Player {
        /// <summary>
        /// Name of the player (no spaces).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Number of games won.
        /// </summary>
        public int Wins { get; private set; }

        /// <summary>
        /// Number of games drawn.
        /// </summary>
        public int Draws { get; private set; }

        /// <summary>
        /// Number of games lost.
        /// </summary>
        public int Losses { get; private set; }

        /// <summary>
        /// Number of games played: wins + draws + losses.
        /// </summary>
        public int Games => Wins + Draws + Losses;

        /// <summary>
        /// Creates a player with an empty record.
        /// </summary>
        /// <param name="name">Name of the player.</param>
        public Player(string name) {
            Name = name;
            Wins = 0;
            Draws = 0;
            Losses = 0;
        }

        /// <summary>
        /// Registers a win for this player.
        /// </summary>
        public void IncrementWins() {
            Wins++;
        }

        /// <summary>
        /// Registers a draw for this player.
        /// </summary>
        public void IncrementDraws() {
            Draws++;
        }

        /// <summary>
        /// Registers a loss for this player.
        /// </summary>
        public void IncrementLosses() {
            Losses++;
        }
    }
}
