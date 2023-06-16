using Microsoft.AspNetCore.Authorization;

namespace SignalRServer.Authorization
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public RoleRequirement(string role)
        {
            Role = role;
        }

        public string Role { get; }
    }
}
