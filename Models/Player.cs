namespace FantasyFootball.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Position Position { get; set; }
        public string Club { get; set; }
        public string Nationality { get; set; }
        public DateTime DateOfBirth { get; set; }
        public double MarketValue { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int CleanSheets { get; set; }
        public int TotalPoints { get; set; }

        // N-N: igrač može biti u više fantasy timova
        public List<FantasyTeam> FantasyTeams { get; set; }

        public Player()
        {
            FantasyTeams = new List<FantasyTeam>();
        }

        public string FullName => $"{FirstName} {LastName}";
    }
}
