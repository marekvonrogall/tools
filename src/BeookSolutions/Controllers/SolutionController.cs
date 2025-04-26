using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace BeookSolutions.Controllers
{
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

        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromForm] IFormFile file)
        {
            return await ProcessFile(file, true);
        }

        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromForm] IFormFile file)
        {
            return await ProcessFile(file, false);
        }

        private async Task<IActionResult> ProcessFile(IFormFile file, bool newValue)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var tempFilePath = Path.GetTempFileName();

            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string connectionString = $"Data Source={tempFilePath};Version=3;";
                string updateStatement = $"UPDATE ZILPPROPERTY SET ZVALUE = '{newValue}' WHERE ZKEY = 'toolbarExerciseAnswerSolutionToggle';";

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(updateStatement, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                var modifiedFileBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath);
                var contentType = "application/octet-stream";
                var fileName = file.FileName;

                return File(modifiedFileBytes, contentType, fileName);
            }
            catch
            {
                return StatusCode(500, "An error occurred while processing the file.");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }
    }
}
