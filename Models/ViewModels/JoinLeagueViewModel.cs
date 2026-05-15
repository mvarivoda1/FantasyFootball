using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class JoinLeagueViewModel
    {
        [Required(ErrorMessage = "Šifra lige je obavezna.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Šifra mora imati točno 6 znakova.")]
        [RegularExpression("^[A-Z0-9]{6}$", ErrorMessage = "Šifra smije sadržavati samo velika slova i brojeve.")]
        [Display(Name = "Šifra lige")]
        public string JoinCode { get; set; } = string.Empty;
    }
}
