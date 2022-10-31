using FileWorker.Data;
using FileWorker.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace FileWorker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton(provider => new ConnectionFactory()
        {
            Uri = new Uri(context.Configuration.GetConnectionString("RabbitMQ")),
            DispatchConsumersAsync = true
        });
        services.AddSingleton<RabbitMQClientManager>();

        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseSqlServer(context.Configuration.GetConnectionString("SqlServer"));
        });

        return services;
    }
}