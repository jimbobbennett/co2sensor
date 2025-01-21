using System.Drawing;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

// Use the local Blinkt file for now. A PR has been merged to the dotnet/iot repo with this file, but
// this hasn't been released as a nuget package yet.
using Iot.Device.Blinkt;

using MQTTnet;
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

// Set up the lights
var blinkt = new Blinkt();

mqttClient.ApplicationMessageReceivedAsync += async m =>
{
    var payload = m.ApplicationMessage.ConvertPayloadToString();

    // Log the message
    await Console.Out.WriteAsync($"Received message on topic: '{m.ApplicationMessage.Topic}' with content: '{payload}'\n").ConfigureAwait(false);

    // Update the lights
    var co2Record = JsonSerializer.Deserialize<CO2Measurement>(payload)!;

    switch (co2Record.CO2)
    {
        case <= 800:
            blinkt.SetAll(Color.Green);
            break;
        case <= 1200:
            blinkt.SetAll(Color.Yellow);
            break;
        case <= 1800:
            blinkt.SetAll(Color.Orange);
            break;
        default:
            blinkt.SetAll(Color.Red);
            break;
    }

    blinkt.SetBrightness(50);
    blinkt.Show();
};

// Start listening for messages
var suback = await mqttClient.SubscribeAsync(mqttSettings["Topic"]);
suback.Items.ToList().ForEach(s => Console.WriteLine($"subscribed to '{s.TopicFilter.Topic}' with '{s.ResultCode}'"));

// Loop forever
while (true)
{
    await Task.Delay(1000);
}