public interface IImageRepository
{
    Task AddAsync(ImageMetadata imageMetadata);
    Task<List<ImageMetadata>> GetAllAsync();
    Task DeleteAsync(ImageMetadata imageMetadata);
    Task<ImageMetadata> GetImageMetadata(string s3Key, string userId);
    Task<List<ImageMetadata>> GetImagesByUserId(string userId);
}