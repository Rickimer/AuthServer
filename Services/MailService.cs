using MimeKit;
using MailKit.Net.Smtp;

namespace AuthServer.Services
{
    public class MailService : IMailService
    {
        public async Task SendActivationMail(string to, string subject, string link)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Site administration", "RickimerSite@yandex.ru"));
            emailMessage.To.Add(new MailboxAddress(to, to));
            emailMessage.Subject = subject;
            
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = "Follow the <b>link</b> to activate <a href=\"" + link + "\">Confirm email</a>"                
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.ru", 465, true);
                await client.AuthenticateAsync("RickimerSite@yandex.ru", "xwdwlegqonkvleom");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
