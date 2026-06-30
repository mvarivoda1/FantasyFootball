using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    public class MatchPerformance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Player))]
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        [ForeignKey(nameof(Gameweek))]
        public int GameweekId { get; set; }
        public virtual Gameweek Gameweek { get; set; }

        public DateTime MatchDate { get; set; }
        public string Opponent { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public bool CleanSheet { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int MinutesPlayed { get; set; }

        // Golman: obrane (1 bod na svake 3). GK/DEF: primljeni golovi (-1 na svaka 2).
        public int Saves { get; set; }
        public int GoalsConceded { get; set; }

        // Bonus bodovi (1–3) za najbolje igrače utakmice.
        public int Bonus { get; set; }

        public int PointsEarned { get; set; }
    }
}
