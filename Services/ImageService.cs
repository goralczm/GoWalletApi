using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using System.Net;

public class ImageService : IImageService
{
    private const string BUCKET_NAME = "gowallet-files";

    private readonly IAmazonS3 _s3Client;
    private readonly IImageRepository _imageRepository;

    private const int MAX_IMAGES_PER_USER = 3;

    public ImageService(IAmazonS3 s3Client, IImageRepository imageRepository)
    {
        _s3Client = s3Client;
        _imageRepository = imageRepository;
    }

    public async Task<ImageMetadata> UploadImageAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            throw new HttpRequestException("Image not provided.", null, HttpStatusCode.BadRequest);

        if (!await CanUserUpload(userId))
            throw new HttpRequestException("Exceeded images upload limit.", null, HttpStatusCode.BadRequest);

        string objectKey = await UploadImageToS3(file, userId);

        ImageMetadata imageMetadata = new ImageMetadata
        {
            FileName = file.FileName,
            S3Key = objectKey,
            Url = $"https://{BUCKET_NAME}.s3.amazonaws.com/{objectKey}",
            ContentType = file.ContentType,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };

        await _imageRepository.AddAsync(imageMetadata);

        return imageMetadata;
    }

    private async Task<bool> CanUserUpload(string userId)
    {
        List<ImageMetadata> uploadedImages = await _imageRepository.GetImagesByUserId(userId);

        return uploadedImages.Count < MAX_IMAGES_PER_USER;
    }

    private async Task<string> UploadImageToS3(IFormFile file, string userId)
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

        return objectKey;
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

    public async Task<List<ImageMetadata>> GetAllAsync()
    {
        return await _imageRepository.GetAllAsync();
    }

    public async Task<List<ImageMetadata>> GetImagesByUserId(string userId)
    {
        return await _imageRepository.GetImagesByUserId(userId);
    }

    public async Task<bool> DeleteImageAsync(string s3Key, string userId)
    {
        ImageMetadata imageMetadata = await GetImageMetadata(s3Key, userId);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = BUCKET_NAME,
            Key = s3Key
        };

        await _imageRepository.DeleteAsync(imageMetadata);

        return await _s3Client.DeleteObjectAsync(deleteRequest).ContinueWith(t => t.IsCompletedSuccessfully);
    }

    public async Task<ImageMetadata> GetImageMetadata(string s3Key, string userId)
    {
        return await _imageRepository.GetImageMetadata(s3Key, userId);
    }
}