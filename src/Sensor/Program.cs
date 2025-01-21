using System.Device.I2c;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using Iot.Device.Scd4x;
using UnitsNet;

using MQTTnet.Client;

using CO2Sensor.Shared;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Read settings from configuration
var mqttSettings = configuration.GetSection("MQTT");

// Connect to the Event Grid MQTT broker
var mqttClient = await MQTTConnection.CreateClient(mqttSettings["X509Pem"], 
                                                   mqttSettings["X509Key"], 
                                                   mqttSettings["Hostname"], 
                                                   mqttSettings["ClientId"]).ConfigureAwait(false);

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

    // Check we have data
    if (co2 is null)
    {
        throw new Exception("CRC failure");
    }

    Console.WriteLine($"CO2: {co2.Value.PartsPerMillion} ppm");

    // Publish the CO2 measurement to the Event Grid MQTT broker
    var co2Record = new CO2Measurement { CO2 = co2.Value.PartsPerMillion };
    await mqttClient.PublishStringAsync(mqttSettings["Topic"], JsonSerializer.Serialize(co2Record)).ConfigureAwait(false);
}
