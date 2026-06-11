using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.DTO
{
    // Ulazni DTO za kreiranje/izmjenu lige preko API-ja (JoinCode se generira na serveru).
    public class LeagueInputDTO
    {
        [Required(ErrorMessage = "Naziv lige je obavezan.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string Season { get; set; } = string.Empty;

        [Range(2, 20, ErrorMessage = "Liga mora dopuštati između 2 i 20 timova.")]
        public int MaxTeams { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
