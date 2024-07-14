using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/{userRef}/members")]
public class MembersController(
    ILogger logger,
    DatabaseContext db,
    MemberRendererService memberRendererService,
    ISnowflakeGenerator snowflakeGenerator) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<MembersController>();

    [HttpGet]
    [ProducesResponseType<IEnumerable<MemberRendererService.PartialMember>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembersAsync(string userRef)
    {
        var user = await db.ResolveUserAsync(userRef, CurrentToken);
        return Ok(await memberRendererService.RenderUserMembersAsync(user, CurrentToken));
    }

    [HttpGet("{memberRef}")]
    [ProducesResponseType<MemberRendererService.MemberResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberAsync(string userRef, string memberRef)
    {
        var member = await db.ResolveMemberAsync(userRef, memberRef, CurrentToken);
        return Ok(memberRendererService.RenderMember(member, CurrentToken));
    }

    [HttpPost("/api/v2/users/@me/members")]
    [ProducesResponseType<MemberRendererService.MemberResponse>(StatusCodes.Status200OK)]
    [Authorize("member.create")]
    public async Task<IActionResult> CreateMemberAsync([FromBody] CreateMemberRequest req)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        // "Translation of the 'string.Equals' overload with a 'StringComparison' parameter is not supported."
        // Member names are case-insensitive, so we need to compare the lowercase forms of both.
#pragma warning disable CA1862
        if (await db.Members.AnyAsync(m => m.UserId == CurrentUser!.Id && m.Name.ToLower() == req.Name.ToLower()))
#pragma warning restore CA1862
        {
            throw new ApiError.BadRequest("A member with that name already exists", "name", req.Name);
        }

        var member = new Member
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Name = req.Name,
            User = CurrentUser!
        };
        db.Add(member);

        _logger.Debug("Creating member {MemberName} ({Id}) for {UserId}", member.Name, member.Id, CurrentUser!.Id);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(memberRendererService.RenderMember(member, CurrentToken));
    }

    public record CreateMemberRequest(string Name);
}