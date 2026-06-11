using System.Globalization;
using FantasyFootball.DAL;
using FantasyFootball.Filters;
using FantasyFootball.Models;
using FantasyFootball.Repositories;
using FantasyFootball.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Globalno preusmjeravanje korisnika bez fantasy tima na Build stranicu
    options.Filters.Add<RequireFantasyTeamFilter>();
});

// EF Core — DbContext (IdentityDbContext)
builder.Services.AddDbContext<FantasyFootballDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FantasyFootballDbContext")));

// ASP.NET Core Identity — lokalni računi, role i vanjski login provideri
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

// Custom claims factory — dodaje "FantasyTeamId" claim
builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

// Konfiguracija Identity application cookie-a (login/logout/access denied putanje)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// 3rd-party autentikacija — Google. Tajne se čitaju iz konfiguracije / user-secrets;
// placeholder vrijednosti omogućuju da se aplikacija pokrene i bez stvarnih kredencijala.
builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "placeholder-client-id";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "placeholder-client-secret";
    });

// Globalna autorizacija — sve rute zahtijevaju prijavu osim onih označenih [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// EF-bazirani repozitoriji
builder.Services.AddScoped<PlayerRepository>();
builder.Services.AddScoped<FantasyTeamRepository>();
builder.Services.AddScoped<TransferRepository>();
builder.Services.AddScoped<LeagueRepository>();
builder.Services.AddScoped<GameweekRepository>();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Lokalizacija — podržani jezici hr i en, default hr
var supportedCultures = new[]
{
    new CultureInfo("hr-HR"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr-HR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAuthentication();
app.UseAuthorization();

// Static assets moraju biti dostupni neprijavljenima (CSS, JS, slike)
app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Marker za integracijske testove (WebApplicationFactory<Program>)
public partial class Program { }
