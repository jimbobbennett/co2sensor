using System.Drawing;
using System.Text.Json;

using Iot.Device.Blinkt;

using MQTTnet;
using MQTTnet.Client;

using CO2Sensor.Shared;

// Connect to the Event Grid MQTT broker
var hostname = "co2sensor.westus-1.ts.eventgrid.azure.net";
var userName = "client2-authn-ID";
var clientId = "client2-session1";
var x509_pem = "client2-authn-ID.pem";
var x509_key = "client2-authn-ID.key";

var mqttClient = await MQTTConnection.CreateClient(x509_pem, x509_key, hostname, clientId, userName).ConfigureAwait(false);

// Set up the lights
var blinkt = new Blinkt();

mqttClient.ApplicationMessageReceivedAsync += async m =>
{
    var payload = m.ApplicationMessage.ConvertPayloadToString();

    // Log the message
    await Console.Out.WriteAsync($"Received message on topic: '{m.ApplicationMessage.Topic}' with content: '{payload}'\n\n");

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
var suback = await mqttClient.SubscribeAsync("CO2TopicSpace/co2");
suback.Items.ToList().ForEach(s => Console.WriteLine($"subscribed to '{s.TopicFilter.Topic}' with '{s.ResultCode}'"));

// Loop forever
while (true)
{
    await Task.Delay(1000);
}