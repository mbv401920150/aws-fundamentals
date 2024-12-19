using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

var secretsManagerClient = new AmazonSecretsManagerClient();
const string secretKeyId = "ApiKey";

// The example assume that you have a Secret Key with at least 3 versions (3 changes over the time)

// You can include the version stage with two possible values:
// - "AWSCURRENT"  = Current version
// - "AWSPREVIOUS" = Previous version
foreach (var versionStage in new[] { "AWSCURRENT", "AWSPREVIOUS" })
{
    var request = new GetSecretValueRequest()
    {
        SecretId = secretKeyId,
        VersionStage = versionStage
    };

    var response = await secretsManagerClient.GetSecretValueAsync(request);

    Console.WriteLine($"ApiKey (Version: {versionStage}) is: '{response.SecretString}'");    
}

// You can get additional details without retrieve the secret itself
var describeSecretRequest = new DescribeSecretRequest
{
    SecretId = secretKeyId
};

var describeSecretResponse = await secretsManagerClient.DescribeSecretAsync(describeSecretRequest);

Console.WriteLine($"The key was created at: {describeSecretResponse.CreatedDate} | Rotation is enabled?: {describeSecretResponse.RotationEnabled} | Next Rotation date: {describeSecretResponse.NextRotationDate}");

// GETTING ALL VERSIONS OF THE SAME KEY:
// Assuming you have more than 3 versions of the same key you can get all versions on this way:
// - Index 0: The First version (Already deprecated)
// - Index 1: Previous version
// - Index 2: Current version
var listSecretVersionsRequest = new ListSecretVersionIdsRequest
{
    SecretId = secretKeyId,
    IncludeDeprecated = true
};

var listSecretVersionIdsResponse = await secretsManagerClient.ListSecretVersionIdsAsync(listSecretVersionsRequest);

var deprecatedRequest = new GetSecretValueRequest()
{
    SecretId = secretKeyId,
    VersionId = listSecretVersionIdsResponse.Versions.First().VersionId
};

var deprecatedResponse = await secretsManagerClient.GetSecretValueAsync(deprecatedRequest);

Console.WriteLine($"Deprecated ApiKey is '{deprecatedResponse.SecretString}'");