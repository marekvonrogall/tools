using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using MusicService;

namespace MusicService.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly MusicLibrary _musicLibrary;
    private readonly HttpClient _httpClient;

    public MusicController(IWebHostEnvironment env, MusicLibrary musicLibrary, HttpClient httpClient)
    {
        _env = env;
        _musicLibrary = musicLibrary;
        _httpClient = httpClient;
        if (_musicLibrary.StoredMusicLibrary == null)
            _musicLibrary.StoredMusicLibrary = new List<MusicFileMetadata>();
    }

    [HttpGet("ping")]
    public IActionResult Ping() => Ok("works! (MusicService)");

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0) {
            return BadRequest("No file uploaded.");
        }

        // Save file
        var uploadsPath = Path.Combine(_env.ContentRootPath, "UploadedMusic");
        Directory.CreateDirectory(uploadsPath);
        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsPath, fileName);
        await using (var stream = new FileStream(filePath, FileMode.Create)) {
            await file.CopyToAsync(stream);
        }

        // Default metadata
        string title = "Unknown Title";
        string artist = "Unknown Artist";
        string album = "Unknown Album";
        double durationSeconds = 0;
        string coverUrl = null;

        try // Get metadata from iTunes
        {
            var baseName = Path.GetFileName(fileName);
            while (Path.HasExtension(baseName))
                baseName = Path.GetFileNameWithoutExtension(baseName);
            var searchTerm = Uri.EscapeDataString(baseName);
´
            var apiUrl = $"https://itunes.apple.com/search?term={searchTerm}&media=music&limit=1";
            var json = await _httpClient.GetStringAsync(apiUrl);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
            {
                var item = results[0];
                if (item.TryGetProperty("trackName", out var tname))
                    title = tname.GetString();
                if (item.TryGetProperty("artistName", out var aname))
                    artist = aname.GetString();
                if (item.TryGetProperty("collectionName", out var cname))
                    album = cname.GetString();
                if (item.TryGetProperty("trackTimeMillis", out var tmillis) && tmillis.TryGetInt64(out var ms))
                    durationSeconds = ms / 1000.0;
                if (item.TryGetProperty("artworkUrl100", out var artUrl))
                {
                    coverUrl = artUrl.GetString().Replace("100x100bb.jpg", "600x600bb.jpg");
                }
            }
        }
        catch
        {
            // API fetch failed
        }

        var metadata = new MusicFileMetadata
        {
            FileName = fileName,
            Path = $"/UploadedMusic/{fileName}",
            Title = title,
            Artist = artist,
            Album = album,
            Duration = durationSeconds,
            CoverUrl = coverUrl
        };

        _musicLibrary.StoredMusicLibrary.Add(metadata);
        return Ok(metadata);
    }

    [HttpGet("library")]
    public IActionResult GetLibrary() => Ok(_musicLibrary.StoredMusicLibrary);
}
