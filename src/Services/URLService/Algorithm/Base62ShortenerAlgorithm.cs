using System.Security.Cryptography;
using System.Text;

namespace URLService.Algorithm
{
    public class Base62ShortenerAlgorithm : IURLShortenerAlgorithm
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string GenerateCode(string longUrl)
        {
            // 1. Use MD5 to create a deterministic hash of the long URL
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(longUrl));

                // 2. Convert the first 8 bytes of the hash to a long number
                long numericValue = Math.Abs(BitConverter.ToInt64(hashBytes, 0));

                // 3. Convert that number to Base62 string
                return Encode(numericValue);
            }
        }

        private string Encode(long input)
        {
            if (input == 0) return Alphabet[0].ToString();

            var result = new StringBuilder();
            while (input > 0)
            {
                // Get the remainder (0-61)
                int index = (int)(input % 62);
                result.Insert(0, Alphabet[index]);
                input /= 62;
            }

            // Return a fixed length or substring for the short link (e.g., 7 characters)
            string code = result.ToString();
            return code.Length > 7 ? code.Substring(0, 7) : code;
        }
    }
}