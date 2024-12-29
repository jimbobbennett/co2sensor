using System.Text.Json.Serialization;

namespace CO2Sensor.Shared;

public record CO2Measurement
{
    [JsonPropertyName("co2")]
    public double CO2 { get; init; }
}
