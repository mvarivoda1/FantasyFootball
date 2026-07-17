using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog — logging mehanizam (konzola + rolling file u logs/ff-api-<datum>.log).
// Razine i sinkovi čitaju se iz "Serilog" sekcije u appsettings.json.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// REST API controlleri (bez Razor viewova)
builder.Services.AddControllers();

// EF Core — DbContext (IdentityDbContext)
builder.Services.AddDbContext<FantasyFootballDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FantasyFootballDbContext")));

// ASP.NET Core Identity — ista konfiguracija kao u Web projektu; Api ne izdaje
// cookie (login radi Web), ali ga mora znati validirati i po potrebi osvježiti.
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<FantasyFootballDbContext>()
    .AddDefaultTokenProviders();

// Custom claims factory — security-stamp revalidacija cookieja ne smije izgubiti
// "FantasyTeamId" claim koji je Web dodao pri prijavi.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

// Konfiguracija Identity application cookie-a — isto kao Web; redirect na login
// u produkciji kroz reverse proxy završava na Web projektu.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Data Protection — isti ApplicationName kao Web da Api može čitati cookie koji je
// izdao Web. U kontejnerima se ključevi dijele kroz volume (DataProtection__KeysPath);
// lokalno oba procesa koriste isti user-profile key ring.
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("FantasyFootball");
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

// Globalna autorizacija — sve rute zahtijevaju prijavu osim onih označenih [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Healthcheck za Docker/compose orkestraciju
builder.Services.AddHealthChecks();

var app = builder.Build();

// Primijeni migracije i pokreni seeder pri startu aplikacije
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<FantasyFootballDbContext>();
    // Relacijska baza (SQL Server) -> migracije + seed. Kod integracijskih testova
    // (InMemory provider) preskačemo jer InMemory ne podržava migracije.
    if (ctx.Database.IsRelational())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        ctx.Database.Migrate();
        await DbSeeder.SeedAsync(ctx, userManager, roleManager);
    }
}

// Serilog — strukturirani zapis svakog HTTP zahtjeva (metoda, putanja, status, trajanje).
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// /health mora biti dostupan bez prijave (fallback policy inače traži auth)
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

// Marker za integracijske testove (WebApplicationFactory<Program>)
public partial class Program { }
