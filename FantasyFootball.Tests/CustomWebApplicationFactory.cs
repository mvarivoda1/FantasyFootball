using FantasyFootball.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyFootball.Tests
{
    // Pokreće stvarnu aplikaciju kroz HTTP sloj, ali zamjenjuje SQL Server bazu
    // EF InMemory bazom i autentikaciju testnim handlerom (Admin korisnik).
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "FantasyFootballTests_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Ukloni postojeću (SQL Server) konfiguraciju DbContext-a
                var toRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<FantasyFootballDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().Name.StartsWith("IDbContextOptionsConfiguration")))
                    .ToList();
                foreach (var d in toRemove) services.Remove(d);

                // Registriraj InMemory bazu (isto ime -> dijeljeno između scope-ova)
                services.AddDbContext<FantasyFootballDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                // Testna autentikacija — sve kao Admin
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });
            });
        }

        // Pristup svježem DbContext-u za pripremu (seed) podataka u testovima.
        public FantasyFootballDbContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<FantasyFootballDbContext>();
        }
    }
}
