using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace medical.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");

                // Validate configuration
                if (string.IsNullOrEmpty(smtpSettings["SmtpServer"]) ||
                    string.IsNullOrEmpty(smtpSettings["FromEmail"]))
                {
                    throw new ApplicationException("SMTP configuration is incomplete");
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    smtpSettings["FromName"] ?? "Medical App",
                    smtpSettings["FromEmail"]));

                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Password Reset Request";
                message.Body = new TextPart("html")
                {
                    Text = $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>"
                };

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    smtpSettings["SmtpServer"],
                    int.Parse(smtpSettings["SmtpPort"]),
                    MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    smtpSettings["SmtpUsername"],
                    smtpSettings["SmtpPassword"]);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Email sending failed: {ex}");
                throw; // Re-throw to be handled by the controller
            }
        }
    }
}
