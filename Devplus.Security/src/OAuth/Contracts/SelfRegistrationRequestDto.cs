namespace Devplus.Security.OAuth.Contracts;

public class SelfRegistrationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid SystemId { get; set; }

    // Dados do Tenant
    public string? TenantName { get; set; }
    public string? Cnpj { get; set; }
    public string? Cpf { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}
