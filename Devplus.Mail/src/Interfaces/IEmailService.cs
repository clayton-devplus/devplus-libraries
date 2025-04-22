using Devplus.Mail.Enum;
using Devplus.Mail.Models;

namespace Devplus.Mail.Interfaces;
public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent,
                                                IEnumerable<EmailAttachment>? attachments = null);
    Task<bool> SendTemplateEmail(string toEmail, string subject, string message, string link = "",
                                                EmailTemplateType templateType = EmailTemplateType.Success,
                                                IEnumerable<EmailAttachment>? attachments = null);
}
