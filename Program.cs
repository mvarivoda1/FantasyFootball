using FantasyFootball.DAL;
using FantasyFootball.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core — DbContext
builder.Services.AddDbContext<FantasyFootballDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FantasyFootballDbContext")));

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
    ctx.Database.Migrate();
    DbSeeder.Seed(ctx);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
