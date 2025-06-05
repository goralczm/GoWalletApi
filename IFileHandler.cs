using Amazon.S3.Model;

public interface IFileHandler
{
    public Task<UploadedFileDTO> UploadAsync(IFormFile file, string ownerId);
    public Task<bool> DeleteAsync(string objectKey);
    public Task<string> GetUrl(string objectKey);
}