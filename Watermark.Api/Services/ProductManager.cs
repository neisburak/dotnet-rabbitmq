using Watermark.Api.Data;
using Watermark.Api.Extensions;
using Watermark.Api.Models;
using Watermark.Api.Models.RabbitMQ;
using Watermark.Api.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Watermark.Api.Services;

public class ProductManager : IProductService
{
    private readonly DataContext _dataContext;
    private readonly RabbitMQPublisher _publisher;

    public ProductManager(DataContext dataContext, RabbitMQPublisher publisher)
    {
        _publisher = publisher;
        _dataContext = dataContext;
    }

    public async Task<Product?> GetAsync(int id, CancellationToken cancellationToken) => await _dataContext.Products.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<Product>> GetAsync(CancellationToken cancellationToken) => await _dataContext.Products.ToListAsync(cancellationToken);

    public async Task<bool> AddAsync(ProductForUpsert productForUpsert, CancellationToken cancellationToken)
    {
        var product = productForUpsert.Adapt<Product>();
        var result = await productForUpsert.Image.UploadAsync(cancellationToken);
        if (result.Status)
        {
            product.Image = result.Name!;
            _publisher.Publish(new ImageCreatedEvent { Name = product.Image });
        }
        await _dataContext.AddAsync(product, cancellationToken);
        return await _dataContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(int id, ProductForUpsert productForUpsert, CancellationToken cancellationToken)
    {
        var product = productForUpsert.Adapt<Product>();
        product.Id = id;
        var result = await productForUpsert.Image.UploadAsync(cancellationToken);
        if (result.Status)
        {
            product.Image = result.Name!;
            _publisher.Publish(new ImageCreatedEvent { Name = product.Image });
        }
        _dataContext.Entry<Product>(product).State = EntityState.Modified;
        return await _dataContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _dataContext.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product is null) return false;
        _dataContext.Products.Remove(product);
        return await _dataContext.SaveChangesAsync(cancellationToken) > 0;
    }
}