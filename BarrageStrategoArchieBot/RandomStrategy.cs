using BarrageStrategoArchieBot.Model;
using BarrageStrategoArchieBot.Model.Commands;

namespace BarrageStrategoArchieBot;

public class RandomStrategy : Strategy
{
    protected override SetupBoardCommand DoSetupBoard(GameInit init)
    {
        var row = init.You == (int) Player.Red ? 0 : 6;
        var result = new SetupBoardCommand();

        for (var i = 0; i < init.AvailablePieces.Length; i++)
        {
            var p = init.AvailablePieces[i];
            result.Pieces.Add(new PiecePosition
            {
                Rank = p,
                Position = new Coordinate
                {
                    X = i,
                    Y = row + (int)Math.Floor(new Random().NextDouble() * 4)
                }
            });
        }

        return result;
    }

    protected override MoveCommand DoMove(GameState state)
    {
        // Get a map of all cells by coordinate.
        var cellsByCoord = state.Board.ToDictionary(c => c.Coordinate.ToString(), c => c);

        // Get all cells with allied pieces.
        var myCells = state.Board.Where(c => c.Owner == Me).ToList();

        // Get all possible moves for each piece.
        var allPossibleMoves = myCells.SelectMany(c => GetMovesForCell(cellsByCoord, c)).ToList();

        // Pick a random move.
        var randomIndex = new Random().Next(0, allPossibleMoves.Count);
        return allPossibleMoves[randomIndex];
    }

    protected override void ProcessOpponentMove(GameState state)
    {
        // Do nothing, just a random move bot
    }

    private List<MoveCommand> GetMovesForCell(Dictionary<string, Cell> cellsByCoord, Cell cell)
    {
        if (cell.Rank == Rank.Flag || cell.Rank == Rank.Bomb)
        {
            return new List<MoveCommand>();
        }

        var result = new List<MoveCommand>();

        var deltas = new[]
        {
            new Coordinate { X = 1, Y = 0 },
            new Coordinate { X = -1, Y = 0 },
            new Coordinate { X = 0, Y = 1 },
            new Coordinate { X = 0, Y = -1 }
        };

        foreach (var delta in deltas)
        {
            var target = cell.Coordinate;
            var steps = 0;

            while (steps < 1 || cell.Rank == Rank.Scout)
            {
                steps++;
                target = AddCoordinates(target, delta);
                // Check if the target is out of bounds, water, or our own piece.
                if (!cellsByCoord.TryGetValue(target.ToString(), out var targetCell) || targetCell.IsWater || targetCell.Owner == Me)
                {
                    break;
                }

                result.Add(new MoveCommand
                {
                    From = cell.Coordinate,
                    To = target
                });

                // After we encounter a piece, we can't move further.
                if (targetCell.Owner != null)
                {
                    break;
                }
            }
        }

        return result;
    }

    private static Coordinate AddCoordinates(Coordinate c1, Coordinate c2)
    {
        return new Coordinate { X = c1.X + c2.X, Y = c1.Y + c2.Y };
    }

}