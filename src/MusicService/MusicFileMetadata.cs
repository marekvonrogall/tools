
namespace MusicService
{
    public class MusicFileMetadata
    {
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string Album { get; set; }
        public required string FileName { get; set; }
        public required string Path { get; set; }
        public string? CoverUrl { get; set; }
        public double Duration { get; set; }
    }
}
