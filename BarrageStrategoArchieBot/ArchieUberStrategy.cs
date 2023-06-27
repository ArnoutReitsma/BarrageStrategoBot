using BarrageStrategoArchieBot.Model;
using BarrageStrategoArchieBot.Model.Commands;
using Newtonsoft.Json;

namespace BarrageStrategoArchieBot;

public class ArchieUberStrategy : Strategy
{
    private GameState gameState;
    private const int MaxMemory = 4; // Adjust this value as necessary
    private List<MoveCommand> moveHistory = new List<MoveCommand>();

    protected override SetupBoardCommand DoSetupBoard(GameInit init)
    {
        var randomFileId = 1; //new Random().Next(1, 5);
        var redFile = File.ReadAllText($"SetupBoard{randomFileId}.json");
        var board = JsonConvert.DeserializeObject<SetupBoardCommand>(redFile) ?? throw new Exception("SetupBoardRed{randomFileId}.json deserialization error");
        if (init.You == (int)Player.Red)
        {
            return board;
        }

        return MirrorCoordinates(board);
    }


    public static SetupBoardCommand MirrorCoordinates(SetupBoardCommand setupCommand)
    {
        SetupBoardCommand mirroredSetupCommand = new SetupBoardCommand();

        foreach (var piecePosition in setupCommand.Pieces)
        {
            // Calculate the mirrored Y-coordinate based on the middle of the board
            int mirroredY = 9 - (piecePosition.Position.Y - 4) + 4;

            // Create a new mirrored piece position
            PiecePosition mirroredPiecePosition = new PiecePosition
            {
                Rank = piecePosition.Rank,
                Position = new Coordinate { X = piecePosition.Position.X, Y = mirroredY }
            };

            // Add the mirrored piece position to the mirrored setup command
            mirroredSetupCommand.Pieces.Add(mirroredPiecePosition);
        }

        return mirroredSetupCommand;
    }

    protected override MoveCommand DoMove(GameState state)
    {
        // Generate all possible moves
        List<MoveCommand> allPossibleMoves = GeneratePossibleMoves(state);

        // Filter out the moves that would repeat a previous sequence of moves
        List<MoveCommand> nonRepeatingMoves = allPossibleMoves.Where(move => !IsRepeating(move)).ToList();

        // If there are non-repeating moves, consider only those moves. Otherwise, consider all moves
        List<MoveCommand> consideredMoves = nonRepeatingMoves.Any() ? nonRepeatingMoves : allPossibleMoves;

        // Find the move with the highest score
        MoveCommand bestMove = consideredMoves
            .OrderByDescending(move => GetScoreOfMove(state, move))
            .First();

        // Remember the move
        RememberMove(bestMove);

        return bestMove;
    }

    private double GetScoreOfMove(GameState state, MoveCommand move)
    {
        GameState hypotheticalState = GetHypotheticalState(state, move);
        return EvaluateGameState(hypotheticalState);
    }

    private List<MoveCommand> GeneratePossibleMoves(GameState state, bool getEnemyMoves = false)
    {
        List<MoveCommand> possibleMoves = new List<MoveCommand>();
        // Iterate over each the pieces
        foreach (Cell piece in state.Board.Where(cell => ((cell.Owner == state.ActivePlayer && !getEnemyMoves) ||  (cell.Owner != state.ActivePlayer && getEnemyMoves))))
        {
            // Flags and Bombs cannot move
            if (piece.Rank == Rank.Flag || piece.Rank == Rank.Bomb)
            {
                return possibleMoves;
            }

            // Generate possible moves in each direction: up, down, left, right
            Coordinate[] directions = new[]
            {
                new Coordinate { X = 0, Y = 1 },
                new Coordinate { X = 0, Y = -1 },
                new Coordinate { X = 1, Y = 0 },
                new Coordinate { X = -1, Y = 0 }
            };

            foreach (var direction in directions)
            {
                var steps = 0;
                var target = piece.Coordinate;
                while (steps < 1 || piece.Rank == Rank.Scout)
                {
                    steps++;
                    target = AddCoordinates(target, direction);
                    var targetCell = state.GetCellAtPosition(target);
                    // Check if the target is out of bounds, water, or other piece.
                    if (targetCell == null || targetCell.IsWater || ((targetCell.Owner == Me && !getEnemyMoves) || (targetCell.Owner != Me && getEnemyMoves)))
                    {
                        break;
                    }

                    possibleMoves.Add(new MoveCommand { From = piece.Coordinate, To = target });

                    // After we encounter a piece, we can't move further.
                    if (targetCell.Owner != null)
                    {
                        break;
                    }
                }
            }
        }

        return possibleMoves;
    }

    private GameState GetHypotheticalState(GameState currentState, MoveCommand move)
    {
        // Create a copy of the current game state
        GameState hypotheticalState = new GameState
        {
            Board = currentState.Board.Select(cell => new Cell
            {
                Rank = cell.Rank,
                Owner = cell.Owner,
                IsWater = cell.IsWater,
                IsRevealed = cell.IsRevealed,
                Coordinate = new Coordinate { X = cell.Coordinate.X, Y = cell.Coordinate.Y }
            }).ToArray(),
            ActivePlayer = currentState.ActivePlayer,
            BattleResult = currentState.BattleResult,
            LastMove = currentState.LastMove,
            TurnNumber = currentState.TurnNumber
        };

        // Find the cell in the hypothetical board that corresponds to the 'from' cell in the move
        Cell fromCell = hypotheticalState.Board.First(cell =>
            cell.Coordinate.Equals(move.From));

        // Find the cell in the hypothetical board that corresponds to the 'to' cell in the move
        Cell toCell = hypotheticalState.Board.First(cell =>
            cell.Coordinate.Equals(move.To));

        // Apply the move to the hypothetical board
        toCell.Rank = fromCell.Rank;
        toCell.Owner = fromCell.Owner;
        toCell.IsRevealed = fromCell.IsRevealed;

        fromCell.Rank = null;
        fromCell.Owner = null;
        fromCell.IsRevealed = false;

        return hypotheticalState;
    }

    private double GetPieceValue(Rank? rank)
    {
        // Assign points based on the rank of the piece
        // The actual values would depend on your strategy
        switch (rank)
        {
            case Rank.Marshal: return 15.0;
            case Rank.General: return 12.0;
            case Rank.Scout: return 9.0;
            case Rank.Miner: return 8.0;
            case Rank.Spy: return 6.0;
            case Rank.Bomb: return 5.0;
            case Rank.Flag: return 4.0;
            default: return 0.0;
        }
    }


    private double EvaluateGameState(GameState state)
    {
        double score = 0.0;
        // Scoring for pi
        foreach (Cell cell in state.Board)
        {
            if (cell.Owner == state.ActivePlayer)
            {
                // Add points based on the rank of the piece
                score += GetPieceValue(cell.Rank);
            }
            else if (cell.Owner != null)
            {
                // Subtract points based on the rank of the piece
                score -= GetPieceValue(cell.Rank);
            }
        }

        // Scoring for potential captures
        foreach (MoveCommand potentialMove in GeneratePossibleMoves(state))
        {
            Cell targetCell = state.Board.First(cell =>
                cell.Coordinate.X == potentialMove.To.X && cell.Coordinate.Y == potentialMove.To.Y);

            if (targetCell.Owner != state.ActivePlayer && targetCell.IsRevealed)
            {
                if ((targetCell.Rank == Rank.Marshal && state.GetCellAtPosition(potentialMove.From)?.Rank == Rank.Spy) ||
                    (targetCell.Rank != Rank.Bomb && state.GetCellAtPosition(potentialMove.From)?.Rank > targetCell.Rank))
                {
                    // Add points if you can capture an enemy piece
                    score += GetPieceValue(targetCell.Rank);
                }
                else if (targetCell.Rank == Rank.Bomb && state.GetCellAtPosition(potentialMove.From)?.Rank == Rank.Miner)
                {
                    // Add points if a Miner can defuse a Bomb
                    score += GetPieceValue(targetCell.Rank);
                }
            }
        }

        // Scoring for potential losses
        foreach (MoveCommand potentialMove in GeneratePossibleMoves(state, true))
        {
            Cell targetCell = state.Board.First(cell => cell.Coordinate.Equals(potentialMove.To));

            if (targetCell.Owner == state.ActivePlayer && targetCell.IsRevealed)
            {
                if ((targetCell.Rank == Rank.Spy && state.GetCellAtPosition(potentialMove.From)?.Rank == Rank.Marshal) ||
                    (targetCell.Rank != Rank.Bomb && state.GetCellAtPosition(potentialMove.From)?.Rank < targetCell.Rank))
                {
                    // Subtract points if an enemy can capture your piece
                    score -= GetPieceValue(targetCell.Rank);
                }
            }
        }

        // Scoring for flag defense
        Cell flagCell = state.Board.First(cell => cell.Owner == state.ActivePlayer && cell.Rank == Rank.Flag);
        double minEnemyDistanceToFlag = state.Board.Where(cell => cell.Owner != state.ActivePlayer)
            .Min(cell => GetManhattanDistance(cell.Coordinate, flagCell.Coordinate));
        double minMyDistanceToFlag = state.Board.Where(cell => cell.Owner == state.ActivePlayer)
            .Min(cell => GetManhattanDistance(cell.Coordinate, flagCell.Coordinate));

        if (minEnemyDistanceToFlag < minMyDistanceToFlag)
        {
            // Subtract points if an enemy piece is closer to your flag than any of your pieces
            score -= 20.0;
        }

        return score;
    }

    private bool IsRepeating(MoveCommand move)
    {
        // Check if the proposed move would repeat a sequence of moves from the history
        for (int i = 0; i < moveHistory.Count - MaxMemory; i++)
        {
            if (moveHistory.Skip(i).Take(MaxMemory).SequenceEqual(moveHistory.Skip(i + 1).Take(MaxMemory).Append(move)))
            {
                return true;
            }
        }

        return false;
    }

    private void RememberMove(MoveCommand move)
    {
        moveHistory.Add(move);

        // Limit the size of the move history
        while (moveHistory.Count > MaxMemory)
        {
            moveHistory.RemoveAt(0);
        }
    }

    private int GetManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    protected override void ProcessOpponentMove(GameState state)
    {
        // Do nothing, just a random move bot
    }

    private static Coordinate AddCoordinates(Coordinate c1, Coordinate c2)
    {
        return new Coordinate { X = c1.X + c2.X, Y = c1.Y + c2.Y };
    }
}