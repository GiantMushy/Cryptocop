namespace Cryptocop.Software.API.Services.Interfaces;

public interface IQueueService
{
    Task PublishMessageAsync(string routingKey, object body);
}