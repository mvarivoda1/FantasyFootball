using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    // Jedna odigrana utakmica unutar kola (10 po kolu). Generira se simulacijom
    // i prikazuje u statistici kola kao rezultat (HomeClub H–A AwayClub).
    public class Fixture
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Gameweek))]
        public int GameweekId { get; set; }
        public virtual Gameweek Gameweek { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string HomeClub { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AwayClub { get; set; } = string.Empty;

        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
    }
}
