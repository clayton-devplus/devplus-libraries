namespace Devplus.Mail.Models;
/// <summary>
/// Representa um anexo de e‑mail para o SendGrid.
/// </summary>
public class EmailAttachment
{
    /// <summary>Nome do arquivo (com extensão).</summary>
    public string FileName { get; set; } = default!;
    /// <summary>Mime type, ex: "application/pdf", "image/png".</summary>
    public string MimeType { get; set; } = "application/octet-stream";
    /// <summary>Stream contendo os bytes do arquivo.</summary>
    public Stream ContentStream { get; set; } = default!;
    /// <summary>Normalmente "attachment".</summary>
    public string? Disposition { get; set; }
    /// <summary>Se quiser referenciar inline.</summary>
    public string? ContentId { get; set; }
}