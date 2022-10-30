using Api.Models;

namespace Api.Services.Interfaces;

public interface IProductService
{
    Task<Product?> GetAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<Product>> GetAsync(CancellationToken cancellationToken);
    Task<bool> AddAsync(ProductForUpsert productForUpsert, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, ProductForUpsert productForUpsert, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}