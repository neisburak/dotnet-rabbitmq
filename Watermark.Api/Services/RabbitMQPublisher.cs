using System.Text;
using System.Text.Json;
using Watermark.Api.Models.RabbitMQ;

namespace Watermark.Api.Services;

public class RabbitMQPublisher
{
    private readonly RabbitMQClientManager _clientManager;

    public RabbitMQPublisher(RabbitMQClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public void Publish(ImageCreatedEvent createdEvent)
    {
        var channel = _clientManager.Connect();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(createdEvent));

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(RabbitMQClientManager.ExchangeName, RabbitMQClientManager.RoutingKey, true, properties, body);
    }
}