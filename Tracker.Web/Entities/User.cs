namespace Tracker.Web.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    /// <summary>
    /// Whether this user can access the Consolidation feature
    /// </summary>
    public bool CanConsolidate { get; set; } = false;

    // Navigation
    public virtual ICollection<UserServiceArea> ServiceAreas { get; set; } = new List<UserServiceArea>();
}

public enum UserRole
{
    User,
    Reporting,
    SuperAdmin
}
