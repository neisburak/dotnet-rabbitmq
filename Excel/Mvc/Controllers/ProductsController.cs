using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Data;
using Mvc.Models;
using Mvc.Services;
using Shared;

namespace Mvc.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly DataContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RabbitMQPublisher _publisher;

    public ProductsController(ILogger<ProductsController> logger, DataContext context, UserManager<IdentityUser> userManager, RabbitMQPublisher publisher)
    {
        _logger = logger;
        _context = context;
        _publisher = publisher;
        _userManager = userManager;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> CreateExcel()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name);
        var userFile = new UserFile
        {
            UserId = user.Id,
            Name = Guid.NewGuid().ToString(),
            Status = FileStatus.Creating
        };
        await _context.UserFiles.AddAsync(userFile);
        await _context.SaveChangesAsync();

        _publisher.Publish(new CreateExcelMessage { FileId = userFile.Id });

        TempData["CreationStarted"] = true;
        
        return RedirectToAction("Files");
    }

    public async Task<IActionResult> Files()
    {
        var user = await _userManager.FindByNameAsync(User.Identity?.Name);
        var files = await _context.UserFiles.ToListAsync();
        return View(files);
    }
}