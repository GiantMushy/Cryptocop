namespace Cryptocop.Software.API.Services.Interfaces;

public interface IQueueService
{
    Task PublishMessage(string routingKey, object body);
}