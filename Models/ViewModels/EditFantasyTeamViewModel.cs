using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class EditFantasyTeamViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv tima je obavezan.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Naziv tima mora imati između 2 i 60 znakova.")]
        [Display(Name = "Naziv tima")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime vlasnika je obavezno.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ime vlasnika mora imati između 2 i 100 znakova.")]
        [Display(Name = "Vlasnik")]
        public string OwnerName { get; set; } = string.Empty;
    }
}
