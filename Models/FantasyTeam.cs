using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    public class FantasyTeam
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Budget { get; set; }
        public int TotalPoints { get; set; }

        // N-N: tim ima više igrača
        public virtual ICollection<Player> Players { get; set; }

        // 1-N: tim pripada jednoj ligi
        [ForeignKey(nameof(League))]
        public int? LeagueId { get; set; }
        public virtual League? League { get; set; }

        // 1-1: tim ima vlasnika (korisnika)
        public virtual User? Owner { get; set; }

        public FantasyTeam()
        {
            Players = new List<Player>();
        }
    }
}
