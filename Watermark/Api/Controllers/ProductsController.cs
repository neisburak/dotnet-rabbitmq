using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetAsync(id, cancellationToken);
        return product is not null ? Ok(product) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return Ok(await _productService.GetAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(ProductForUpsert model, CancellationToken cancellationToken)
    {
        var result = await _productService.AddAsync(model, cancellationToken);
        return result ? Ok() : BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, ProductForUpsert model, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, model, cancellationToken);
        return result ? Ok() : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id, ProductForUpsert model, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(id, cancellationToken);
        return result ? Ok() : BadRequest();
    }
}