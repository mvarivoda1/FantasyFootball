namespace FantasyFootball.Models.ViewModels
{
    public class MyTeamViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? LogoPath { get; set; }

        public List<Player> Starters { get; set; } = new();
        public List<Player> Bench { get; set; } = new();

        public int StartersCount => 11;
        public int BenchCount => 4;

        public int MinGk { get; set; } = 1;
        public int MinDef { get; set; } = 3;
        public int MinMid { get; set; } = 2;
        public int MinFwd { get; set; } = 1;

        public int MaxGk { get; set; } = 1;
        public int MaxDef { get; set; } = 5;
        public int MaxMid { get; set; } = 5;
        public int MaxFwd { get; set; } = 3;

        public List<Player> StarterGoalkeepers => Starters.Where(p => p.Position == Position.Goalkeeper).ToList();
        public List<Player> StarterDefenders => Starters.Where(p => p.Position == Position.Defender).ToList();
        public List<Player> StarterMidfielders => Starters.Where(p => p.Position == Position.Midfielder).ToList();
        public List<Player> StarterForwards => Starters.Where(p => p.Position == Position.Forward).ToList();

        public string FormationLabel =>
            $"{StarterDefenders.Count}-{StarterMidfielders.Count}-{StarterForwards.Count}";
    }
}
