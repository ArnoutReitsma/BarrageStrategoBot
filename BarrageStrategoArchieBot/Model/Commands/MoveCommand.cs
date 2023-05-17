namespace BarrageStrategoArchieBot.Model.Commands;

public class MoveCommand
{
    public Coordinate From { get; set; } = new();
    public Coordinate To { get; set; } = new();
}
