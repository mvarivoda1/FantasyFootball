using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models
{
    public class Gameweek
    {
        [Key]
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // 1-N: gameweek ima više performansi
        public virtual ICollection<MatchPerformance> Performances { get; set; }

        public Gameweek()
        {
            Performances = new List<MatchPerformance>();
        }
    }
}
