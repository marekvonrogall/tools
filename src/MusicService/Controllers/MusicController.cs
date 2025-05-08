using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.StaticFiles;
using MusicService;

namespace MusicService.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly MusicLibrary _musicLibrary;
    private readonly HttpClient _httpClient;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();


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

        MusicFileMetadata metadata = await ExtractMetadataAsync(filePath, fileName);

        _musicLibrary.StoredMusicLibrary.Add(metadata);
        return Ok(metadata);
    }

    private async Task<MusicFileMetadata> ExtractMetadataAsync(string filePath, string fileName)
    {
        string title = "Unknown Title";
        string artist = "Unknown Artist";
        string album = "Unknown Album";
        double durationSeconds = 0;
        string coverUrl = null;

        try
        {
            string baseName = Path.GetFileName(fileName);
            while (Path.HasExtension(baseName)) { // for files like .flac.m4a
                baseName = Path.GetFileNameWithoutExtension(baseName);
            }
            string searchTerm = Uri.EscapeDataString(baseName);

            string apiUrl = $"https://itunes.apple.com/search?term={searchTerm}&media=music&limit=1";
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
                    coverUrl = artUrl.GetString()?.Replace("100x100bb.jpg", "600x600bb.jpg");
            }
        }
        catch
        {
            // API fetch failed
        }

        return new MusicFileMetadata
        {
            FileName = fileName,
            Path = $"/UploadedMusic/{fileName}",
            Title = title,
            Artist = artist,
            Album = album,
            Duration = durationSeconds,
            CoverUrl = coverUrl
        };
    }

    public async Task RefreshLibrary()
    {
        var uploadsPath = Path.Combine(_env.ContentRootPath, "UploadedMusic");
        if (!Directory.Exists(uploadsPath))
            return;

        var files = Directory.GetFiles(uploadsPath);
        _musicLibrary.StoredMusicLibrary.Clear();

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            var metadata = await ExtractMetadataAsync(filePath, fileName);
            _musicLibrary.StoredMusicLibrary.Add(metadata);
        }
    }

    [HttpGet("library")]
    public IActionResult GetLibrary() => Ok(_musicLibrary.StoredMusicLibrary);

    [HttpDelete("delete")]
    public IActionResult Delete([FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) {
            return BadRequest("fileName is required");
        }
        MusicFileMetadata item = _musicLibrary.StoredMusicLibrary.FirstOrDefault(m => m.FileName == fileName);
        if (item != null) {
            _musicLibrary.StoredMusicLibrary.Remove(item);
        }
        string uploadsPath = Path.Combine(_env.ContentRootPath, "UploadedMusic");
        string filePath = Path.Combine(uploadsPath, fileName);
        if (System.IO.File.Exists(filePath)) {
            System.IO.File.Delete(filePath);
        }
        return Ok();
    }
    
    [HttpGet("stream")]
    public IActionResult Stream([FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) {
            return BadRequest("fileName is required");
        }
        string uploadsPath = Path.Combine(_env.ContentRootPath, "UploadedMusic");
        string filePath = Path.Combine(uploadsPath, fileName);
        if (!System.IO.File.Exists(filePath)) {
            return NotFound();
        }
        string contentType;
        if (!_contentTypeProvider.TryGetContentType(fileName, out contentType)) {
            contentType = "application/octet-stream";
        }
        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fs, contentType, enableRangeProcessing: true);
    }
}
