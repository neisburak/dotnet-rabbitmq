using Api.Data;
using Api.Models;
using Api.Models.RabbitMQ;
using Api.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ProductManager : IProductService
{
    private readonly DataContext _dataContext;
    private readonly RabbitMQPublisher _publisher;

    public ProductManager(DataContext dataContext, RabbitMQPublisher publisher)
    {
        _publisher = publisher;
        _dataContext = dataContext;
    }

    #region Helper Methods
    private bool UploadFile(IFormFile file, out string imageName)
    {
        try
        {
            if (file is { Length: > 0 })
            {
                imageName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageName);

                using var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);

                return true;
            }
            else
            {
                imageName = default!;
                return false;
            }
        }
        catch
        {
            imageName = default!;
            return false;
        }
    }
    #endregion

    public async Task<Product?> GetAsync(int id, CancellationToken cancellationToken) => await _dataContext.Products.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<Product>> GetAsync(CancellationToken cancellationToken) => await _dataContext.Products.ToListAsync(cancellationToken);

    public async Task<bool> AddAsync(ProductForUpsert productForUpsert, CancellationToken cancellationToken)
    {
        var product = productForUpsert.Adapt<Product>();
        if (UploadFile(productForUpsert.Image, out string imageName))
        {
            product.Image = imageName;

            _publisher.Publish(new ImageCreatedEvent { Name = imageName });
        }
        await _dataContext.AddAsync(product, cancellationToken);
        return await _dataContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(int id, ProductForUpsert productForUpsert, CancellationToken cancellationToken)
    {
        var product = productForUpsert.Adapt<Product>();
        product.Id = id;
        if (UploadFile(productForUpsert.Image, out string imageName))
        {
            product.Image = imageName;

            _publisher.Publish(new ImageCreatedEvent { Name = imageName });
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