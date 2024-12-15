using Chess.Controllers;

public class CLI {
    private PlayerController playerController;
    private GameController gameController;

    public CLI() {
        playerController = new PlayerController();
        gameController = new GameController();
    }

    public void Run() {
        while (true) {
            string line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) {
                break;
            }

            string[] tokens = line.Split(" ");
            string operation = tokens[0];

            switch (operation) {
                case "RJ":
                    if (tokens.Length == 2) {
                        string playerName = tokens[1];
                        Console.WriteLine(playerController.RegisterPlayer(playerName));
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                case "LJ":
                    Console.WriteLine(playerController.ListPlayers());
                    break;

                case "IJ":
                    if (tokens.Length == 4 && tokens[1] == "Novo") {
                        string whitePlayer = tokens[2];
                        string blackPlayer = tokens[3];
                        Console.WriteLine(gameController.StartNewGame(whitePlayer, blackPlayer));
                    } else if (tokens.Length > 4 && tokens[1] == "Continuação") {
                        string whitePlayer = tokens[2];
                        string blackPlayer = tokens[3];
                        string boardState = string.Join("\n", tokens[4..]);
                        Console.WriteLine(gameController.StartContinuedGame(whitePlayer, blackPlayer, boardState));
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                case "MP":
                    if (tokens.Length == 4) {
                        string playerName = tokens[1];
                        string startPosition = tokens[2];
                        string endPosition = tokens[3];

                        int startRow = 8 - int.Parse(startPosition[1].ToString());
                        int startCol = startPosition[0] - 'A';
                        int endRow = 8 - int.Parse(endPosition[1].ToString());
                        int endCol = endPosition[0] - 'A';

                        gameController.MovePiece(startRow, startCol, endRow, endCol);
                        Console.WriteLine("Movimento efetuado com sucesso.");
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                case "OS":
                    if (tokens.Length == 3 || tokens.Length == 4) {
                        string playerName = tokens[1];
                        string startPosition = tokens[2];
                        string endPosition = tokens.Length == 4 ? tokens[3] : null;

                        int startRow = 8 - int.Parse(startPosition[1].ToString());
                        int startCol = startPosition[0] - 'A';

                        if (endPosition != null) {
                            int endRow = 8 - int.Parse(endPosition[1].ToString());
                            int endCol = endPosition[0] - 'A';
                            Console.WriteLine(gameController.PerformSpecialMove(playerName, startRow, startCol, endRow, endCol));
                        } else {
                            Console.WriteLine(gameController.PerformSpecialMove(playerName, startRow, startCol));
                        }
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                case "DJ":
                    Console.WriteLine(gameController.GetBoardDetails());
                    break;

                case "D":
                    if (tokens.Length == 2 || tokens.Length == 3) {
                        string player1 = tokens[1];
                        string player2 = tokens.Length == 3 ? tokens[2] : null;
                        Console.WriteLine(gameController.Resign(player1, player2));
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                case "G":
                    if (tokens.Length == 2) {
                        string fileName = tokens[1];
                        Console.WriteLine(gameController.SaveGame(fileName));
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                case "L":
                    if (tokens.Length == 2) {
                        string fileName = tokens[1];
                        Console.WriteLine(gameController.LoadGame(fileName));
                    } else {
                        Console.WriteLine("Instrução inválida.");
                    }
                    break;

                default:
                    Console.WriteLine("Instrução inválida.");
                    break;
            }
        }
    }
}
