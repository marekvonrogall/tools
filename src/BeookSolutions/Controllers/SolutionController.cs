using Microsoft.AspNetCore.Mvc;

namespace BeookSolutions.Controllers;

[ApiController]
[Route("[controller]")]
public class SolutionController : ControllerBase
{
    [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            string message = "works! (Beook Solutions)";
            return Ok(message);
        }
}
