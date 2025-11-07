using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Cryptocop.Software.Worker.Emails;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = _config["RabbitMQ:HostName"] ?? "localhost";
        var user = _config["RabbitMQ:UserName"] ?? "guest";
        var pass = _config["RabbitMQ:Password"] ?? "guest";
        var portStr = _config["RabbitMQ:Port"];
        var port = 0;
        int.TryParse(portStr, out port);
        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass,
            Port = port > 0 ? port : AmqpTcpEndpoint.UseDefaultPort,
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        var exchange = _config["RabbitMQ:Exchange"] ?? "cryptocop";
        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

        var queueName = "email-queue";
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queue: queueName, exchange: exchange, routingKey: "create-order");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = System.Text.Encoding.UTF8.GetString(body);
                var order = JsonConvert.DeserializeObject<OrderPayload>(json);
                if (order != null)
                {
                    _logger.LogInformation("[Emails] Received order {orderId} for {email}", order.Id, order.Email);
                    await SendOrderEmailAsync(order);
                }
                else
                {
                    _logger.LogWarning("[Emails] Received invalid order message: {json}", json);
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Emails] Error processing message");
                // Nack and requeue=false to avoid infinite loops; adjust as needed
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("[Emails] Listening on queue {queue} bound to routing key 'create-order'", queueName);
        return Task.CompletedTask;
    }

    private async Task SendOrderEmailAsync(OrderPayload order)
    {
        var apiKey = _config["SendGrid:ApiKey"] ?? Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? string.Empty;
        var fromEmail = _config["SendGrid:FromEmail"] ?? "no-reply@example.com";
        var fromName = _config["SendGrid:FromName"] ?? "Cryptocop";

        var subject = $"Your Cryptocop order #{order.Id} was successful";
        var html = $@"
<!DOCTYPE html>
<html>
  <body style='font-family: Arial, sans-serif;'>
    <h2>Thank you for your order!</h2>
    <p>Hello {System.Net.WebUtility.HtmlEncode(order.FullName)},</p>
    <p>Your order has been received on {System.Net.WebUtility.HtmlEncode(order.OrderDate)}.</p>
    <h3>Shipping address</h3>
    <p>
      {System.Net.WebUtility.HtmlEncode(order.StreetName)} {System.Net.WebUtility.HtmlEncode(order.HouseNumber)}<br/>
      {System.Net.WebUtility.HtmlEncode(order.City)}, {System.Net.WebUtility.HtmlEncode(order.ZipCode)}<br/>
      {System.Net.WebUtility.HtmlEncode(order.Country)}
    </p>
    <h3>Order summary</h3>
    <ul>
      {string.Join("", order.OrderItems.Select(i => $"<li>{System.Net.WebUtility.HtmlEncode(i.ProductIdentifier)} × {i.Quantity} — ${i.TotalPrice:F2}</li>"))}
    </ul>
    <p><strong>Total:</strong> ${order.TotalPrice:F2}</p>
    <p>Best regards,<br/>Cryptocop</p>
  </body>
</html>";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("[Emails] SENDGRID_API_KEY not set. Would send email to {email} with subject '{subject}'. Body preview: {html}",
                order.Email, subject, html);
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(order.Email, order.FullName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: "Your order is successful.", htmlContent: html);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation("[Emails] SendGrid status: {status}", response.StatusCode);
    }

    private sealed class OrderPayload
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public float TotalPrice { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }

    private sealed class OrderItem
    {
        public int Id { get; set; }
        public string ProductIdentifier { get; set; } = string.Empty;
        public float Quantity { get; set; }
        public float UnitPrice { get; set; }
        public float TotalPrice { get; set; }
    }
}
