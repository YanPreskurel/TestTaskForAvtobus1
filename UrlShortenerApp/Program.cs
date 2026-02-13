using Microsoft.EntityFrameworkCore;
using UrlShortenerApp.Data;
using UrlShortenerApp.Services;

var builder = WebApplication.CreateBuilder(args);

// база
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ShortenerService>();

var app = builder.Build();

// для авто миграции
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseStatusCodePagesWithReExecute("/Home/Index");

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// отдельний маршрут для редиректа, чтобы ссылки были вида google.com/abcde 
app.MapControllerRoute(
    name: "short",
    pattern: "{code}",
    defaults: new { controller = "Home", action = "RedirectTo" });

app.Run();