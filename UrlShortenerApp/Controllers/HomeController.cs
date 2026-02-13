using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using UrlShortenerApp.Data;
using UrlShortenerApp.Models;
using UrlShortenerApp.Services;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly ShortenerService _shortener;

    public HomeController(AppDbContext context, ShortenerService shortener)
    {
        _context = context;
        _shortener = shortener;
    }

    public async Task<IActionResult> Index()
    {
        var urls = await _context.Urls
            .AsNoTracking()
            .OrderByDescending(u => u.Id)
            .ToListAsync();

        return View(urls);
    }

    [HttpPost]
    public async Task<IActionResult> Shorten(string longUrl)
    {
        if (!IsValidUrl(longUrl))
        {
            TempData["ErrorMessage"] = "Недопустимый формат ссылки. Укажите корректный URL с доменом (напр. .com, .ru)";
            return RedirectToAction("Index");
        }

        // При 10K RPS это предотвратит заполнение одинаковыми записями.
        var existing = await _context.Urls.FirstOrDefaultAsync(u => u.LongUrl == longUrl);
        if (existing != null) return RedirectToAction("Index");

        string code;
        do
        {
            code = _shortener.GenerateCode();
        } 
        while (await _context.Urls.AnyAsync(u => u.ShortCode == code));

        var mapping = new UrlMapping
        {
            LongUrl = longUrl,
            ShortCode = code,
            CreatedAt = DateTime.UtcNow
        };

        _context.Urls.Add(mapping);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> RedirectTo(string code)
    {
        var mapping = await _context.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
        if (mapping == null) return NotFound();

        mapping.ClickCount++;

        await _context.SaveChangesAsync();

        return Redirect(mapping.LongUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string newLongUrl)
    {
        if (!IsValidUrl(newLongUrl))
        {
            TempData["ErrorMessage"] = "Ошибка обновления: некорректная ссылка.";
            return RedirectToAction(nameof(Index));
        }

        var entry = await _context.Urls.FindAsync(id);
        if (entry != null)
        {
            entry.LongUrl = newLongUrl;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var url = await _context.Urls.FindAsync(id);
        if (url != null)
        {
            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        var pattern = @"^https?://([\w-]+\.)+[\w-]{2,6}(:[0-9]+)?(/.*)?$";
        if (!Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase)) return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult)) return false;

        var parts = uriResult.Host.Split('.');
        if (parts.Length < 2) return false;

        string tld = parts.Last();

        // Ограничиваем длину чтобы отсекать мусор и не проверять страницы на существовавние 
        return tld.Length >= 2 && tld.Length <= 6 && tld.All(char.IsLetter);
    }

    public IActionResult PageNotFound()
    {
        return View();
    }
}