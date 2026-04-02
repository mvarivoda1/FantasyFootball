namespace FantasyFootball.Models
{
    public class MatchPerformance
    {
        public int Id { get; set; }
        public Player Player { get; set; }
        public DateTime MatchDate { get; set; }
        public string Opponent { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public bool CleanSheet { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int MinutesPlayed { get; set; }
        public int PointsEarned { get; set; }
    }
}
