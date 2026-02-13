using System.ComponentModel.DataAnnotations;

namespace UrlShortenerApp.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }

        [Required]
        [Url]
        public string LongUrl { get; set; } = string.Empty;

        public string ShortCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ClickCount { get; set; } = 0;
    }
}