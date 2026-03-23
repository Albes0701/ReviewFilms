namespace ReviewFilms.Api.Entities;

public class UserRole
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public Guid? AssignedByUserId { get; set; }

    public DateTime AssignedAt { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;

    public User? AssignedByUser { get; set; }
}
