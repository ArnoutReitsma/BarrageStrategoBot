namespace BarrageStrategoArchieBot.Model;

public class Coordinate : IEquatable<Coordinate>
{
    public override string ToString()
    {
        return $"{X},{Y}";
    }

    public int X { get; set; }
    public int Y { get; set; }

    public virtual bool Equals(Coordinate? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}
