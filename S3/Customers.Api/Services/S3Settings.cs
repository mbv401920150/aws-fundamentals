namespace Customers.Api.Services;

public class S3Settings
{
    public const string Key = "S3";
    
    public required string BucketName {get;set;}
}