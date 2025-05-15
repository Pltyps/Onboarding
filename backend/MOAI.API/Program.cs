using MOAI.API.Data;
using MOAI.API.Services;
using MOAI.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------
// ✅ Add core services
// -------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------------
// ✅ Database setup (SQLite for now)
// 🔧 Change connection string when migrating to SQL Server
// -------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=moai.db"));

// -------------------------------------
// ✅ Dependency injection
// -------------------------------------
builder.Services.AddScoped<IDocumentService, DocumentService>();

var app = builder.Build();

// -------------------------------------
// ✅ Enable Swagger (dev only)
// -------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------------
// ✅ Auto-create DB and seed admin user
// 🔐 Replace with SSO identity provider later
// -------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    // Seed default admin user if none exist
    if (!db.Users.Any())
    {
        var admin = new AppUser
        {
            Email = "admin@byu.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("moaiadmin123"), // 🔧 Replace for prod
            Role = "Admin"
        };

        db.Users.Add(admin);
        db.SaveChanges(); // ✅ Changed from await SaveChangesAsync to sync call inside sync method
    }
}

// -------------------------------------
// ✅ Middleware
// -------------------------------------
app.UseHttpsRedirection();
app.UseAuthorization(); // 🔧 Add UseAuthentication() when JWT/SSO is enabled
app.MapControllers();

app.Run();
