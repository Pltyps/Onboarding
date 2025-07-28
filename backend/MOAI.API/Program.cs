using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Data;
using MOAI.API.Models;
using MOAI.API.Services;
using DotNetEnv;
using Npgsql.EntityFrameworkCore.PostgreSQL;


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
});


// ── Load .env variables into Environment ──
Env.Load();
builder.Configuration.AddEnvironmentVariables();


// ── Core services ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── SQLite (Local) or Neon DB (Online) ──
if (builder.Environment.IsDevelopment())
{
    // ✅ Use local SQLite for development
    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        opts.UseSqlite(builder.Configuration.GetConnectionString("StorageConnection")));
}
else
{
    // ✅ Use Neon PostgreSQL for production
    var host = "ep-shy-art-af4560fp-pooler.c-2.us-west-2.aws.neon.tech";
    var dbName   = "neondb";
    var username = "neondb_owner";
    var password = Environment.GetEnvironmentVariable("NEON_DB_PASSWORD");

    var connectionString = $"Host={host};Database={dbName};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// ── CORS ──
builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p =>
    p.WithOrigins(
    "http://localhost:3000",
    "https://onboarding-hp91.onrender.com")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// ── Dummy auth ──
// Register the default TimeProvider in DI
builder.Services.AddSingleton(TimeProvider.System);
builder.Services
    .AddAuthentication("CookieAuth")
    .AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>("CookieAuth", _ => { });

// ── Azure Blob Storage──
builder.Services.AddSingleton<AzureBlobService>();


// ── Document + AI services ──
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<PdfConverterService>();

builder.Services.AddScoped<PdfTextExtractorService>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<OpenAiClientService>();

builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddScoped<ChatHistoryService>();


var app = builder.Build();

// ── Build index and seed users ──
using (var scope = app.Services.CreateScope())
{
    // ✅ Seed admin + users
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    var users = new List<AppUser>
    {
        new AppUser
        {
            Email = "admin@byu.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            Department = "IT"
        },
        new AppUser
        {
            Email = "employee@byu.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
            Role = "FullTime",
            Department = "IS"
        },
        new AppUser
        {
            Email = "student@byu.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
            Role = "Student",
            Department = "IS"
        }
    };

    foreach (var user in users)
    {
        if (!db.Users.Any(u => u.Email == user.Email))
        {
            db.Users.Add(user);
            Console.WriteLine($"✅ Seeded {user.Role} - {user.Email}");
        }
    }

    db.SaveChanges();
}



// ── Middleware ──
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // 👈 shows full error
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Unexpected server error.\"}");
        });
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
