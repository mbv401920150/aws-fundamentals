using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Customers.Api.Services;

public class CustomerImageService : ICustomerImageService
{
    private readonly IAmazonS3 _s3;
    private readonly IOptions<S3Settings> _s3Settings;
    private const string S3ImageFolder = "images";
    public CustomerImageService(IAmazonS3 s3, IOptions<S3Settings> s3Settings)
    {
        _s3 = s3;
        _s3Settings = s3Settings;
    }
    
    public async Task<PutObjectResponse> UploadImageAsync(Guid id, IFormFile file)
    {
        var putObjectRequest = new PutObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{S3ImageFolder}/{id}",
            ContentType = file.ContentType,
            InputStream = file.OpenReadStream(),
            Metadata =
            {
                ["x-amz-meta-originalname"] = file.FileName,
                ["x-amz-meta-extension"] = Path.GetExtension(file.FileName)
            }
        };
        
        return await _s3.PutObjectAsync(putObjectRequest);
    }

    public async Task<GetObjectResponse> GetImageAsync(Guid id)
    {
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{S3ImageFolder}/{id}",
        };
        
        return await _s3.GetObjectAsync(getObjectRequest);
    }

    public async Task<DeleteObjectResponse> DeleteImageAsync(Guid id)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{S3ImageFolder}/{id}",
        };
        
        return await _s3.DeleteObjectAsync(deleteObjectRequest);
    }
}