using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

// Create clients for each service
var dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USEast1); // Change to your region
var s3Client = new AmazonS3Client(RegionEndpoint.USEast1); // Change to your region
var secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.USEast1); // Change to your region
var lambdaClient = new AmazonLambdaClient(RegionEndpoint.USEast1); // Change to your region
var snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1); // Change to your region
var sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1); // Change to your region

// Get resources
await GetSqsResources(sqsClient);
await GetSnsResources(snsClient);
await GetSnsSubscriptions(snsClient);
await GetDynamoDbResources(dynamoDbClient);
await GetS3Resources(s3Client);
await GetSecretsManagerResources(secretsManagerClient);
await GetLambdaResources(lambdaClient);

return;


static async Task GetSqsResources(AmazonSQSClient client)
{
    try
    {
        var queues = new List<Dictionary<string, object>>();

        var listQueuesResponse = await client.ListQueuesAsync(new ListQueuesRequest());
        foreach (var queueUrl in listQueuesResponse.QueueUrls)
        {
            var attributesResponse = await client.GetQueueAttributesAsync(queueUrl, new List<string> { "All" });
            var queueDetails = new Dictionary<string, object>
            {
                { "QueueUrl", queueUrl },
                { "Attributes", attributesResponse.Attributes }
            };
            queues.Add(queueDetails);
        }

        string jsonOutput = JsonSerializer.Serialize(queues, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("sqs-queues-backup.json", jsonOutput);
        Console.WriteLine($"SQS backup created successfully at: sqs-queues-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving SQS resources: {ex.Message}");
    }
}

static async Task GetSnsResources(AmazonSimpleNotificationServiceClient client)
{
    try
    {
        var topics = new List<Dictionary<string, object>>();

        string nextToken = null;
        do
        {
            var listTopicsResponse = await client.ListTopicsAsync(new ListTopicsRequest
            {
                NextToken = nextToken
            });

            foreach (var topic in listTopicsResponse.Topics)
            {
                var topicAttributesResponse = await client.GetTopicAttributesAsync(topic.TopicArn);
                var topicDetails = new Dictionary<string, object>
                {
                    { "TopicArn", topic.TopicArn },
                    { "Attributes", topicAttributesResponse.Attributes }
                };
                topics.Add(topicDetails);
            }

            nextToken = listTopicsResponse.NextToken;
        } while (nextToken != null);

        string jsonOutput = JsonSerializer.Serialize(topics, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("sns-topics-backup.json", jsonOutput);
        Console.WriteLine($"SNS backup created successfully at: sns-topics-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving SNS resources: {ex.Message}");
    }
}

static async Task GetSnsSubscriptions(AmazonSimpleNotificationServiceClient client)
{
    try
    {
        var subscriptions = new List<Dictionary<string, object>>();

        string nextToken = null;
        do
        {
            var listSubscriptionsResponse = await client.ListSubscriptionsAsync(new ListSubscriptionsRequest
            {
                NextToken = nextToken
            });

            foreach (var subscription in listSubscriptionsResponse.Subscriptions)
            {
                var subscriptionAttributesResponse =
                    await client.GetSubscriptionAttributesAsync(subscription.SubscriptionArn);
                var subscriptionDetails = new Dictionary<string, object>
                {
                    { "SubscriptionArn", subscription.SubscriptionArn },
                    { "TopicArn", subscription.TopicArn },
                    { "Protocol", subscription.Protocol },
                    { "Endpoint", subscription.Endpoint },
                    { "Attributes", subscriptionAttributesResponse.Attributes }
                };
                subscriptions.Add(subscriptionDetails);
            }

            nextToken = listSubscriptionsResponse.NextToken;
        } while (nextToken != null);

        string jsonOutput = JsonSerializer.Serialize(subscriptions, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("sns-subscriptions-backup.json", jsonOutput);
        Console.WriteLine($"SNS Subscriptions backup created successfully at: sns-subscriptions-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving SNS subscriptions: {ex.Message}");
    }
}


static async Task GetDynamoDbResources(AmazonDynamoDBClient client)
{
    try
    {
        var tables = new List<Dictionary<string, object>>();

        var listTablesResponse = await client.ListTablesAsync();
        foreach (var tableName in listTablesResponse.TableNames)
        {
            var describeTableResponse = await client.DescribeTableAsync(tableName);
            var tableDetails = new Dictionary<string, object>
            {
                { "TableName", describeTableResponse.Table.TableName },
                { "TableArn", describeTableResponse.Table.TableArn },
                { "ItemCount", describeTableResponse.Table.ItemCount },
                { "ProvisionedThroughput", describeTableResponse.Table.ProvisionedThroughput },
                { "CreationDateTime", describeTableResponse.Table.CreationDateTime },
                { "TableStatus", describeTableResponse.Table.TableStatus },
                { "KeySchema", describeTableResponse.Table.KeySchema },
                { "GlobalSecondaryIndexes", describeTableResponse.Table.GlobalSecondaryIndexes },
                { "LocalSecondaryIndexes", describeTableResponse.Table.LocalSecondaryIndexes },
                { "BillingModeSummary", describeTableResponse.Table.BillingModeSummary },
                { "SSEDescription", describeTableResponse.Table.SSEDescription }
            };

            tables.Add(tableDetails);
        }

        string jsonOutput = JsonSerializer.Serialize(tables, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("dynamodb-tables-backup.json", jsonOutput);
        Console.WriteLine($"DynamoDB backup created successfully at: dynamodb-tables-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving DynamoDB resources: {ex.Message}");
    }
}

static async Task GetS3Resources(AmazonS3Client client)
{
    try
    {
        var buckets = new List<Dictionary<string, object>>();

        var listBucketsResponse = await client.ListBucketsAsync();
        foreach (var bucket in listBucketsResponse.Buckets)
        {
            var bucketDetails = new Dictionary<string, object>
            {
                { "BucketName", bucket.BucketName },
                { "CreationDate", bucket.CreationDate },
                { "Location", await client.GetBucketLocationAsync(bucket.BucketName) },
                { "VersioningStatus", await GetVersioningStatus(client, bucket.BucketName) },
                { "Policy", await GetBucketPolicy(client, bucket.BucketName) }
            };

            buckets.Add(bucketDetails);
        }

        string jsonOutput = JsonSerializer.Serialize(buckets, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("s3-buckets-backup.json", jsonOutput);
        Console.WriteLine($"S3 backup created successfully at: s3-buckets-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving S3 resources: {ex.Message}");
    }
}

static async Task<string> GetVersioningStatus(AmazonS3Client client, string bucketName)
{
    try
    {
        var versioningResponse = await client.GetBucketVersioningAsync(bucketName);
        return versioningResponse.ToString() ?? "Not Enabled";
    }
    catch
    {
        return "Error Retrieving Versioning Status";
    }
}

static async Task<string> GetBucketPolicy(AmazonS3Client client, string bucketName)
{
    try
    {
        var policyResponse = await client.GetBucketPolicyAsync(bucketName);
        return policyResponse.Policy;
    }
    catch
    {
        return "Error Retrieving Bucket Policy";
    }
}

static async Task GetSecretsManagerResources(AmazonSecretsManagerClient client)
{
    try
    {
        var secrets = new List<Dictionary<string, object>>();

        string nextToken = null;
        do
        {
            var listSecretsResponse = await client.ListSecretsAsync(new ListSecretsRequest
            {
                NextToken = nextToken
            });

            foreach (var secret in listSecretsResponse.SecretList)
            {
                var secretDetails = new Dictionary<string, object>
                {
                    { "Name", secret.Name },
                    { "ARN", secret.ARN },
                    { "Description", secret.Description },
                    { "LastChangedDate", secret.LastChangedDate },
                    { "KmsKeyId", secret.KmsKeyId },
                    // { "SecretType", secret.SecretType }
                };
                secrets.Add(secretDetails);
            }

            nextToken = listSecretsResponse.NextToken;
        } while (nextToken != null);

        string jsonOutput = JsonSerializer.Serialize(secrets, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("secrets-manager-backup.json", jsonOutput);
        Console.WriteLine($"Secrets Manager backup created successfully at: secrets-manager-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving Secrets Manager resources: {ex.Message}");
    }
}

static async Task GetLambdaResources(AmazonLambdaClient client)
{
    try
    {
        var functions = new List<Dictionary<string, object>>();

        string nextMarker = null;
        do
        {
            var listFunctionsResponse = await client.ListFunctionsAsync(new ListFunctionsRequest
            {
                Marker = nextMarker
            });

            foreach (var function in listFunctionsResponse.Functions)
            {
                var functionDetails = new Dictionary<string, object>
                {
                    { "FunctionName", function.FunctionName },
                    { "FunctionArn", function.FunctionArn },
                    { "Runtime", function.Runtime },
                    { "LastModified", function.LastModified },
                    { "Handler", function.Handler },
                    { "MemorySize", function.MemorySize },
                    { "Timeout", function.Timeout },
                    { "Role", function.Role },
                    { "EnvironmentVariables", function.Environment },
                    { "DeadLetterConfig", function.DeadLetterConfig },
                    { "TracingConfig", function.TracingConfig },
                    { "VpcConfig", function.VpcConfig }
                };
                functions.Add(functionDetails);
            }

            nextMarker = listFunctionsResponse.NextMarker;
        } while (nextMarker != null);

        string jsonOutput = JsonSerializer.Serialize(functions, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("lambda-functions-backup.json", jsonOutput);
        Console.WriteLine($"Lambda backup created successfully at: lambda-functions-backup.json");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving Lambda resources: {ex.Message}");
    }
}
