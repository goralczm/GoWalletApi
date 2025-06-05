using Amazon.S3;
using Amazon.S3.Model;

public class AWSFileHandler : IFileHandler
{
    private const string BUCKET_NAME = "gowallet-files";

    private readonly IAmazonS3 _s3Client;

    public AWSFileHandler(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<UploadedFileDTO> UploadAsync(IFormFile file, string userId)
    {
        string objectKey = System.Guid.NewGuid().ToString();

        var putRequest = new PutObjectRequest
        {
            BucketName = BUCKET_NAME,
            Key = objectKey,
            InputStream = file.OpenReadStream(),
            ContentType = file.ContentType
        };
        putRequest.Metadata.Add("UploadedBy", userId);

        await _s3Client.PutObjectAsync(putRequest);

        return new UploadedFileDTO { ObjectKey = objectKey, Url = $"https://{BUCKET_NAME}.s3.amazonaws.com/{objectKey}" };
    }

    public async Task<string> GetUrl(string objectKey)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = BUCKET_NAME,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<bool> DeleteAsync(string objectKey)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = BUCKET_NAME,
            Key = objectKey
        };

        return await _s3Client.DeleteObjectAsync(deleteRequest).ContinueWith(t => t.IsCompletedSuccessfully);
    }
}