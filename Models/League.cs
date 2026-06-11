using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models
{
    public class League
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string Season { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        [Range(2, 20)]
        public int MaxTeams { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // Šifra od 6 znakova za pridruživanje ligi (npr. "A7K9PQ")
        [Required, StringLength(6, MinimumLength = 6)]
        public string JoinCode { get; set; } = string.Empty;

        // Korisnik koji je kreirao ligu (Identity string PK; opcionalno za stare seedane lige)
        public string? CreatorUserId { get; set; }

        // 1-N: liga ima više fantasy timova
        public virtual ICollection<FantasyTeam> Teams { get; set; }

        // 1-N: liga ima više transfera
        public virtual ICollection<Transfer> Transfers { get; set; }

        public League()
        {
            Teams = new List<FantasyTeam>();
            Transfers = new List<Transfer>();
        }
    }
}
