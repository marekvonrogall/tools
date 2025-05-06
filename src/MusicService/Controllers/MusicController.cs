using Microsoft.AspNetCore.Mvc;

namespace MusicService.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            string message = "works! (MusicService)";
            return Ok(message);
        }
}
