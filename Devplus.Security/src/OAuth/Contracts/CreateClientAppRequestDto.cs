namespace Devplus.Security.OAuth.Contracts;

public class CreateClientAppRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string? NomeCompleto { get; set; }
}