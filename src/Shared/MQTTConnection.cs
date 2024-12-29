using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using MQTTnet.Client;

namespace CO2Sensor.Shared;

public static class MQTTConnection
{
    public static async Task<IMqttClient> CreateClient(string x509_pem, string x509_key, string hostname, string clientId, string userName)
    {
        // Load certificate and private key from PEM files
        var certificate = new X509Certificate2(X509Certificate2.CreateFromPemFile(x509_pem, x509_key).Export(X509ContentType.Pkcs12));

        // Add the loaded certificate to a certificate collection
        X509Certificate2Collection certificates = [ certificate ];

        // Create TLS options using TLS 2/3 and the loaded certificate
        var tlsOptions = new MqttClientTlsOptionsBuilder()
                .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13)
                .WithClientCertificates(certificates)
                .Build();

        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(hostname, 8883)
            .WithClientId(clientId)
            .WithCredentials(userName, "")
            .WithTlsOptions(tlsOptions)
            .Build();

        // Create a new MQTT client
        var mqttClient = new MqttFactory().CreateMqttClient();

        // Connect to the Event Grid MQTT broker
        var connAck = await mqttClient.ConnectAsync(clientOptions).ConfigureAwait(false);

        Console.WriteLine($"Client Connected: {mqttClient.IsConnected} with CONNACK: {connAck.ResultCode}");

        return mqttClient;
    }
}