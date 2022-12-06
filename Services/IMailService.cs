namespace AuthServer.Services
{
    public interface IMailService
    {
        Task SendActivationMail(string to, string subject, string link);
    }
}
