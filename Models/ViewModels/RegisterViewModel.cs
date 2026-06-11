using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email nije u ispravnom formatu.")]
        [StringLength(200)]
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

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati između 6 i 100 znakova.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Lozinka i potvrda lozinke se ne podudaraju.")]
        [Display(Name = "Potvrdi lozinku")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
