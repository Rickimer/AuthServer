namespace MessagesQueueService.Shared
{
    public interface IMQServices
    {
        Task SendActivationEmailAsync(SendActivationDto sendActivationDto, CancellationToken cancellationToken = default);
        //Task SendEmailAsync(EmailMessageMQ message, CancellationToken cancellationToken = default);
    }
}
