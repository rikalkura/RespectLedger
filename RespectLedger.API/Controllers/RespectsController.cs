using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RespectLedger.Application.Common.Interfaces;
using RespectLedger.Application.Features.Respects.Commands.GiveRespect;
using RespectLedger.Application.Features.Respects.Commands.LikeRespect;
using RespectLedger.Application.Features.Respects.Queries.GetGlobalFeed;
using RespectLedger.Application.Features.Respects.Queries.GetUserRespectHistory;
using System.Security.Claims;

namespace RespectLedger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RespectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IImageStorageService _imageStorageService;
    private readonly IRespectLikeRepository _respectLikeRepository;

    public RespectsController(
        IMediator mediator,
        IImageStorageService imageStorageService,
        IRespectLikeRepository respectLikeRepository)
    {
        _mediator = mediator;
        _imageStorageService = imageStorageService;
        _respectLikeRepository = respectLikeRepository;
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

    [HttpGet("feed")]
    public async Task<IActionResult> GetGlobalFeed([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetGlobalFeedQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        // Update UserLiked status for current user
        Guid currentUserId = GetCurrentUserId();
        List<RespectDto> updatedItems = new();
        foreach (var item in result.Value.Items)
        {
            bool userLiked = await _respectLikeRepository.HasUserLikedAsync(item.Id, currentUserId, cancellationToken);
            updatedItems.Add(item with { UserLiked = userLiked });
        }

        var updatedResult = result.Value with { Items = updatedItems };
        return Ok(updatedResult);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<IActionResult> GetUserRespectHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserRespectHistoryQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        // Update UserLiked status for current user
        Guid currentUserId = GetCurrentUserId();
        List<RespectDto> updatedItems = new();
        foreach (var item in result.Value.Items)
        {
            bool userLiked = await _respectLikeRepository.HasUserLikedAsync(item.Id, currentUserId, cancellationToken);
            updatedItems.Add(item with { UserLiked = userLiked });
        }

        var updatedResult = result.Value with { Items = updatedItems };
        return Ok(updatedResult);
    }

    [HttpPost("{respectId}/like")]
    public async Task<IActionResult> LikeRespect(int respectId, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        var command = new LikeRespectCommand(respectId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    private Guid GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
