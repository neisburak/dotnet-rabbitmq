using System.Text;
using System.Text.Json;
using Watermark.Api.Extensions;
using Watermark.Api.Models.RabbitMQ;
using Watermark.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Watermark.Api.BackgroundServices;

public class ImageWatermarkProcessBackgroundService : BackgroundService
{
    private readonly RabbitMQClientManager _clientManager;
    private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
    private IModel _channel = default!;

    public ImageWatermarkProcessBackgroundService(RabbitMQClientManager clientManager, ILogger<ImageWatermarkProcessBackgroundService> logger)
    {
        _logger = logger;
        _clientManager = clientManager;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = _clientManager.Connect();

        _channel.BasicQos(0, 1, false);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        _channel.BasicConsume(RabbitMQClientManager.QueueName, false, consumer);
        consumer.Received += Received;

        return Task.CompletedTask;
    }

    private async Task Received(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var createdImage = JsonSerializer.Deserialize<ImageCreatedEvent>(Encoding.UTF8.GetString(args.Body.ToArray()));
            if (createdImage is not null)
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var path = Path.Combine(basePath, createdImage.Name);
                var outputPath = Path.Combine(basePath, "watermarks", createdImage.Name);

                if (await path.AddWatermark(outputPath))
                {
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                _logger.LogInformation($"Watermark added to {createdImage.Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}