using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambda;

public class Function
{
    public string FunctionHandler(Hello request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Hello {request.World}");

        return System.Text.Json.JsonSerializer.Serialize(request);
    }
}

public class Hello
{
    public required string World { get; set; }
}