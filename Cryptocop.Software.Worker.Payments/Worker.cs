using System.Text;
using Cryptocop.Software.API.Models.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cryptocop.Software.Worker.Payments;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var user = _configuration["RabbitMQ:UserName"] ?? "guest";
        var pass = _configuration["RabbitMQ:Password"] ?? "guest";
        var portStr = _configuration["RabbitMQ:Port"];
        var port = 0; int.TryParse(portStr, out port);
        var exchange = _configuration["RabbitMQ:Exchange"] ?? "cryptocop";
        var queue = _configuration["RabbitMQ:Queue"] ?? "payment-queue";
        var routingKey = _configuration["RabbitMQ:RoutingKey"] ?? "create-order";

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass
        };
        if (port > 0) factory.Port = port;

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: queue, exchange: exchange, routingKey: routingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.Span);
                var order = JsonConvert.DeserializeObject<OrderDto>(message);
                if (order == null)
                {
                    _logger.LogWarning("Payment worker: received null/invalid order message");
                    return;
                }

                var card = order.CreditCard;
                var isValid = IsValidLuhn(card);
                var validationMsg = isValid ? "VALID" : "INVALID";
                Console.WriteLine($"[Payments] Order {order.Id} card {Mask(card)} => {validationMsg}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment message");
            }
        };

        _channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
        _logger.LogInformation("Payment worker listening on queue {queue} bound to {routingKey}", queue, routingKey);

        return Task.CompletedTask;
    }

    private static bool IsValidLuhn(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return false;
        var digits = new string(number.Where(char.IsDigit).ToArray());
        if (digits.Length < 12) return false;
        int sum = 0; bool alt = false;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int n = digits[i] - '0';
            if (alt)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alt = !alt;
        }
        return sum % 10 == 0;
    }

    private static string Mask(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return string.Empty;
        var digits = new string(number.Where(char.IsDigit).ToArray());
        if (digits.Length <= 4) return new string('*', digits.Length);
        return new string('*', digits.Length - 4) + digits[^4..];
    }

    public override void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Close(); } catch { }
        try { _connection?.Dispose(); } catch { }
        base.Dispose();
    }
}
