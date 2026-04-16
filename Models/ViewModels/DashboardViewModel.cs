using FantasyFootball.Models;

namespace FantasyFootball.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Gameweek Status
        public Gameweek? LatestGameweek { get; set; }
        public int MatchesInLatestGw { get; set; }
        public int TotalGoalsInLatestGw { get; set; }
        public int TotalAssistsInLatestGw { get; set; }
        public int TotalCleanSheetsInLatestGw { get; set; }

        // Next Deadline
        public int NextGameweekNumber { get; set; }
        public DateTime NextDeadline { get; set; }

        // Transfers
        public List<TransferWidgetItem> TopTransfersIn { get; set; } = new();
        public List<TransferWidgetItem> TopTransfersOut { get; set; } = new();

        // Player of the Week
        public MatchPerformance? PlayerOfTheWeek { get; set; }

        // Team of the Week (by position)
        public List<MatchPerformance> TotwGoalkeepers { get; set; } = new();
        public List<MatchPerformance> TotwDefenders { get; set; } = new();
        public List<MatchPerformance> TotwMidfielders { get; set; } = new();
        public List<MatchPerformance> TotwForwards { get; set; } = new();

        // Player Availability
        public List<PlayerAvailabilityItem> PlayerAvailability { get; set; } = new();

        // Most Valuable Teams
        public List<FantasyTeam> MostValuableTeams { get; set; } = new();

        // Best Leagues
        public List<LeagueWidgetItem> BestLeagues { get; set; } = new();

        // Top Scorers
        public List<Player> TopScorers { get; set; } = new();
    }

    public class TransferWidgetItem
    {
        public Player Player { get; set; } = null!;
        public int TransferCount { get; set; }
        public double LatestPrice { get; set; }
    }

    public class PlayerAvailabilityItem
    {
        public Player Player { get; set; } = null!;
        public string Status { get; set; } = "Available";
        public string StatusClass { get; set; } = "available";
        public int ChancePercent { get; set; } = 100;
    }

    public class LeagueWidgetItem
    {
        public League League { get; set; } = null!;
        public int TotalTeams { get; set; }
        public int TotalPoints { get; set; }
        public double AveragePoints { get; set; }
    }
}
