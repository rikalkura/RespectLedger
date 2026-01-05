using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RespectLedger.Application.Features.Users.Commands.ApproveUser;
using RespectLedger.Application.Features.Users.Commands.UpdateProfile;
using RespectLedger.Application.Features.Users.Commands.UploadAvatar;
using RespectLedger.Application.Features.Users.Queries.GetCurrentUser;
using RespectLedger.Application.Features.Users.Queries.GetPendingUsers;
using System.Security.Claims;

namespace RespectLedger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        var query = new GetCurrentUserQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        return Ok(result.Value);
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        var updateCommand = command with { UserId = userId };
        var result = await _mediator.Send(updateCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> UploadAvatar([FromBody] UploadAvatarCommand command, CancellationToken cancellationToken)
    {
        Guid userId = GetCurrentUserId();
        var uploadCommand = command with { UserId = userId };
        var result = await _mediator.Send(uploadCommand, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken cancellationToken)
    {
        var query = new GetPendingUsersQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost("{userId}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveUser(Guid userId, CancellationToken cancellationToken)
    {
        var command = new ApproveUserCommand(userId);
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
