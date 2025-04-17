using Booknix.Persistence.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Repositories;
using Booknix.Application.Interfaces;
using Booknix.Application.Services;
using Booknix.Infrastructure.Email;
using Booknix.Shared.Configuration;
using Booknix.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// .env dosyas�n� y�kle
Env.Load();
// PostgreSQL ba�lant� c�mlesini olu�tur
var connectionString = $"Host={Env.GetString("DB_HOST")};" +
                       $"Port={Env.GetString("DB_PORT")};" +
                       $"Database={Env.GetString("DB_NAME")};" +
                       $"Username={Env.GetString("DB_USER")};" +
                       $"Password={Env.GetString("DB_PASSWORD")}";

// DbContext'i servis olarak ekle
builder.Services.AddDbContext<BooknixDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository DI kay�tlar�
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<IAppSettings, AppSettings>();


builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum zaman a��m�n� 30 dakika olarak ayarla
    options.Cookie.HttpOnly = true; // �erezlerin HttpOnly olmas�n� sa�la
    options.Cookie.IsEssential = true; // �erezlerin gerekli oldu�unu belirt
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
