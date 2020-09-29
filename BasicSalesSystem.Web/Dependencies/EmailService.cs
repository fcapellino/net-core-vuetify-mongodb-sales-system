namespace BasicSalesSystem.Web.Dependencies.EmailService
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class EmailService : IDisposable
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string emailTo, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var sender = _configuration["EmailSettings:SmtpUser"];
            var senderPassword = _configuration["EmailSettings:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

            var mailMessage = new MailMessage() { From = new MailAddress(sender) };
            mailMessage.To.Add(new MailAddress(emailTo));
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(sender, senderPassword),
                EnableSsl = enableSsl
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        public void Dispose()
        {
            // method intentionally left empty.
        }
    }
}
