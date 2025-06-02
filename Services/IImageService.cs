public interface IImageService
{
    public Task<ImageMetadata> UploadImageAsync(IFormFile file, string userId);
    public Task<string> GetImageUrl(string s3Key, string userId);
    public Task<ImageMetadata> GetImageMetadata(string s3Key, string userId);
    public Task<List<ImageMetadata>> GetAllAsync();
    public Task<List<ImageMetadata>> GetImagesByUserId(string userId);
    public Task<bool> DeleteImageAsync(string s3Key, string userId);
}