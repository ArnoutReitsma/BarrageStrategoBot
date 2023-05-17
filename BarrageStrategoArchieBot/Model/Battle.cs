namespace BarrageStrategoArchieBot.Model;

public class Battle
{
    public int? Winner { get; set; }
    public Piece Attacker { get; set; } = new();
    public Piece Defender { get; set; } = new();
    public Coordinate Position { get; set; } = new();
}
