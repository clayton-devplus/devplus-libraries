namespace Devplus.Security.OAuth.Contracts;

public class CreateClientAppResponseDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? NomeCompleto { get; set; }
}