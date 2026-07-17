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

        // ===== Gameweek slider =====

        // Sva odigrana kola (za navigaciju strelicama). Prazno = još nema simulacija.
        public List<GameweekOption> Gameweeks { get; set; } = new();

        // Odabrano kolo (WeekNumber). null = prikaz ukupne sezone.
        public int? SelectedGameweek { get; set; }

        // Bodovi tima u odabranom kolu (snapshot iz GameweekTeamScore).
        public int? GameweekTeamPoints { get; set; }

        // Ukupni bodovi tima u sezoni (zbroj svih kola).
        public int SeasonTotalPoints { get; set; }

        // PlayerId -> bodovi u odabranom kolu (0 ako igrač nije nastupio).
        public Dictionary<int, int> PlayerGameweekPoints { get; set; } = new();

        public bool ShowingGameweek => SelectedGameweek.HasValue;

        public GameweekOption? PrevGameweek => Gameweeks
            .Where(g => g.Number < (SelectedGameweek ?? int.MaxValue))
            .OrderByDescending(g => g.Number).FirstOrDefault();

        public GameweekOption? NextGameweek => SelectedGameweek.HasValue
            ? Gameweeks.Where(g => g.Number > SelectedGameweek.Value)
                .OrderBy(g => g.Number).FirstOrDefault()
            : null;
    }

    public class GameweekOption
    {
        public int Id { get; set; }
        public int Number { get; set; }
    }
}
