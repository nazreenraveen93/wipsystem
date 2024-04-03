// EmailService.cs
using MailKit.Net.Smtp;
using MimeKit;

namespace WIPSystem.Web.Services
{
    public interface IEmailService
    {
        void SendLifeSpanAlertEmail(string recipientEmail, string jigOrMoldName);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SMTPServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:SMTPPort"]);
            _smtpUsername = configuration["EmailSettings:SMTPUsername"];
            _smtpPassword = configuration["EmailSettings:SMTPPassword"];
        }

        public void SendLifeSpanAlertEmail(string recipientEmail, string jigOrMoldName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MES System Jig Status", "ADMIN_NGKMY@domino.ngked.co.jp"));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = $"Alert: Life Span Issue for {jigOrMoldName}";

            message.Body = new TextPart("plain")
            {
                Text = $"The life span for {jigOrMoldName} is either exceeded or low. Please take necessary action."
            };

            using (var client = new SmtpClient())
            {
                client.Connect(_smtpServer, _smtpPort, false);
                client.Authenticate(_smtpUsername, _smtpPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
