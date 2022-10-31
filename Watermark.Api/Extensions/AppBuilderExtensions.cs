using Watermark.Api.BackgroundServices;
using Watermark.Api.Data;
using Watermark.Api.Services;
using Watermark.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Watermark.Api.Extensions;

public static class AppBuilderExtensions
{
    public static void ConfigureApp(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(provider => new ConnectionFactory()
        {
            Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")),
            DispatchConsumersAsync = true
        });
        builder.Services.AddSingleton<RabbitMQClientManager>();
        builder.Services.AddSingleton<RabbitMQPublisher>();
        builder.Services.AddScoped<IProductService, ProductManager>();
        builder.Services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase("Products"));
        builder.Services.AddHostedService<ImageWatermarkProcessBackgroundService>();
    }
}