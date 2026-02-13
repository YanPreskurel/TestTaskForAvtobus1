using Xunit;
using UrlShortenerApp.Services;

namespace Tests
{
    public class ShortenerServiceTests
    {
        private readonly ShortenerService _service;

        public ShortenerServiceTests()
        {
            _service = new ShortenerService();
        }

        #region Валидация URL
        [Theory]
        [InlineData("https://google.com", true)]
        [InlineData("http://yandex.ru", true)]
        [InlineData("http://www.you", true)]
        [InlineData("http://www.sdgsstgtrg", false)] 
        [InlineData("http://www.", false)]           
        [InlineData("http://", false)]               
        [InlineData("javascript:alert(1)", false)]   
        [InlineData("не-ссылка", false)]             
        public void IsValidUrl_ShouldValidateCorrectly(string url, bool expected)
        {
            bool result = _service.IsValidUrl(url);

            Assert.Equal(expected, result);
        }
        #endregion

        #region Генерация кода
        [Fact]
        public void GenerateCode_ShouldReturnCorrectLength()
        {
            var code = _service.GenerateCode();

            Assert.NotNull(code);
            Assert.Equal(6, code.Length);
        }

        [Fact]
        public void GenerateCode_ShouldUseOnlyAllowedCharacters()
        {
            const string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var code = _service.GenerateCode();

            Assert.All(code, character => Assert.Contains(character, allowed));
        }

        [Fact]
        public void GenerateCode_ShouldBeRandom()
        {
            var code1 = _service.GenerateCode();
            var code2 = _service.GenerateCode();

            Assert.NotEqual(code1, code2);
        }
        #endregion
    }
}