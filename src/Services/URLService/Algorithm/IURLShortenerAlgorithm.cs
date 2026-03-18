namespace URLService.Algorithm
{
    public interface IURLShortenerAlgorithm
    {
        // Method to generate a unique short code from a long URL
        string GenerateCode(string longUrl);
    }
}