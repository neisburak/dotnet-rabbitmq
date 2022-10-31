using FileWorker;
using FileWorker.Extensions;

var host = Host.CreateDefaultBuilder(args)
.ConfigureServices((context, services) =>
{
    services.AddServices(context);
    services.AddHostedService<Worker>();
})
.Build();

await host.RunAsync();
