using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FantasyFootball.Models
{
    // Aplikacijski korisnik — proširuje ASP.NET Core Identity (IdentityUser, string PK).
    // Email / UserName / PasswordHash dolaze iz IdentityUser baze.
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "OIB je obavezan.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB mora imati točno 11 znamenki.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
        [Display(Name = "OIB")]
        public string OIB { get; set; } = string.Empty;

        [Required(ErrorMessage = "JMBG je obavezan.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati točno 13 znamenki.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve.")]
        [Display(Name = "JMBG")]
        public string JMBG { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Transfer budžet — koliko korisnik može potrošiti na nove igrače
        public double Budget { get; set; }

        // 1-1: korisnik ima (opcionalno) jedan fantasy tim
        [ForeignKey(nameof(FantasyTeam))]
        public int? FantasyTeamId { get; set; }
        public virtual FantasyTeam? FantasyTeam { get; set; }
    }
}
