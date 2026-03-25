namespace Devplus.Security.OAuth.Contracts;

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid SystemId { get; set; }
    public bool Active { get; set; }
}
