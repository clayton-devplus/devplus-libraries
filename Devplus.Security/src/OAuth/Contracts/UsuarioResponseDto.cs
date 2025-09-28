namespace Devplus.Security.OAuth.Contracts;

public class UsuarioResponseDto
{
    public long Id { get; set; }
    public string? Nome { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Domain { get; set; }
    public bool Ativada { get; set; }
    public DateTime? Validade { get; set; }
}