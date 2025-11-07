using System.Text;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Cryptocop.Software.API.Services.Implementations;

public class QueueService : IQueueService, IDisposable
{
    private readonly RabbitMQ.Client.IConnection _connection;
    private readonly RabbitMQ.Client.IModel _channel;
    private readonly string _exchangeName;

    public QueueService(IConfiguration configuration)
    {
        var host = configuration["RabbitMQ:HostName"] ?? "localhost";
        var user = configuration["RabbitMQ:UserName"] ?? "guest";
        var pass = configuration["RabbitMQ:Password"] ?? "guest";
        var portStr = configuration["RabbitMQ:Port"];
        var port = 0;
        int.TryParse(portStr, out port);
        _exchangeName = configuration["RabbitMQ:Exchange"] ?? "cryptocop";

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass
        };
        if (port > 0) factory.Port = port;

    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();
    _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
    }

    public Task PublishMessage(string routingKey, object body)
    {
        var json = JsonConvert.SerializeObject(body);
        var bytes = Encoding.UTF8.GetBytes(json);
    var props = _channel.CreateBasicProperties();
    props.ContentType = "application/json";
    props.DeliveryMode = 2; // persistent
    _channel.BasicPublish(exchange: _exchangeName, routingKey: routingKey, basicProperties: props, body: bytes);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    try { _channel?.Close(); } catch { }
    try { _channel?.Dispose(); } catch { }
        try { _connection?.Close(); } catch { }
        try { _connection?.Dispose(); } catch { }
    }
}