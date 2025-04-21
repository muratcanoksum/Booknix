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
using Booknix.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5122");

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
    options
    .UseNpgsql(connectionString)
    .ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning))
);

// Dependency Injection

// Repositories
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IRoleRepository, EfRoleRepository>();
builder.Services.AddScoped<ITrustedIpRepository, EfTrustedIpRepository>();
builder.Services.AddScoped<IUserProfileRepository, EfUserProfileRepository>();
builder.Services.AddScoped<ISectorRepository, EfSectorRepository>();
builder.Services.AddScoped<IUserProfileRepository, EfUserProfileRepository>();
builder.Services.AddScoped<IMediaFileRepository, EfMediaFileRepository>();
builder.Services.AddScoped<INotificationRepository, EfNotificationRepository>();
builder.Services.AddScoped<IReviewRepository, EfReviewRepository>();
builder.Services.AddScoped<IServiceEmployeeRepository, EfServiceEmployeeRepository>();
builder.Services.AddScoped<IUserLocationRepository, EfUserLocationRepository>();
builder.Services.AddScoped<IWorkingHourRepository, EfWorkingHourRepository>();


// Unit of Work
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISectorService, SectorService>();


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

app.UseExceptionHandler("/Error/500"); // sunucu hataları (500+)
app.UseStatusCodePages(context =>
{
    var request = context.HttpContext.Request;

    if (HttpMethods.IsGet(request.Method) &&
        !request.Headers.Accept.ToString().Contains("application/json"))
    {
        var statusCode = context.HttpContext.Response.StatusCode;
        context.HttpContext.Response.Redirect($"/Error/{statusCode}");
    }

    return Task.CompletedTask; // artık return Task gerekiyor
});




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
