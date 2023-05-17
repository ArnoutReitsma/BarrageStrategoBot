namespace BarrageStrategoArchieBot.Model;

public record Coordinate
{

    public override string ToString()
    {
        return $"{X},{Y}";
    }

    public int X { get; set; }
    public int Y { get; set; }
}
