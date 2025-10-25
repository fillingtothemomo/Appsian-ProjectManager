using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Api.Data;
using ProjectManager.Api.Services;
using System.Text;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ----- CORS -----
static string NormalizeOrigin(string? origin)
{
    if (string.IsNullOrWhiteSpace(origin)) return string.Empty;
    origin = origin.Trim().TrimEnd('/');
    try
    {
        var uri = new Uri(origin);
        // Only scheme + host + optional port
        var normalized = $"{uri.Scheme}://{uri.Host}";
        if (!uri.IsDefaultPort) normalized += $":{uri.Port}";
        return normalized;
    }
    catch
    {
        return origin;
    }
}

var frontendOriginRaw = builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";
var frontendOrigin = NormalizeOrigin(frontendOriginRaw);

var allowedOrigins = new[]
{
    frontendOrigin,
    NormalizeOrigin("http://localhost:5173"),
    NormalizeOrigin("https://localhost:5173"),
    NormalizeOrigin("http://127.0.0.1:5173"),
    NormalizeOrigin("https://127.0.0.1:5173")
}.Where(o => !string.IsNullOrWhiteSpace(o))
 .Distinct(StringComparer.OrdinalIgnoreCase)
 .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
    {
        // Be robust to trailing slashes or minor variations
        policy.SetIsOriginAllowed(origin =>
              allowedOrigins.Contains(NormalizeOrigin(origin), StringComparer.OrdinalIgnoreCase))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // remove if not using cookies/credentials
    });
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

// JWT settings
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisWithAStrongKeyForProd";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "projectmanager.local";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

var app = builder.Build();

// ensure database created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANT: Use CORS BEFORE authentication/authorization middleware
app.UseCors("DevFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireCors("DevFrontend");

// Ensure preflight (OPTIONS) always succeeds for CORS-allowed origins
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok())
    .RequireCors("DevFrontend");

// Lightweight health endpoints (useful for Render/uptime checks)
app.MapGet("/healthz", () => Results.Text("OK"))
    .ExcludeFromDescription();
app.MapGet("/", () => Results.Text("Project Manager API online"))
    .ExcludeFromDescription();

app.Run();
