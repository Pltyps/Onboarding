using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Data;
using MOAI.API.Models;
using MOAI.API.Services;
using DotNetEnv;

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

// ── SQLite DB ──
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("StorageConnection")));

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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Unexpected server error.\"}");
    });
});


app.Use(async (ctx, next) =>
{
    var email = ctx.Request.Cookies["user_email"];
    var role = ctx.Request.Cookies["user_role"];
    var dept = ctx.Request.Cookies["user_department"];

    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(role))
    {
        ActiveUserTracker.Track(email);
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("Department", dept ?? "")
        }, "CookieAuth"));
    }

    await next();
});

app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
