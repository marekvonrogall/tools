using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("admin-ping")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AdminPing()
    {
        string message = "works! (VRMO)";
        return Ok(message);
    }
}
