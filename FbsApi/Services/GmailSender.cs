using System.Net;
using System.Net.Mail;
using FbsApi.Services.Interfaces;
using FbsApi.Settings;
using Microsoft.Extensions.Options;

namespace FbsApi.Services
{
    public class GmailSender : IEmailSender
    {
        private EmailSenderOptions Options { get; }
        public GmailSender(IOptions<EmailSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(Options.GmailSenderEmail, Options.GmailSenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            using (var client = new SmtpClient(Options.SmtpServer, Options.SmtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(Options.GmailSenderEmail, Options.GmailAppPassword);

                try
                {
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("Email sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while sending email: {ex}");
                }
            }
        }
    }
}
