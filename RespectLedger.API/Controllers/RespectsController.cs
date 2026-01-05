using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Application.Features.Respects.Commands.GiveRespect;
using System.Security.Claims;

namespace RespectLedger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RespectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IImageStorageService _imageStorageService;

    public RespectsController(IMediator mediator, IImageStorageService imageStorageService)
    {
        _mediator = mediator;
        _imageStorageService = imageStorageService;
    }

    [HttpPost]
    public async Task<IActionResult> GiveRespect([FromBody] GiveRespectCommand command, CancellationToken cancellationToken)
    {
        Guid senderId = GetCurrentUserId();
        var giveRespectCommand = command with { SenderId = senderId };
        
        var result = await _mediator.Send(giveRespectCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest("Invalid file type. Allowed types: jpg, jpeg, png, gif, webp");
        }

        // Validate file size (max 5MB)
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxFileSize)
        {
            return BadRequest("File size exceeds 5MB limit");
        }

        try
        {
            using Stream stream = file.OpenReadStream();
            string imageUrl = await _imageStorageService.UploadImageAsync(stream, file.FileName, cancellationToken);
            
            return Ok(new { ImageUrl = imageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading image: {ex.Message}");
        }
    }

    private Guid GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
