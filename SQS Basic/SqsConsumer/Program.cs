using System.Text.Json;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using SqsCommon;

var cts = new CancellationTokenSource();
var sqsClient = new AmazonSQSClient(
    new AmazonSQSConfig
    {
        RegionEndpoint = RegionEndpoint.USEast1
    });
    
var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customers");

var receiveMessageRequest = new ReceiveMessageRequest
{
    QueueUrl = queueUrlResponse.QueueUrl,
    MessageAttributeNames = new List<string> {"All"},
    MessageSystemAttributeNames = ["All"]
};

while (!cts.IsCancellationRequested)
{
    var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts.Token);

    foreach (var message in response.Messages)
    {
        Console.WriteLine($"Message Id: {message.MessageId}");
        Console.WriteLine($"Message Body: {message.Body}");
        Console.WriteLine();

        var messageBody = JsonSerializer.Deserialize<CustomerCreated>(message.Body);
        await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle);
    }
    
    await Task.Delay(3000);
}