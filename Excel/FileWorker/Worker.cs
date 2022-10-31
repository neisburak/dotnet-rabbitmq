using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FileWorker.Data;
using FileWorker.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;

namespace FileWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMQClientManager _clientManager;
    private readonly IServiceProvider _serviceProvider;
    private IModel _channel = default!;

    public Worker(ILogger<Worker> logger, RabbitMQClientManager clientManager, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _clientManager = clientManager;
        _serviceProvider = serviceProvider;
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
        consumer.Received += Received;

        _channel.BasicConsume(RabbitMQClientManager.QueueName, false, consumer);

        return Task.CompletedTask;
    }

    private async Task Received(object sender, BasicDeliverEventArgs args)
    {
        var excel = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(args.Body.ToArray()));
        if(excel is null) return;
        
        using var ms = new MemoryStream();
        var workbook = new XLWorkbook();
        var ds = new DataSet();
        ds.Tables.Add(GetTable("products"));
        workbook.Worksheets.Add(ds);
        workbook.SaveAs(ms);

        var multipartFormDataContent = new MultipartFormDataContent();
        multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()), "formFile", $"{Guid.NewGuid().ToString()}.xlsx");
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync($"https://localhost:7139/api/files/{excel.FileId}", multipartFormDataContent);
        if (response.IsSuccessStatusCode)
        {
            _channel.BasicAck(args.DeliveryTag, false);
        }
    }

    private DataTable GetTable(string tableName)
    {
        using var scope = _serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<NorthwindContext>();
        var products = context.Products.ToList();
        var table = new DataTable { TableName = tableName };

        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("ProductName", typeof(int));
        table.Columns.Add("SupplierId", typeof(int?));
        table.Columns.Add("CategoryId", typeof(int?));
        table.Columns.Add("QuantityPerUnit", typeof(string));
        table.Columns.Add("UnitPrice", typeof(decimal?));
        table.Columns.Add("UnitsInStock", typeof(short?));
        table.Columns.Add("UnitsOnOrder", typeof(short?));
        table.Columns.Add("ReorderLevel", typeof(short?));
        table.Columns.Add("Discontinued", typeof(bool));

        products.ForEach(f => table.Rows.Add(f.ProductId, f.ProductName, f.SupplierId, f.CategoryId, f.QuantityPerUnit, f.UnitPrice, f.UnitsInStock, f.UnitsOnOrder, f.ReorderLevel, f.Discontinued));

        return table;
    }
}
