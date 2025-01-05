# Chess Variant CLI

A two-player **chess variant** played through text commands in the terminal, written in C#
(.NET 8) with the standard library only.

Besides the normal rules of chess (check, checkmate, castling, en passant and promotion),
every piece except the king has a **special operation**, such as a rook that captures two
pieces at once or a bishop that clears the pawns around it.

This is my individual project for the **Fundamentos da Programação** (Fundamentals of
Programming) course, 1st semester of the 1st year of the Computer Engineering degree at
[IADE](https://www.iade.europeia.pt/), 2024/2025.

The program follows a strict input/output contract: every instruction produces a fixed
message, so a whole game can be scripted in a text file and its output compared byte by byte
with an expected file. The messages are in Portuguese, as the assignment requires.

## Contents

- [The board](#the-board)
- [Special operations](#special-operations)
- [Getting started](#getting-started)
- [Instructions](#instructions)
- [Example](#example)
- [Tests](#tests)
- [Project structure](#project-structure)
- [Design notes](#design-notes)
- [Documents](#documents)

## The board

Columns are `A` to `H` and rows `1` to `8`. Row 1 is at the top and holds the black pieces;
White starts and moves up the board, towards row 1.

```text
    A    B    C    D    E    F    G    H
1  BR1  BH1  BB1  BQ1  BK1  BB2  BH2  BR2
2  BP1  BP2  BP3  BP4  BP5  BP6  BP7  BP8
3  
4  
5  
6  
7  WP1  WP2  WP3  WP4  WP5  WP6  WP7  WP8
8  WR1  WH1  WB1  WQ1  WK1  WB2  WH2  WR2
```

Each piece is named by its colour (`W` or `B`), its type (`P` pawn, `R` rook, `H` horse,
`B` bishop, `Q` queen, `K` king) and an index.

## Special operations

| Piece | Operation | How often |
|---|---|---|
| Pawn | Moves one square backwards | Once per pawn |
| Rook | Captures two opposing pieces in a straight line, the first one next to the rook | Once per rook |
| Horse | When no piece touches it, jumps exactly four squares in a straight line, over anything | Any time |
| Bishop | Captures every opposing pawn in the 3 x 3 square around it, without moving | Once per bishop |
| Queen | Swaps places with its king, if the king is safe before and after | Any time |

No move or operation may leave the player's own king in check.

## Getting started

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/Cabacadosinformaticos/chess-variant-cli.git
cd chess-variant-cli
dotnet run --project src/Project
```

Type one instruction per line. An empty line ends the program. To play a scripted game:

```bash
dotnet run --project src/Project < iotests/04-check-and-checkmate.in
```

## Instructions

| Instruction | Syntax | What it does |
|---|---|---|
| `RJ` | `RJ Name` | Registers a player |
| `LJ` | `LJ` | Lists players by wins, with games, wins, draws and losses |
| `IJ` | `IJ Novo White Black` | Starts a new game |
| `IJ` | `IJ Continuação White Black` + 8 board lines | Starts a game from a given position |
| `MP` | `MP Name From To` | Moves a piece, e.g. `MP Ana E7 E5` |
| `OS` | `OS Name From [To]` | Uses the special operation of a piece |
| `DJ` | `DJ` | Shows the board and whose turn it is |
| `D` | `D Name [Name]` | Resigns (both names: a draw) |
| `G` | `G File` | Saves the players and the current game |
| `L` | `L File` | Loads players and game from a file |

For `IJ Continuação`, each of the 8 following lines is one row of the board, with the 8
cells separated by commas and empty cells left blank:

```text
IJ Continuação Ana Rui
BR1,BH1,BB1,BQ1,BK1,BB2,BH2,BR2
BP1,BP2,BP3,,BP5,BP6,BP7,BP8
,,,,,,,
,,,BP4,,,,
,,,,,,,
,,WH1,,,,,
WP1,WP2,WP3,WP4,WP5,WP6,WP7,WP8
WR1,,WB1,WQ1,WK1,WB2,WH2,WR2
```

Unknown instructions, or a wrong number of arguments, print `Instrução inválida.` When an
instruction fails for more than one reason, only the first error in the order defined by the
assignment is printed. The full list of messages is in the [briefing](docs/BRIEFING.md).

## Example

The shortest checkmate, played by Black:

```text
RJ Ana
Jogador registado com sucesso.
RJ Rui
Jogador registado com sucesso.
IJ Novo Ana Rui
Jogo iniciado com sucesso.
MP Ana F7 F6
WP6 movida com sucesso.
MP Rui E2 E4
BP5 movida com sucesso.
MP Ana G7 G5
WP7 movida com sucesso.
MP Rui D1 H5
Checkmate. Rui venceu.
LJ
Rui 1 1 0 0
Ana 1 0 0 1
```

## Tests

`iotests/` holds input/output pairs: each `NN-name.in` is fed to the program and the result
must match `NN-name.out` byte by byte.

```bash
./run-tests.sh
```

On Windows, in PowerShell:

```powershell
./run-tests.ps1
```

| Test | Covers |
|---|---|
| `01-players` | Registering and listing players |
| `02-new-game-and-board` | Starting a game and the board printed by `DJ` |
| `03-moves-and-captures` | Normal moves and captures |
| `04-check-and-checkmate` | Check, illegal moves into check and checkmate |
| `05-castling-en-passant-promotion` | The special moves of chess |
| `06-continued-game` | Starting from a given board |
| `07-special-operations` | The five special operations and their limits |
| `08-resign-and-stats` | Resignation, draws and the statistics in `LJ` |
| `09-save-and-load` | Saving a game, loading it and playing on |
| `10-errors` | The error messages of every instruction |

The tests also run on every push through GitHub Actions.

## Project structure

```text
chess-variant-cli
├── src/Project
│   ├── Program.cs                      entry point: UTF-8 console, starts the CLI
│   ├── Views/CLI.cs                    reads instructions and prints the answers
│   ├── Controllers
│   │   ├── GameController.cs           game state, turns, MP, OS, DJ and D
│   │   ├── PlayerController.cs         registered players, RJ and LJ
│   │   └── PersistenceController.cs    G and L, the save file format
│   └── Models
│       ├── Board.cs                    8 x 8 grid, initial layout and rendering
│       ├── Piece.cs                    colour, type, index and movement state
│       ├── Pieces/                     Pawn, Rook, Horse, Bishop, Queen and King
│       ├── MoveRules.cs                attacked squares and check detection
│       ├── Player.cs                   name and results
│       └── Position.cs                 parsing of squares such as E4
├── iotests                             input/output test pairs
├── docs                                assignment briefing and project report
└── run-tests.sh / run-tests.ps1
```

Models hold the state and the rules, controllers carry out the instructions and return the
messages, and only the view writes to the console. Each piece class holds both its normal
movement and its special operation.

To check whether a move is legal, the game plays it on the board, tests whether the
player's king is attacked, and undoes it. Checkmate is a check with no legal move left.

## Design notes

The assignment leaves a few situations open. These are the choices made:

- `MP` prints one line, the first that applies in this order: checkmate, check, capture,
  promotion, castling, en passant, normal move. An en passant capture prints
  `En passant efetuado.`
- `DJ` prints the board followed by `Vez de Name.`
- Castling also requires that the king is not in check and does not cross an attacked
  square, as in normal chess.
- A promoted pawn becomes a queen with the next index after the highest queen index of
  its colour on the board.
- A position with no legal move and no check (stalemate) ends the game in a draw.
- In a continued game, White moves first and every piece counts as not yet moved.
- Special operations cannot capture a king. The bishop operation is refused when there is
  no pawn to capture. After an operation, a second line reports a check or a checkmate.
- The `IJ Novo` and `IJ Continuação` players are White and Black, in that order.
- `LJ` breaks ties by name using the invariant culture, so the order is the same on every
  computer.
- `G` saves to a plain text file whose format is described at the top of
  `PersistenceController.cs`.

## Documents

- [Assignment briefing](docs/BRIEFING.md) (Portuguese): rules, instructions and evaluation.
- [Project report](docs/REPORT.md): architecture, data structures, algorithms and the decisions on open points.
