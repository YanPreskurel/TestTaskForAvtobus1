using System.Text.RegularExpressions;

namespace UrlShortenerApp.Services
{
    public class ShortenerService
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string GenerateCode()
        {
            return string.Create(6, Alphabet, (buffer, alphabet) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = alphabet[Random.Shared.Next(alphabet.Length)];
                }
            });
        }

        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            var pattern = @"^https?://([\w-]+\.)+[\w-]{2,6}(:[0-9]+)?(/.*)?$";
            bool isMatch = Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);

            if (!isMatch) return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult)) return false;

            var parts = uriResult.Host.Split('.');
            if (parts.Length < 2) return false;

            string tld = parts.Last();
            return tld.Length >= 2 && tld.Length <= 6 && tld.All(char.IsLetter);
        }
    }
}