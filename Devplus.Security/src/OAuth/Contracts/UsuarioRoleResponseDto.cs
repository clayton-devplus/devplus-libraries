namespace Devplus.Security.OAuth.Contracts;

public class UsuarioRoleResponseDto
{
    public Guid Id { get; set; }
    public long UsuarioId { get; set; }
    public Guid RoleId { get; set; }
    public RoleResponseDto? Role { get; set; }
    public Guid SistemaId { get; set; }
}
