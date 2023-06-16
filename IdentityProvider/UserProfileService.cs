using IdentityModel;
using IdentityProvider.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityProvider
{
    public class UserProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;

        public UserProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory)
        {
            _userManager = userManager;
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        }
        async Task IProfileService.GetProfileDataAsync(ProfileDataRequestContext context)
        {
            string subject = context.Subject.GetSubjectId();
            ApplicationUser user = await _userManager.FindByIdAsync(subject);
            ClaimsPrincipal claimsPrincipal = await _userClaimsPrincipalFactory.CreateAsync(user);
            List<Claim> claims = claimsPrincipal.Claims.ToList();
            claims = claims.Where(c => context.RequestedClaimTypes.Contains(c.Type)).ToList();

            claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            claims.Add(new Claim(JwtClaimTypes.Name, user.UserName));

            if (_userManager.SupportsUserRole)
            {
                foreach (string role in await _userManager.GetRolesAsync(user))
                {
                    claims.Add(new Claim(JwtClaimTypes.Role, role));
                    if (role == "admin")
                    {
                        claims.Add(new Claim("admin", "true"));
                    }
                }
            }
            context.IssuedClaims = claims;
        }

        async Task IProfileService.IsActiveAsync(IsActiveContext context)
        {
            var subject = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(subject);
            context.IsActive = (user != null);
        }
    }
}
