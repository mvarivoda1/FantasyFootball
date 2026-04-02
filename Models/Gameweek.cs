namespace FantasyFootball.Models
{
    public class Gameweek
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // 1-N: gameweek ima više performansi
        public List<MatchPerformance> Performances { get; set; }

        public Gameweek()
        {
            Performances = new List<MatchPerformance>();
        }
    }
}
