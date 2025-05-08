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
using Booknix.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Booknix.Infrastructure.Middleware;
using Booknix.Infrastructure.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

// Geliştirme ortamında UseUrls ve özel .env yolu kullan
if (env.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5122");
    Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
}
else
{
    // Yayın modunda .env bulunduğu dizinden yüklenir
    Env.Load();
}

// PostgreSQL bağlantı cümlesini oluştur
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


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

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
builder.Services.AddScoped<IWorkerRepository, EfWorkerRepository>();
builder.Services.AddScoped<IWorkingHourRepository, EfWorkingHourRepository>();
builder.Services.AddScoped<ILocationRepository, EfLocationRepository>();
builder.Services.AddScoped<IServiceRepository, EfServiceRepository>();
builder.Services.AddScoped<IWorkerWorkingHourRepository, EfWorkerWorkingHourRepository>();
builder.Services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
builder.Services.AddScoped<IAppointmentSlotRepository, EfAppointmentSlotRepository>();
builder.Services.AddScoped<IUserSessionRepository, EfUserSessionRepository>();
builder.Services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();
builder.Services.AddScoped<IEmailQueueRepository, EfEmailQueueRepository>();



// Unit of Work
builder.Services.AddScoped<IEmailSender, QueuedEmailSender>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<IRawSmtpSender, SmtpEmailSender>();


// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPublicService, PublicService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

// Hosted
builder.Services.AddHostedService<EmailQueueProcessor>();

builder.Services.AddSingleton<IAppSettings, AppSettings>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddMemoryCache();


var app = builder.Build();

// Token süresi yapılandırması (EmailHelper'a aktar)
var appSettings = app.Services.GetRequiredService<IAppSettings>();
EmailHelper.Configure(appSettings.TokenExpireMinutes, appSettings.BaseUrl);

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.UseMiddleware<SessionValidationMiddleware>();


app.UseExceptionHandler("/Error/500"); // Sunucu hataları (500+)
app.UseStatusCodePages(context =>
{
    var request = context.HttpContext.Request;

    if (HttpMethods.IsGet(request.Method) &&
        !request.Headers.Accept.ToString().Contains("application/json"))
    {
        var statusCode = context.HttpContext.Response.StatusCode;
        var requestedUrl = request.Path; // Hatalı istek yapılan URL'yi al

        // Yönlendirme ve URL'yi detaylara ekleyerek hatayı daha anlamlı hale getir
        context.HttpContext.Response.Redirect($"/Error/{statusCode}?url={requestedUrl}");
    }

    return Task.CompletedTask;
});





app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Index}/{id?}");

app.Run();
