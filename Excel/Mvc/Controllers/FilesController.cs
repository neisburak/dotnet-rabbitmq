using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Hubs;
using Mvc.Models;

namespace Mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IHubContext<MyHub> _hubContext;

    public FilesController(DataContext context, IHubContext<MyHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> UploadAsync(int id, IFormFile formFile)
    {
        if (formFile is not { Length: > 0 }) return BadRequest();

        var userFile = await _context.UserFiles.FirstOrDefaultAsync(f => f.Id == id);
        if (userFile is null) return BadRequest();

        var filePath = userFile.Name + Path.GetExtension(formFile.FileName);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

        using var stream = new FileStream(path, FileMode.Create);
        await formFile.CopyToAsync(stream);

        userFile.CreatedOn = DateTime.Now;
        userFile.Path = filePath;
        userFile.Status = FileStatus.Completed;

        await _context.SaveChangesAsync();

        await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");

        return Ok();
    }
}