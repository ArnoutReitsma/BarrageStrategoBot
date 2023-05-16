using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BarrageStrategoCSharpBot.Model;

public class Piece
{
    [JsonConverter(typeof(StringEnumConverter))]
    public Rank Rank { get; set; }

    public Player Player { get; set; }
}
