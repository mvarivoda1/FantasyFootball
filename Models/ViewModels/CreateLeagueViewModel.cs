using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class CreateLeagueViewModel
    {
        [Required(ErrorMessage = "Naziv lige je obavezan.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Naziv mora imati između 3 i 100 znakova.")]
        [Display(Name = "Naziv lige")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maksimalan broj ekipa je obavezan.")]
        [Range(2, 20, ErrorMessage = "Maksimalan broj ekipa mora biti između 2 i 20.")]
        [Display(Name = "Maksimalan broj ekipa")]
        public int MaxTeams { get; set; } = 8;
    }
}
