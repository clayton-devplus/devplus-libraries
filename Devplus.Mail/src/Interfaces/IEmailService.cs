using Devplus.Mail.Enum;

namespace Devplus.Mail.Interfaces;
public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent);
    Task<bool> SendTemplateEmail(string toEmail, string subject, string message, string link = "",
                                                EmailTemplateType templateType = EmailTemplateType.Success);
}
