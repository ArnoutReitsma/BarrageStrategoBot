namespace BarrageStrategoCSharpBot.Model;

public class Battle
{
    public int? Winner { get; set; }
    public Piece Attacker { get; set; }
    public Piece Defender { get; set; }
    public Coordinate Position { get; set; }
}
