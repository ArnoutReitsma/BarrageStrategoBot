namespace BarrageStrategoCSharpBot.Model;

public class GameState
{
    public Cell[] Board { get; set; }
    public Player ActivePlayer { get; set; }
    public Battle? BattleResult { get; set; }
    public MoveCommand? LastMove { get; set; }
    public int TurnNumber { get; set; }
}
