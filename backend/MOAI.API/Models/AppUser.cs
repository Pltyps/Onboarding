namespace MOAI.API.Models;

/// <summary>
/// Represents a user account for internal login (until SSO is added).
/// </summary>
public class AppUser
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password (BCrypt)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User role (Admin or Viewer).
    /// 🔧 Expandable to Editor, Reviewer, etc.
    /// </summary>
    public string Role { get; set; } = "Viewer"; // 🔧 Valid values: "Admin", "Viewer"
}
