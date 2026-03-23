namespace ReviewFilms.Api.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;

    public User? CreatedByUser { get; set; }
}
