using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Services;
using RabbitMQ.Client;

namespace Mvc.Extensions;

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
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<DataContext>();

        builder.Services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        });

        builder.Services.AddSignalR();
    }

    public static void MigrateContext(this WebApplication application)
    {
        var scope = application.Services.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        context.Database.Migrate();

        if (!userManager.Users.Any())
        {
            userManager.CreateAsync(new IdentityUser { UserName = "John", Email = "john@test.com" }, "Password12*").Wait();
            userManager.CreateAsync(new IdentityUser { UserName = "Jane", Email = "jane@test.com" }, "Password12*").Wait();
        }
    }
}