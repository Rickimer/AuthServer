namespace MessagesQueueService.Shared
{
    public class EmailMessageMQ
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string? From { get; set; }
        public string Message { get; set; } = string.Empty;
        public EmailType emailType { get; set; }
    }
}
