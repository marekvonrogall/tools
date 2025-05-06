using Microsoft.AspNetCore.Mvc;

namespace MusicService.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public MusicController(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        string message = "works! (MusicService)";
        return Ok(message);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var uploadsPath = Path.Combine(_env.ContentRootPath, "UploadedMusic");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var filePath = Path.Combine(uploadsPath, Path.GetFileName(file.FileName));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { file = file.FileName, path = filePath });
    }
}
