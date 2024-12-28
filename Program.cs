using System.Device.I2c;
using System.Drawing;
using Iot.Device.Blinkt;
using Iot.Device.Scd4x;
using UnitsNet;

I2cConnectionSettings settings = new(1, Scd4x.DefaultI2cAddress);

I2cDevice device = I2cDevice.Create(settings);
Scd4x sensor = new(device);

Blinkt blinkt = new();

while (true)
{
    // Read the measurement.
    // This async operation will not finish until the next measurement period.
    (VolumeConcentration? co2, RelativeHumidity? hum, Temperature? temp) =
        await sensor.ReadPeriodicMeasurementAsync();

    if (co2 is null || hum is null || temp is null)
    {
        throw new Exception("CRC failure");
    }

    Console.WriteLine($"CO2: {co2.Value.PartsPerMillion} ppm");
    Console.WriteLine($"Temperature: {temp.Value.DegreesCelsius} ℃");
    Console.WriteLine($"Humidity: {hum.Value.Percent} %");

    if (co2.Value.PartsPerMillion <= 800)
    {
        blinkt.SetAll(Color.Green);
    }
    else if (co2.Value.PartsPerMillion <= 1200)
    {
        blinkt.SetAll(Color.Yellow);
    }
    else if (co2.Value.PartsPerMillion <= 1800)
    {
        blinkt.SetAll(Color.Orange);
    }
    else
    {
        blinkt.SetAll(Color.Red);
    }

    blinkt.SetBrightness(50);
    blinkt.Show();
}
