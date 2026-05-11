using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Player))]
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;

        // Tim koji je izveo akciju (kupio ili prodao igrača)
        [ForeignKey(nameof(Team))]
        public int TeamId { get; set; }
        public virtual FantasyTeam Team { get; set; } = null!;

        public TransferDirection Direction { get; set; }
        public DateTime TransferDate { get; set; }
        public double Price { get; set; }

        // 1-N: transfer pripada jednoj ligi
        [ForeignKey(nameof(League))]
        public int? LeagueId { get; set; }
        public virtual League? League { get; set; }
    }
}
