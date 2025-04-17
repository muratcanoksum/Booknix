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
using Booknix.Application.Helpers; // EmailHelper burada
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// .env dosyas�n� y�kle
//Env.Load();
Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));


// PostgreSQL ba�lant� c�mlesini olu�tur
var connectionString = $"Host={Env.GetString("DB_HOST")};" +
                       $"Port={Env.GetString("DB_PORT")};" +
                       $"Database={Env.GetString("DB_NAME")};" +
                       $"Username={Env.GetString("DB_USER")};" +
                       $"Password={Env.GetString("DB_PASSWORD")}";

// DbContext
builder.Services.AddDbContext<BooknixDbContext>(options =>
    options.UseNpgsql(connectionString));

// Dependency Injection
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IUserProfileRepository, EfUserProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddSingleton<IAppSettings, AppSettings>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Token s�resi yap�land�rmas� (EmailHelper'a aktar)
var appSettings = app.Services.GetRequiredService<IAppSettings>();
EmailHelper.Configure(appSettings.TokenExpireMinutes);

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseExceptionHandler("/Error/500");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
