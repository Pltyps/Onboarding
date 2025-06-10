using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using MOAI.API.Data;
using MOAI.API.Models;
using MOAI.API.Services;

var builder = WebApplication.CreateBuilder(args);


// -------------------------------------
// ✅ Add core services
// -------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// -------------------------------------
// ✅ Database setup (SQLite for now)
// -------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StorageConnection")));

// -------------------------------------
// ✅ CORS configuration for frontend (dev only)
// -------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// -------------------------------------
// ✅ Dependency injection
// -------------------------------------
builder.Services.AddScoped<IDocumentService, DocumentService>();


// -------------------------------------
// ✅ Authentication setup
// -------------------------------------
builder.Services.AddAuthentication("CookieAuth").AddScheme<AuthenticationSchemeOptions, DummyAuthHandler>("CookieAuth", options => { });


// -------------------------------------
// ✅ Set custom dev port
// -------------------------------------
builder.WebHost.UseUrls("https://localhost:5000");

var app = builder.Build();

// -------------------------------------
// ✅ Enable Swagger only in dev
// -------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------------
// ✅ Auto-seed admin + users
// -------------------------------------
using (var scope = app.Services.CreateScope())
{
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

// -------------------------------------
// ✅ Middleware pipeline
// -------------------------------------
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseRouting();

app.Use(async (context, next) =>
{
    var email = context.Request.Cookies["user_email"];
    var role = context.Request.Cookies["user_role"];
    var dept = context.Request.Cookies["user_department"];

    if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(role))
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("Department", dept ?? "Unknown")
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        context.User = new ClaimsPrincipal(identity);
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
