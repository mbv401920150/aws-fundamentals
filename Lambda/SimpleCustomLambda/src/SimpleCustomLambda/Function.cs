using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace SimpleCustomLambda;

public class Function
{
    private static async Task Main(string[] args)
    {
        Func<ILambdaContext, string> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync();
    }

    public static string FunctionHandler(ILambdaContext context)
    {
        return "Lambda .Net 9 - Running test";
    }
}