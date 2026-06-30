using FantasyFootball.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.DAL
{
    // IdentityDbContext daje DbSet<AppUser> Users, role i sve Identity tablice.
    public class FantasyFootballDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        protected FantasyFootballDbContext() { }

        public FantasyFootballDbContext(DbContextOptions<FantasyFootballDbContext> options) : base(options)
        { }

        public DbSet<Player> Players { get; set; }
        public DbSet<FantasyTeam> FantasyTeams { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Gameweek> Gameweeks { get; set; }
        public DbSet<MatchPerformance> MatchPerformances { get; set; }
        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<GameweekTeamScore> GameweekTeamScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // N-N: Player <-> FantasyTeam (EF će kreirati join tablicu automatski)
            modelBuilder.Entity<Player>()
                .HasMany(p => p.FantasyTeams)
                .WithMany(t => t.Players);

            // 1-N: League -> FantasyTeam
            modelBuilder.Entity<FantasyTeam>()
                .HasOne(t => t.League)
                .WithMany(l => l.Teams)
                .HasForeignKey(t => t.LeagueId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-N: League -> Transfer
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.League)
                .WithMany(l => l.Transfers)
                .HasForeignKey(t => t.LeagueId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-N: Player -> Transfer
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.Player)
                .WithMany()
                .HasForeignKey(t => t.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N: FantasyTeam -> Transfer (tim koji je obavio akciju)
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.Team)
                .WithMany()
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N: Gameweek -> MatchPerformance
            modelBuilder.Entity<MatchPerformance>()
                .HasOne(mp => mp.Gameweek)
                .WithMany(g => g.Performances)
                .HasForeignKey(mp => mp.GameweekId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-N: Player -> MatchPerformance
            modelBuilder.Entity<MatchPerformance>()
                .HasOne(mp => mp.Player)
                .WithMany()
                .HasForeignKey(mp => mp.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N: Gameweek -> Fixture (brisanjem kola brišu se i utakmice)
            modelBuilder.Entity<Fixture>()
                .HasOne(f => f.Gameweek)
                .WithMany(g => g.Fixtures)
                .HasForeignKey(f => f.GameweekId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-N: Gameweek -> GameweekTeamScore (Cascade — kao MatchPerformance)
            modelBuilder.Entity<GameweekTeamScore>()
                .HasOne(s => s.Gameweek)
                .WithMany()
                .HasForeignKey(s => s.GameweekId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-N: FantasyTeam -> GameweekTeamScore (Restrict — izbjegava multiple
            // cascade paths; tim se prije brisanja mora ručno očistiti od rezultata)
            modelBuilder.Entity<GameweekTeamScore>()
                .HasOne(s => s.FantasyTeam)
                .WithMany()
                .HasForeignKey(s => s.FantasyTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-1: AppUser <-> FantasyTeam (AppUser drži FK)
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.FantasyTeam)
                .WithOne(t => t.Owner)
                .HasForeignKey<AppUser>(u => u.FantasyTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // JoinCode lige mora biti jedinstven
            modelBuilder.Entity<League>()
                .HasIndex(l => l.JoinCode)
                .IsUnique();
        }
    }
}
