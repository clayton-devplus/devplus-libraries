using Devplus.Mail.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Devplus.Mail.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync()
    {
        var apiKey = "";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("clayton.oliveira@devplus.com.br", "Clayton Oliviera");
        var subject = "Sending with SendGrid is Fun";
        var to = new EmailAddress("clayton@devplus.com.br", "Example User");
        var plainTextContent = "and easy to do anywhere, even with C#";
        var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }
}