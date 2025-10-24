using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Api.Data;
using ProjectManager.Api.Services;
using System.Text;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ----- CORS -----
var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";
var allowedOrigins = new[]
{
    frontendOrigin,
    "http://localhost:5173",
    "https://localhost:5173",
    "http://127.0.0.1:5173",
    "https://127.0.0.1:5173"
}.Where(o => !string.IsNullOrWhiteSpace(o)).Distinct().ToArray();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)   // allow only explicit dev URLs
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();            // remove if not using cookies/credentials
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

app.Run();
