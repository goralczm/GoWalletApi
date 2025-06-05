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
        return Created($"/images/{imageMetadata.ObjectKey}", new { Url = _imagesService.GetImageUrl(imageMetadata.ObjectKey, UserId) });
    }

    [HttpGet("{objectKey}"), Authorize]
    public async Task<IActionResult> GetImage(string objectKey)
    {
        ImageMetadata image = await _imagesService.GetImageMetadata(objectKey, UserId);
        return Ok(new { Image = image });
    }

    [HttpGet("request-signed-url/{objectKey}"), Authorize]
    public async Task<IActionResult> GetImageUrl(string objectKey)
    {
        string url = await _imagesService.GetImageUrl(objectKey, UserId);
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

    [HttpDelete("delete/{objectKey}"), Authorize]
    public async Task<IActionResult> DeleteImage(string objectKey)
    {
        bool deleted = await _imagesService.DeleteImageAsync(objectKey, UserId);
        return NoContent();
    }
}
