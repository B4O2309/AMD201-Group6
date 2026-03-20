namespace URLService.Entities
{
    public class Url
    {
        public int Id { get; set; }
        public string LongUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ClickCount { get; set; } = 0;

        /// <summary>
        /// Owner of this short URL.
        /// Extracted from JWT claim "sub" at creation time.
        /// Stored as plain int — NOT a foreign key (different database/service).
        /// </summary>
        public int UserId { get; set; }
    }
}