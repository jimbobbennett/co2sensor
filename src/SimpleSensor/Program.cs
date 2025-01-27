using System.Device.I2c;
using Iot.Device.Scd4x;
using UnitsNet;

// Connect to the CO2 sensor
I2cConnectionSettings settings = new(1, Scd4x.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);
Scd4x sensor = new(device);

// Loop forever, reading CO2 measurements and publishing them to the Event Grid MQTT broker
while (true)
{
    // Read the measurement.
    // This async operation will not finish until the next measurement period, every 5 seconds
    (VolumeConcentration? co2, _, _) = await sensor.ReadPeriodicMeasurementAsync().ConfigureAwait(false);
    Console.WriteLine($"CO2: {co2.Value.PartsPerMillion} ppm");
}
