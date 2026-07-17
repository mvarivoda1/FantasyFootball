using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.DTO
{
    // Ulazni DTO za kreiranje/izmjenu fantasy tima preko API-ja.
    public class FantasyTeamInputDTO
    {
        [Required(ErrorMessage = "Naziv tima je obavezan.")]
        [StringLength(60, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime vlasnika je obavezno.")]
        [StringLength(100, MinimumLength = 2)]
        public string OwnerName { get; set; } = string.Empty;

        public int? LeagueId { get; set; }
    }
}
