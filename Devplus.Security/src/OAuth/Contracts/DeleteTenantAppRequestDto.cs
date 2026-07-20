namespace Devplus.Security.OAuth.Contracts;

/// <summary>
/// Exclusão de conta: apaga o client_app do tenant (no sistema desta app) + associações de usuário +
/// contas órfãs no IdP. O tenant é mantido.
/// </summary>
public class DeleteTenantAppRequestDto
{
    public Guid TenantId { get; set; }

    /// <summary>E-mail de quem disparou a exclusão (para a auditoria do IdP registrar a pessoa). Opcional.</summary>
    public string? ActorEmail { get; set; }
}
