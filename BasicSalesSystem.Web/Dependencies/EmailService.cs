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
            var smtpFrom = _configuration["EmailSettings:SmtpFrom"];
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

            var mailMessage = new MailMessage() { From = new MailAddress(smtpFrom) };
            mailMessage.To.Add(new MailAddress(emailTo));
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = enableSsl,
                Timeout = Convert.ToInt32(TimeSpan.FromSeconds(30).TotalMilliseconds)
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        public void Dispose()
        {
            // method intentionally left empty.
        }
    }
}
