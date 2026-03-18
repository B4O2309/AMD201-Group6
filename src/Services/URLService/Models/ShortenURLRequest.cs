namespace URLService.Models
{
    public class ShortenURLRequest
    {
        // The original long URL provided by the user
        public string LongUrl { get; set; } = string.Empty;
    }
}