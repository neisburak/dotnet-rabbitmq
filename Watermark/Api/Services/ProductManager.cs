using Api.Data;
using Api.Models;
using Api.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ProductManager : IProductService
{
    private readonly DataContext _dataContext;

    public ProductManager(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<Product?> GetAsync(int id, CancellationToken cancellationToken)
    {
        return await _dataContext.Products.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAsync(CancellationToken cancellationToken)
    {
        return await _dataContext.Products.ToListAsync(cancellationToken);
    }

    public async Task<bool> AddAsync(ProductForUpsert productForUpsert, CancellationToken cancellationToken)
    {
        var product = productForUpsert.Adapt<Product>();
        await _dataContext.AddAsync(product, cancellationToken);
        return await _dataContext.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> UpdateAsync(int id, ProductForUpsert productForUpsert, CancellationToken cancellationToken)
    {
        var product = productForUpsert.Adapt<Product>();
        product.Id = id;
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