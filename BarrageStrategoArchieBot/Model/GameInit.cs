
namespace BarrageStrategoArchieBot.Model;

public class GameInit
{
    public Player You { get; set; }

    public Rank[] AvailablePieces { get; set; } = Array.Empty<Rank>();
}
