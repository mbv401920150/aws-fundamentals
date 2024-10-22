using System.Text.Json;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using SqsCommon;

// POSSIBLE ERRORS
// - Check the region of your Queue
// - Check the name of your Queue

var sqsClient = new AmazonSQSClient(
    new AmazonSQSConfig
    {
        RegionEndpoint = RegionEndpoint.USEast1
    });

// Contract Type
var customer = new CustomerCreated()
{
    Id = Guid.NewGuid(),
    Email = "mibol@gmail.com",
    FullName = "Michael Bolanos",
    DateOfBirth = new DateTime(1986, 08, 30),
    GitHubUser = "mibolhub"
};


// The Queue URL you can found it in the details in AWS SQS Section > Your Queue
// var url = "https://sqs.us-east-1.amazonaws.com/842676019507/customers";

// But this information may vary depending on the region, user, etc.
// The best way to grab this data is from the client

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customers");
var queueUrl = queueUrlResponse.QueueUrl;

var sendMessageRequest = new SendMessageRequest
{
    QueueUrl = queueUrl,
    MessageBody = JsonSerializer.Serialize(customer),
    MessageAttributes = new Dictionary<string, MessageAttributeValue>
    {
        {
            "MessageType", new MessageAttributeValue
            {
                DataType = "String",
                StringValue = nameof(CustomerCreated)
            }
        }
    },
    DelaySeconds = 10
};

var response = await sqsClient.SendMessageAsync(sendMessageRequest);

Console.WriteLine($"Done sending message to queue: {response.HttpStatusCode}");