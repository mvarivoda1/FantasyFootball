using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    // Forma koju korisnik ispunjava pri prvoj prijavi vanjskim providerom (Google),
    // kako bi se dovršila registracija lokalnog AppUser računa.
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email nije u ispravnom formatu.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

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

        public string Provider { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
