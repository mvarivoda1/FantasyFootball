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
        public virtual Player Player { get; set; }

        [ForeignKey(nameof(FromTeam))]
        public int? FromTeamId { get; set; }
        public virtual FantasyTeam? FromTeam { get; set; }

        [ForeignKey(nameof(ToTeam))]
        public int ToTeamId { get; set; }
        public virtual FantasyTeam ToTeam { get; set; }

        public DateTime TransferDate { get; set; }
        public double Price { get; set; }
        public TransferStatus Status { get; set; }

        // 1-N: transfer pripada jednoj ligi
        [ForeignKey(nameof(League))]
        public int? LeagueId { get; set; }
        public virtual League? League { get; set; }
    }
}
