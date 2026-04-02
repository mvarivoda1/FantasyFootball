namespace FantasyFootball.Models
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Season { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MaxTeams { get; set; }
        public string Description { get; set; }

        // 1-N: liga ima više fantasy timova
        public List<FantasyTeam> Teams { get; set; }

        // 1-N: liga ima više transfera
        public List<Transfer> Transfers { get; set; }

        public League()
        {
            Teams = new List<FantasyTeam>();
            Transfers = new List<Transfer>();
        }
    }
}
