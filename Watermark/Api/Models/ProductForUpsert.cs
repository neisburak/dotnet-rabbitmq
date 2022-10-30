namespace Api.Models;

public class ProductForUpsert
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public IFormFile Image { get; set; } = default!;
}