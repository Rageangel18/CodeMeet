using CodeMeet.DAL.Enums;

public class OrganizationUser
{
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }
    public OrgRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }

    public Organization Org { get; set; } = null!;
    public User User { get; set; } = null!;
}