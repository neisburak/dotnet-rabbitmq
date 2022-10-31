using System.Text;
using System.Text.Json;
using Shared;

namespace Mvc.Services;

public class RabbitMQPublisher
{
    private readonly RabbitMQClientManager _clientManager;

    public RabbitMQPublisher(RabbitMQClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public void Publish(CreateExcelMessage excelMessage)
    {
        var channel = _clientManager.Connect();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(excelMessage));

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(RabbitMQClientManager.ExchangeName, RabbitMQClientManager.RoutingKey, true, properties, body);
    }
}