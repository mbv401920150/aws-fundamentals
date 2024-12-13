using Amazon.S3;
using Amazon.S3.Model;

const string bucketName = "mibol-cloud";

var s3Client = new AmazonS3Client();


async Task UploadFile(string filePath, string awsStoredPath, string contentType)
{
    // You can use ContentBody to set plain text
    // Or FilePath to specify the file in the machine
    // But the best option is use InputStream to save the file.
    await using var inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

    var putObjectRequest = new PutObjectRequest
    {
        BucketName = bucketName,
        Key = awsStoredPath,
        ContentType = contentType,
        InputStream = inputStream
    };

    await s3Client.PutObjectAsync(putObjectRequest);
}

async Task DownloadFile(string downloadFilePath, string awsStoredPath)
{
    var getObjectRequest = new GetObjectRequest
    {
        BucketName = bucketName,
        Key = awsStoredPath
    };
    
    var response = await s3Client.GetObjectAsync(getObjectRequest);

    await using var fileStream = new FileStream(downloadFilePath, FileMode.Create, FileAccess.Write);
    await response.ResponseStream.CopyToAsync(fileStream);
}

await UploadFile("./report.pdf", "pdfs/report-2024.pdf", "application/pdf");
await UploadFile("./movies.csv", "reports/movies_catalog.csv", "text/csv");

await DownloadFile("./report-downloaded-file.pdf", "pdfs/report-2024.pdf");
await DownloadFile("./movies-downloaded-file.csv", "reports/movies_catalog.csv");