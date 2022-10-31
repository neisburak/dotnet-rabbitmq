using RabbitMQ.Client;

namespace FileWorker.Services;

public class RabbitMQClientManager : IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection = default!;
    private IModel _channel = default!;
    private readonly ILogger<RabbitMQClientManager> _logger;

    internal readonly static string QueueName = "excel-queue";

    public RabbitMQClientManager(ConnectionFactory connectionFactory, ILogger<RabbitMQClientManager> logger)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
    }

    public IModel Connect()
    {
        _connection = _connectionFactory.CreateConnection();
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }
        _channel = _connection.CreateModel();

        _logger.LogInformation("Connected to RabbitMQ");
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();

        _connection?.Close();
        _connection?.Dispose();

        _logger.LogInformation("Disconnected from RabbitMQ");
    }
}