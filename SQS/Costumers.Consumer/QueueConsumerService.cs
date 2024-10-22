using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Costumers.Consumer.Messages;
using MediatR;
using Microsoft.Extensions.Options;

namespace Costumers.Consumer;

public class QueueConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IMediator _mediator;
    private readonly IOptions<QueueSettings> _queueSettings;
    private readonly ILogger<QueueConsumerService> _logger;
    
    public QueueConsumerService(IAmazonSQS sqsClient, IMediator mediator, IOptions<QueueSettings> queueSettings, ILogger<QueueConsumerService> logger)
    {
        _sqsClient = sqsClient;
        _mediator = mediator;
        _queueSettings = queueSettings;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(_queueSettings.Value.Name, stoppingToken);

        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            MessageSystemAttributeNames = ["All"],
            MessageAttributeNames = ["All"],
            MaxNumberOfMessages = 1
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);
            foreach (var message in response.Messages)
            {
                var messageType = message.MessageAttributes["MessageType"].StringValue;
                var type = Type.GetType($"Costumers.Consumer.Messages.{messageType}");

                if (type is null)
                {
                    _logger.LogWarning($"Unknown message type: {messageType}");
                    continue;
                }
                
                var typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type)!;
                
                try
                {
                    await _mediator.Send(typedMessage, stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Message failed during processing");
                    continue;
                }
                
                await _sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, stoppingToken);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}

// SWITCH COULD BE A NICE OPTION
// BUT WILL BE BETTER AND CLEAN USE THE MEDIATOR CLASS
// switch (messageType)
// {
//     case nameof(CustomerCreated):
//         var created = JsonSerializer.Deserialize<CustomerCreated>(message.Body);
//         break;
//     case nameof(CustomerUpdated):
//         var updated = JsonSerializer.Deserialize<CustomerUpdated>(message.Body);
//         break;
//     case nameof(CustomerDeleted):
//         var deleted = JsonSerializer.Deserialize<CustomerDeleted>(message.Body);
//         break;
// }