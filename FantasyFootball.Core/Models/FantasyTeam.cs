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

        // Broj raspoloživih besplatnih transfera. Vrijedi tek NAKON što je odigrano
        // prvo kolo — do tada su transferi neograničeni (pretsezona). Tim dobiva +1
        // nakon svakog odigranog (potvrđenog) kola; neiskorišteni se gomilaju
        // (1, 2, 3, ...). Svaki transfer preko ovog broja stoji −4 boda.
        public int FreeTransfers { get; set; }

        // Ukupno oduzetih bodova zbog transfera preko besplatne kvote (sezona).
        public int TransferPointHits { get; set; }

        public string? StartingLineupIds { get; set; }

        // Putanja do loga tima (slika uploadana preko Dropzone). null = nema loga.
        public string? LogoPath { get; set; }

        public virtual ICollection<Player> Players { get; set; }

        [ForeignKey(nameof(League))]
        public int? LeagueId { get; set; }
        public virtual League? League { get; set; }

        public virtual AppUser? Owner { get; set; }

        public FantasyTeam()
        {
            Players = new List<Player>();
        }
    }
}
