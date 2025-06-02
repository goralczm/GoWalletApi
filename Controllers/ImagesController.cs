using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

[ApiController]
[Route("api/v1/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imagesService;

    private string UserId => User.FindFirst("user_id")?.Value ?? throw new HttpRequestException("No authorized user.", null, HttpStatusCode.Unauthorized);

    public ImagesController(IImageService imagesService)
    {
        _imagesService = imagesService;
    }

    [HttpPost("upload"), Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        ImageMetadata imageMetadata = await _imagesService.UploadImageAsync(file, UserId);
        return Created($"/images/{imageMetadata.S3Key}", new { Url = _imagesService.GetImageUrl(imageMetadata.S3Key, UserId) });
    }

    [HttpGet("{s3Key}"), Authorize]
    public async Task<IActionResult> GetImage(string s3Key)
    {
        ImageMetadata image = await _imagesService.GetImageMetadata(s3Key, UserId);
        return Ok(new { Image = image });
    }

    [HttpGet("request-signed-url/{s3Key}"), Authorize]
    public async Task<IActionResult> GetImageUrl(string s3Key)
    {
        string url = await _imagesService.GetImageUrl(s3Key, UserId);
        return Ok(new { Url = url });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllImages()
    {
        return Ok(await _imagesService.GetAllAsync());
    }

    [HttpGet("my"), Authorize]
    public async Task<IActionResult> GetMyImages()
    {
        return Ok(await _imagesService.GetImagesByUserId(UserId));
    }

    [HttpDelete("delete/{s3Key}"), Authorize]
    public async Task<IActionResult> DeleteImage(string s3Key)
    {
        bool deleted = await _imagesService.DeleteImageAsync(s3Key, UserId);
        return NoContent();
    }
}
