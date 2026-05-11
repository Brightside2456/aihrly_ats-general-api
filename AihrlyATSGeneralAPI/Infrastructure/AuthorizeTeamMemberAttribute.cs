using AihrlyATSGeneralAPI.Data;
using AihrlyATSGeneralAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AihrlyATSGeneralAPI.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeTeamMemberAttribute : TypeFilterAttribute
{
    public AuthorizeTeamMemberAttribute() : base(typeof(AuthorizeTeamMemberFilter)) { }
}

public class AuthorizeTeamMemberFilter : IAsyncActionFilter
{
    private readonly AihrlyDbContext _db;

    public AuthorizeTeamMemberFilter(AihrlyDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Team-Member-Id", out var teamMemberIdStr) || 
            !int.TryParse(teamMemberIdStr, out var teamMemberId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "X-Team-Member-Id header is missing or invalid." });
            return;
        }

        var teamMember = await _db.TeamMembers.FindAsync(teamMemberId);
        if (teamMember == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid Team Member ID." });
            return;
        }

        context.HttpContext.Items["TeamMember"] = teamMember;
        await next();
    }
}

public static class HttpContextExtensions
{
    public static TeamMember GetTeamMember(this HttpContext context)
    {
        return (TeamMember)context.Items["TeamMember"]!;
    }
}
