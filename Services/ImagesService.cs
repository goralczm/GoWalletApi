using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using System.Net;

public class ImagesService : IImagesService
{
    private const string BUCKET_NAME = "gowallet-files";

    private readonly IAmazonS3 _s3Client;
    private readonly AppDbContext _dbContext;

    public ImagesService(IAmazonS3 s3Client, AppDbContext dbContext)
    {
        _s3Client = s3Client;
        _dbContext = dbContext;
    }

    public async Task<ImageMetadata> UploadImageAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0) throw new HttpRequestException("Image not provided.", null, HttpStatusCode.BadRequest);

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

        string s3Url = $"https://{BUCKET_NAME}.s3.amazonaws.com/{objectKey}";

        ImageMetadata imageMeta = new ImageMetadata
        {
            FileName = file.FileName,
            S3Key = objectKey,
            Url = s3Url,
            ContentType = file.ContentType,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };
        _dbContext.Images.Add(imageMeta);
        await _dbContext.SaveChangesAsync();

        return imageMeta;
    }

    public async Task<string> GetImageUrl(string s3Key, string userId)
    {
        ImageMetadata image = await GetImageMetadata(s3Key, userId);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = BUCKET_NAME,
            Key = s3Key,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public async Task<List<ImageMetadata>> GetAllImages()
    {
        return await _dbContext.Images
            .OrderByDescending(i => i.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<ImageMetadata>> GetImagesByUserId(string userId)
    {
        return await _dbContext.Images
            .Where(i => i.UploadedBy == userId)
            .OrderByDescending(i => i.UploadedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteImageAsync(string s3Key, string userId)
    {
        ImageMetadata image = await GetImageMetadata(s3Key, userId);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = BUCKET_NAME,
            Key = s3Key
        };

        _dbContext.Images.Remove(image);
        _dbContext.SaveChanges();
        return await _s3Client.DeleteObjectAsync(deleteRequest).ContinueWith(t => t.IsCompletedSuccessfully);
    }

    public async Task<ImageMetadata> GetImageMetadata(string s3Key, string userId)
    {
        var image = await _dbContext.Images.FirstOrDefaultAsync(i => i.S3Key == s3Key);
        if (image == null)
            throw new HttpRequestException("Image not found.", null, HttpStatusCode.NotFound);

        if (image.UploadedBy != userId)
            throw new HttpRequestException("Unauthorized access to delete image.", null, HttpStatusCode.Unauthorized);

        return image;
    }
}