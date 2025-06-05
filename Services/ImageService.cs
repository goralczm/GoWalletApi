using System.Net;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileHandler _fileHandler;

    private const int MAX_IMAGES_PER_USER = 3;

    public ImageService(IImageRepository imageRepository, AWSFileHandler awsFileHandler)
    {
        _imageRepository = imageRepository;
        _fileHandler = awsFileHandler;
    }

    public async Task<ImageMetadata> UploadImageAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            throw new HttpRequestException("Image not provided.", null, HttpStatusCode.BadRequest);

        if (!await CanUserUpload(userId))
            throw new HttpRequestException("Exceeded images upload limit.", null, HttpStatusCode.BadRequest);

        UploadedFileDTO uploadedFile = await _fileHandler.UploadAsync(file, userId);

        ImageMetadata imageMetadata = new ImageMetadata
        {
            FileName = file.FileName,
            ObjectKey = uploadedFile.ObjectKey,
            Url = uploadedFile.Url,
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

    public async Task<string> GetImageUrl(string objectKey, string userId)
    {
        ImageMetadata image = await GetImageMetadata(objectKey, userId);

        return await _fileHandler.GetUrl(objectKey);
    }

    public async Task<List<ImageMetadata>> GetAllAsync()
    {
        return await _imageRepository.GetAllAsync();
    }

    public async Task<List<ImageMetadata>> GetImagesByUserId(string userId)
    {
        return await _imageRepository.GetImagesByUserId(userId);
    }

    public async Task<bool> DeleteImageAsync(string objectKey, string userId)
    {
        ImageMetadata imageMetadata = await GetImageMetadata(objectKey, userId);

        await _imageRepository.DeleteAsync(imageMetadata);

        return await _fileHandler.DeleteAsync(objectKey);
    }

    public async Task<ImageMetadata> GetImageMetadata(string s3Key, string userId)
    {
        return await _imageRepository.GetImageMetadata(s3Key, userId);
    }
}