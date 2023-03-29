using MessagesQueueService.Shared;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagesQueueService.RabbitMQ
{
    public class RabbitMQService : IMQServices
    {
        private const string QUEUE_NAME = "emailworker";

        /*private readonly IConnection connection;
        private readonly IModel channel;*/
        private readonly string replyQueueName;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

        public RabbitMQService()
        {
        }

        //public IModel CreateChannel() //todel
        //{
        //var factory = new ConnectionFactory { HostName = "localhost" };
        /*var connectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
            ConsumerDispatchConcurrency = 2,
            DispatchConsumersAsync = true
        };

        var connection = connectionFactory.CreateConnection();
        var channel = connection.CreateModel();*/

        // declare a server-named queue
        /*replyQueueName = channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(consumer: consumer,
                             queue: replyQueueName,
                             autoAck: true);*/

        //return channel;
        //}

        public async Task SendActivationEmailAsync(SendActivationDto sendActivationDto, CancellationToken cancellationToken = default)
        {
            var message = new EmailMessageMQ
            {
                emailType = EmailType.ActivationEmail,
                ToEmail = sendActivationDto.ToEmail,
                Message = sendActivationDto.ConfirmUrl
            };

            //await SendEmailAsync(message, cancellationToken);
            await SendEmailAsync(message, cancellationToken);
        }

        public Task SendEmailAsync(EmailMessageMQ message, CancellationToken cancellationToken = default)
        //public void SendEmailAsync(EmailMessageMQ message, CancellationToken cancellationToken = default)
        {
            var connectionFactory = new ConnectionFactory
            {
                /*UserName = "ops1",
                Password = "ops1",*/
                HostName = "localhost",
                ConsumerDispatchConcurrency = 2,
                DispatchConsumersAsync = true
            };

            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: QUEUE_NAME,
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

            IBasicProperties props = channel.CreateBasicProperties();
            /*var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;*/
            props.ReplyTo = QUEUE_NAME;

            //var activationLink = Url.Action("ConfirmEmail", "User", new { item = result.PasswordHash, email = userDto.Email }, Request.Scheme);
            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            /*var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);*/

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: QUEUE_NAME,
                                 basicProperties: props,
                                 body: messageBytes);

            //cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
            //return tcs.Task;

            channel.Close();
            connection.Close();            
            return Task.CompletedTask;
        }

        /*public void Dispose()
        {
            channel.Close();
            connection.Close();
        }*/
    }
}
