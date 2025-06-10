using Microsoft.EntityFrameworkCore;
using MOAI.API.Models;

namespace MOAI.API.Data;

/// <summary>
/// EF Core context class for database access.
/// Holds the Documents table (and any future tables).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Represents the table for uploaded documents.
    /// </summary>
    public DbSet<StoredDocument> Documents => Set<StoredDocument>();
    public DbSet<AppUser> Users => Set<AppUser>();
}
