using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;

namespace Mvc.Extensions;

public static class AppBuilderExtensions
{
    public static void ConfigureApp(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<DataContext>();

        builder.Services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        });
    }

    public static void MigrateContext(this WebApplication application)
    {
        var context = application.Services.GetRequiredService<DataContext>();
        var userManager = application.Services.GetRequiredService<UserManager<IdentityUser>>();

        context.Database.Migrate();

        if (!userManager.Users.Any())
        {
            userManager.CreateAsync(new IdentityUser { UserName = "John", Email = "john@test.com" }, "Password12*").Wait();
            userManager.CreateAsync(new IdentityUser { UserName = "Jane", Email = "jane@test.com" }, "Password12*").Wait();
        }
    }
}