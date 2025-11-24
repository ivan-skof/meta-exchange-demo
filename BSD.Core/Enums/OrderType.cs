using System.Text.Json.Serialization;

namespace BSD.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderType
{
    Buy,
    Sell,
}
