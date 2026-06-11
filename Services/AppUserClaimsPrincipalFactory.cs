using System.Security.Claims;
using FantasyFootball.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FantasyFootball.Services
{
    // Dodaje "FantasyTeamId" claim u korisnikov cookie kada korisnik ima tim.
    // Time _Layout.cshtml i RequireFantasyTeamFilter mogu provjeravati postojanje
    // tima bez dodatnog upita u bazu na svakom zahtjevu.
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (user.FantasyTeamId.HasValue)
                identity.AddClaim(new Claim("FantasyTeamId", user.FantasyTeamId.Value.ToString()));

            return identity;
        }
    }
}
