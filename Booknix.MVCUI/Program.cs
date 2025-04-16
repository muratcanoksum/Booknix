using Booknix.Persistence.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Repositories;
using Booknix.Application.Interfaces;
using Booknix.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// .env dosyasýný yükle
Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

// PostgreSQL baðlantý cümlesini oluþtur
var connectionString = $"Host={Env.GetString("DB_HOST")};" +
                       $"Port={Env.GetString("DB_PORT")};" +
                       $"Database={Env.GetString("DB_NAME")};" +
                       $"Username={Env.GetString("DB_USER")};" +
                       $"Password={Env.GetString("DB_PASSWORD")}";

// DbContext'i servis olarak ekle
builder.Services.AddDbContext<BooknixDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository DI kayýtlarý
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum zaman aþýmýný 30 dakika olarak ayarla
    options.Cookie.HttpOnly = true; // Çerezlerin HttpOnly olmasýný saðla
    options.Cookie.IsEssential = true; // Çerezlerin gerekli olduðunu belirt
});


var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
