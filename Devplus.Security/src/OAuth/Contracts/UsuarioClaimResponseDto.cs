namespace Devplus.Security.OAuth.Contracts;

public class UsuarioClaimResponseDto
{
    public Guid Id { get; set; }
    public long UsuarioId { get; set; }
    public Guid SystemClaimId { get; set; }
    public SystemClaimResponseDto? SystemClaim { get; set; }
    public Guid SistemaId { get; set; }
    public string? Value { get; set; }
}
