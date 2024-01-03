using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace BlazorGPT.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("prepAIdopen", "support@prepaidopen.com"));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.webio.pl", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("support@prepaidopen.com", "Kaz3a/8kaz3a/8");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
