using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BarrageStrategoArchieBot.Model;

public class PiecePosition
{
    [JsonConverter(typeof(StringEnumConverter))]
    public Rank Rank { get; set; }

    public Coordinate Position { get; set; } = new();
}
