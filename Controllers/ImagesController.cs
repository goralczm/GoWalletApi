using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImagesService _imagesService;

    private string UserId => User.FindFirst("user_id")?.Value ?? throw new HttpRequestException("No authorized user.", null, HttpStatusCode.Unauthorized);

    public ImagesController(IImagesService imagesService)
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
        try
        {
            ImageMetadata image = await _imagesService.GetImageMetadata(s3Key, UserId);
            return Ok(new { Image = image });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int?)ex.StatusCode ?? 500, new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet("request-signed-url/{s3Key}"), Authorize]
    public async Task<IActionResult> GetImageUrl(string s3Key)
    {
        try
        {
            string url = await _imagesService.GetImageUrl(s3Key, UserId);
            return Ok(new { Url = url });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int?)ex.StatusCode ?? 500, new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllImages()
    {
        return Ok(await _imagesService.GetAllImages());
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
