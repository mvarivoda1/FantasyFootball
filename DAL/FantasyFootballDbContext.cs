using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.DAL
{
    public class FantasyFootballDbContext : DbContext
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
        public DbSet<User> Users { get; set; }

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

            // 1-1: User <-> FantasyTeam (User drži FK)
            modelBuilder.Entity<User>()
                .HasOne(u => u.FantasyTeam)
                .WithOne(t => t.Owner)
                .HasForeignKey<User>(u => u.FantasyTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Email mora biti jedinstven
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
