using Microsoft.AspNetCore.Mvc;

namespace vrmo.Controllers;

[ApiController]
[Route("[controller]")]
public class VRMOController : ControllerBase
{
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        string message = "works! (VRMO)";
        return Ok(message);
    }
}
