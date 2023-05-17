using BarrageStrategoArchieBot.Model.Commands;

namespace BarrageStrategoArchieBot.Model;

public class GameState
{
    public Cell[] Board { get; set; } = Array.Empty<Cell>();
    public Player ActivePlayer { get; set; }
    public Battle? BattleResult { get; set; }
    public MoveCommand? LastMove { get; set; }
    public int TurnNumber { get; set; }
}
