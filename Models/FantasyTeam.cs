using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    public class FantasyTeam
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv tima je obavezan.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Naziv tima mora imati između 2 i 60 znakova.")]
        [Display(Name = "Naziv tima")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime vlasnika je obavezno.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ime vlasnika mora imati između 2 i 100 znakova.")]
        [Display(Name = "Vlasnik")]
        public string OwnerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public double SquadValue { get; set; }
        public int TotalPoints { get; set; }

        public string? StartingLineupIds { get; set; }

        public virtual ICollection<Player> Players { get; set; }

        [ForeignKey(nameof(League))]
        public int? LeagueId { get; set; }
        public virtual League? League { get; set; }

        public virtual AppUser? Owner { get; set; }

        // 1-N: tim može imati više priloženih datoteka (upload preko Dropzone)
        public virtual ICollection<Attachment> Attachments { get; set; }

        public FantasyTeam()
        {
            Players = new List<Player>();
            Attachments = new List<Attachment>();
        }
    }
}
