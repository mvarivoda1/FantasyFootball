using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email nije u ispravnom formatu.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamti me")]
        public bool RememberMe { get; set; }
    }
}
