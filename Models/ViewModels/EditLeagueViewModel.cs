using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class EditLeagueViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv lige je obavezan.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Naziv mora imati između 3 i 100 znakova.")]
        [Display(Name = "Naziv lige")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maksimalan broj ekipa je obavezan.")]
        [Range(2, 20, ErrorMessage = "Maksimalan broj ekipa mora biti između 2 i 20.")]
        [Display(Name = "Maksimalan broj ekipa")]
        public int MaxTeams { get; set; }

        [StringLength(500, ErrorMessage = "Opis može imati najviše 500 znakova.")]
        [Display(Name = "Opis")]
        public string Description { get; set; } = string.Empty;

        // Informativna polja koja se ne mijenjaju kroz formu
        public string Season { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public int TeamsCount { get; set; }
    }
}
