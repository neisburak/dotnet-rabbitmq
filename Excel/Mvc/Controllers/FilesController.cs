using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models;

namespace Mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly DataContext _context;

    public FilesController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> UploadAsync(int id, IFormFile formFile)
    {
        if (formFile is not { Length: > 0 }) return BadRequest();

        var userFile = await _context.UserFiles.FirstOrDefaultAsync(f => f.Id == id);
        if(userFile is null) return BadRequest();

        var filePath = userFile.Name + Path.GetExtension(formFile.FileName);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

        using var stream = new FileStream(path, FileMode.Create);
        await formFile.CopyToAsync(stream);

        userFile.CreatedOn = DateTime.Now;
        userFile.Path = filePath;
        userFile.Status = FileStatus.Completed;

        await _context.SaveChangesAsync();
        
        return Ok();
    }
}