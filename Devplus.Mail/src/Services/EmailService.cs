using System.Reflection;
using Devplus.Mail.Configuration;
using Devplus.Mail.Enum;
using Devplus.Mail.Interfaces;
using Devplus.Mail.Models;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Devplus.Mail.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SendGridConfig _sendGridConfig;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _sendGridConfig = _configuration.GetSection("SendGrid").Get<SendGridConfig>() ?? throw new ArgumentNullException(nameof(_sendGridConfig), "SendGrid configuration section is missing.");
    }
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent,
                                            IEnumerable<EmailAttachment>? attachments = null)
    {
        if (string.IsNullOrEmpty(toEmail))
            throw new ArgumentException("Recipient email address is required.", nameof(toEmail));
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentException("Email subject is required.", nameof(subject));
        if (string.IsNullOrEmpty(plainTextContent) && string.IsNullOrEmpty(htmlContent))
            throw new ArgumentException("At least one content type (plain text or HTML) is required.");

        var client = new SendGridClient(_sendGridConfig.ApiKey);
        var from = new EmailAddress(_sendGridConfig.FromEmail, _sendGridConfig.FromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                var bytes = attachment.ContentStream is MemoryStream ms
                    ? ms.ToArray()
                    : ReadFully(attachment.ContentStream);
                var base64 = Convert.ToBase64String(bytes);

                msg.AddAttachment(
                    attachment.FileName,
                    base64,
                    attachment.MimeType,
                    attachment.Disposition ?? "attachment",
                    attachment.ContentId);
            }
        }

        var response = await client.SendEmailAsync(msg);

        return response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted;
    }
    public async Task<bool> SendTemplateEmail(string toEmail, string subject, string message, string link = "",
                                                EmailTemplateType templateType = EmailTemplateType.Success,
                                                IEnumerable<EmailAttachment>? attachments = null)
    {
        if (string.IsNullOrEmpty(toEmail))
            throw new ArgumentException("Recipient email address is required.", nameof(toEmail));
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentException("Email subject is required.", nameof(subject));
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("At least one content type (plain text or HTML) is required.");

        string template = string.Empty;

        switch (templateType)
        {
            case EmailTemplateType.Success:
                subject = "✅ Sucesso - " + subject;
                template = LoadTemplate("Success.html")
                    .Replace("{{titulo}}", subject)
                    .Replace("{{mensagem}}", message)
                    .Replace("{{link}}", link == "" ? "" : $"<a class=\"button\" href=\"{link}\" target=\"_blank\">Ver Detalhes</a>");
                break;
            case EmailTemplateType.Warning:
                subject = "⚠️ Atenção - " + subject;
                template = LoadTemplate("Warning.html")
                    .Replace("{{titulo}}", subject)
                    .Replace("{{mensagem}}", message)
                    .Replace("{{link}}", link == "" ? "" : $"<a class=\"button\" href=\"{link}\" target=\"_blank\">Ver Detalhes</a>");
                break;
            case EmailTemplateType.Error:
                subject = "❌ Erro - " + subject;
                template = LoadTemplate("Error.html")
                    .Replace("{{titulo}}", subject)
                    .Replace("{{mensagem}}", message)
                    .Replace("{{link}}", link == "" ? "" : $"<a class=\"button\" href=\"{link}\" target=\"_blank\">Ver Detalhes</a>");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null);
        }

        var client = new SendGridClient(_sendGridConfig.ApiKey);
        var from = new EmailAddress(_sendGridConfig.FromEmail, _sendGridConfig.FromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", template);

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                var bytes = attachment.ContentStream is MemoryStream ms
                    ? ms.ToArray()
                    : ReadFully(attachment.ContentStream);
                var base64 = Convert.ToBase64String(bytes);

                msg.AddAttachment(
                    attachment.FileName,
                    base64,
                    attachment.MimeType,
                    attachment.Disposition ?? "attachment",
                    attachment.ContentId);
            }
        }

        var response = await client.SendEmailAsync(msg);

        return response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted;
    }
    private static string LoadTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith($"Templates.{templateName}", StringComparison.InvariantCultureIgnoreCase));

        if (resourceName == null)
            throw new InvalidOperationException($"Template '{templateName}' não encontrado como recurso embutido.");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    private static byte[] ReadFully(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }


}