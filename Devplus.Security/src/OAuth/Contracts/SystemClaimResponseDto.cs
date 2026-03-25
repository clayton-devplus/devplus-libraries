namespace Devplus.Security.OAuth.Contracts;

public class SystemClaimResponseDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = null!;
    public string? Description { get; set; }
}
