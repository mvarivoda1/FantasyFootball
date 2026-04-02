namespace FantasyFootball.Models
{
    public class Standing
    {
        public int Rank { get; set; }
        public FantasyTeam Team { get; set; }
        public int Points { get; set; }
        public int GamesPlayed { get; set; }
    }
}
