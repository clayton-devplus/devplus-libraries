namespace Devplus.Security.OAuth.Contracts;

public class UserLoginDto
{
    public string? NomeUsuario { get; set; }
    public string? Senha { get; set; }

    // Modo SaaS: quando o usuário tem o mesmo sistema em vários tenants, o primeiro login volta
    // com a lista de tenants (409). O app reenvia o login com o TenantId escolhido para concluir.
    public Guid? TenantId { get; set; }
}
