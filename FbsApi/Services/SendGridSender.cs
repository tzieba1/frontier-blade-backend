using FbsApi.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FbsApi.Services
{
    public class SendGridSender : IEmailSender
    {
        public SendGridSender(IOptions<EmailSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public EmailSenderOptions Options { get; } // Set only via Secret Manager

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                Console.WriteLine("API Key: " + Options.SendGridKey); // Confirm API Key is present
                Console.WriteLine("Sending email to: " + email);

                var result = await Execute(Options.SendGridKey, subject, message, email);

                // Check response status
                if (result.IsSuccessStatusCode)
                {
                    Console.WriteLine("Email sent successfully.");
                    Console.WriteLine($"result: {result.Body}");
                }
                else
                {
                    Console.WriteLine($"Failed to send email. Status Code: {result.StatusCode}");
                    var responseBody = await result.Body.ReadAsStringAsync();
                    Console.WriteLine($"Response Body: {responseBody}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while sending email: {e}");
            }
        }

        private async Task<Response> Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(Options.SendGridSenderEmail, Options.SendGridSenderName),
                Subject = subject,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking as per SendGrid documentation
            msg.SetClickTracking(false, false);
            msg.AddCustomArg("UniqueID", Guid.NewGuid().ToString());

            try
            {
                var response = await client.SendEmailAsync(msg);
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while executing SendGrid API: {e}");
                throw; // Rethrow to be handled in SendEmailAsync
            }
        }

        /// <summary>
    /// Sends the 2FA code to the specified email address.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="code">The 2FA code.</param>
    public async Task Send2FACodeAsync(string email, string code)
    {
        var subject = "Temi: Your 2FA Code";
        string homepageUrl = "http://localhost:5173";
        var message = $@"
    <html>
        <body>
            <p style='font-size: 20px;'>Dear User,</p>
            <p style='font-size: 20px;'>Welcome !</p>
            <br/>
            <p style='font-size: 20px;'>Your 2 Factor Authentication Code is: <strong>{code}</strong></p>
            <br/>
            <p style='font-size: 20px;'>Thank you,</p>
            <p style='font-size: 20px;'>Temi  .</p>
            <p><a href={homepageUrl} style='font-size: 20px;'>  Temi  .</a></p>
        </body>
    </html>
";

        await SendEmailAsync(email, subject, message);
    }

    /// <summary>
    /// Sends the Password Reset Link to the specified email address.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="resetToken">The generated password reset token</param>
    public async Task SendPasswordResetLinkAsync(string email, string resetToken)
    {
        var subject = "Temi: Reset your password";
        string homepageUrl = "http://localhost:5173";
        var message = $@"
    <html>
        <body>
            <p style='font-size: 20px;'>Dear User,</p>
            <p style='font-size: 20px;'>Welcome !</p>
            <br/>
            <p style='font-size: 20px;'>Please reset your password by <a href='{homepageUrl}/auth/reset-password?token={resetToken}&email={email}'>clicking here</a>.</p>
            <br/>
            <p style='font-size: 20px;'>Thank you,</p>
            <p style='font-size: 20px;'>Temi </p>
            <p><a href={homepageUrl} style='font-size: 20px;'>Temi </a></p>
        </body>
    </html>
";

        await SendEmailAsync(email, subject, message);
    }
  }
}
