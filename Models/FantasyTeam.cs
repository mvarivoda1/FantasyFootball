namespace FantasyFootball.Models
{
    public class FantasyTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Budget { get; set; }
        public int TotalPoints { get; set; }

        // N-N: tim ima više igrača
        public List<Player> Players { get; set; }

        // 1-N: tim pripada jednoj ligi
        public League League { get; set; }

        public FantasyTeam()
        {
            Players = new List<Player>();
        }
    }
}
