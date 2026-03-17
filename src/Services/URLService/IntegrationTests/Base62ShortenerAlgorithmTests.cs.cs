using URLService.Algorithm;
using Xunit;    

namespace URLService.IntegrationTests
{
    public class Base62ShortenerAlgorithmTests
    {
        private readonly Base62ShortenerAlgorithm _algorithm;

        public Base62ShortenerAlgorithmTests()
        {
            _algorithm = new Base62ShortenerAlgorithm();
        }

        [Fact]
        public void GenerateCode_SameUrl_ShouldReturnSameCode()
        {
            // Arrange
            var url = "https://google.com";

            // Act
            var code1 = _algorithm.GenerateCode(url);
            var code2 = _algorithm.GenerateCode(url);

            // Assert - Deterministic hashing check
            Assert.Equal(code1, code2);
        }

        [Fact]
        public void GenerateCode_DifferentUrls_ShouldReturnDifferentCodes()
        {
            // Arrange
            var url1 = "https://google.com";
            var url2 = "https://github.com";

            // Act
            var code1 = _algorithm.GenerateCode(url1);
            var code2 = _algorithm.GenerateCode(url2);

            // Assert
            Assert.NotEqual(code1, code2);
        }

        [Theory]
        [InlineData("https://bing.com")]
        [InlineData("https://facebook.com")]
        public void GenerateCode_ShouldReturnCorrectLength(string url)
        {
            // Act
            var code = _algorithm.GenerateCode(url);

            // Assert - Our algorithm targets 7 characters
            Assert.True(code.Length <= 7);
        }
    }
}