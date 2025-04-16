using Booknix.Persistence.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Repositories;
using Booknix.Application.Interfaces;
using Booknix.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// .env dosyas�n� y�kle
Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

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

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
