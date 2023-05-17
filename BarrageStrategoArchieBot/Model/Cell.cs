using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BarrageStrategoArchieBot.Model;

public class Cell
{
    // Null if there is no piece here, Unknown if the piece is unknown to the current player.
    [JsonConverter(typeof(NullableEnumConverter<Rank>))]
    public Rank? Rank { get; set; }

    public Player? Owner { get; set; }
    public bool IsWater { get; set; }
    public bool IsRevealed { get; set; }
    public Coordinate Coordinate { get; set; }  = new();

    public bool IsEmpty() => Rank == null && Owner == null && !IsWater;
    public bool ShouldObstructPieceForPlayer(Player player) => Owner != player && !IsEmpty();
}

public class NullableEnumConverter<T> : StringEnumConverter where T : struct
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(T?);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType == JsonToken.String)
        {
            string value = reader.Value!.ToString() ?? throw new InvalidOperationException();

            if (value == "?")
                return Rank.Unknown;
        }

        return base.ReadJson(reader, objectType, existingValue, serializer);
    }
}