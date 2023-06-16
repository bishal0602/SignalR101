using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SignalRServer.Authorization
{
    public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement, HubInvocationContext>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement, HubInvocationContext resource)
        {
            var roles = ((ClaimsIdentity)context.User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

            if (roles.Contains(requirement.Role))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
