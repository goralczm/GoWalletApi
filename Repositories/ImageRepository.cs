using Microsoft.EntityFrameworkCore;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _dbContext;

    public ImageRepository(AppDbContext context)
    {
        _dbContext = context;
    }

    public async Task AddAsync(ImageMetadata imageMetadata)
    {
        _dbContext.Images.Add(imageMetadata);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ImageMetadata>> GetAllAsync()
    {
        return await _dbContext.Images
            .OrderByDescending(i => i.UploadedAt)
            .ToListAsync();
    }

    public async Task DeleteAsync(ImageMetadata imageMetadata)
    {
        _dbContext.Images.Remove(imageMetadata);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ImageMetadata> GetImageMetadata(string s3Key, string userId)
    {
        var image = await _dbContext.Images.FirstOrDefaultAsync(i => i.ObjectKey == s3Key);
        if (image == null)
            throw new HttpRequestException("Image not found.", null, HttpStatusCode.NotFound);

        if (image.UploadedBy != userId)
            throw new HttpRequestException("Unauthorized access to delete image.", null, HttpStatusCode.Unauthorized);

        return image;
    }

    public async Task<List<ImageMetadata>> GetImagesByUserId(string userId)
    {
        return await _dbContext.Images
            .Where(i => i.UploadedBy == userId)
            .OrderByDescending(i => i.UploadedAt)
            .ToListAsync();
    }
}
