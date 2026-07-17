using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class PlayerFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora imati između 2 i 50 znakova.")]
        [Display(Name = "Ime")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora imati između 2 i 50 znakova.")]
        [Display(Name = "Prezime")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pozicija je obavezna.")]
        [Display(Name = "Pozicija")]
        public Position Position { get; set; }

        [Required(ErrorMessage = "Klub je obavezan.")]
        [StringLength(100, ErrorMessage = "Klub može imati najviše 100 znakova.")]
        [Display(Name = "Klub")]
        public string Club { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nacionalnost je obavezna.")]
        [StringLength(60, ErrorMessage = "Nacionalnost može imati najviše 60 znakova.")]
        [Display(Name = "Nacionalnost")]
        public string Nationality { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rođenja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum rođenja")]
        public DateTime DateOfBirth { get; set; } = new DateTime(2000, 1, 1);

        [Range(0.1, 200.0, ErrorMessage = "Tržišna vrijednost mora biti između 0.1 i 200 milijuna.")]
        [Display(Name = "Tržišna vrijednost (M €)")]
        public double MarketValue { get; set; } = 1.0;

        [Range(0, 1000, ErrorMessage = "Vrijednost mora biti između 0 i 1000.")]
        [Display(Name = "Golovi")]
        public int Goals { get; set; }

        [Range(0, 1000, ErrorMessage = "Vrijednost mora biti između 0 i 1000.")]
        [Display(Name = "Asistencije")]
        public int Assists { get; set; }

        [Range(0, 1000, ErrorMessage = "Vrijednost mora biti između 0 i 1000.")]
        [Display(Name = "Clean sheets")]
        public int CleanSheets { get; set; }

        [Range(0, 10000, ErrorMessage = "Vrijednost mora biti između 0 i 10000.")]
        [Display(Name = "Ukupni bodovi")]
        public int TotalPoints { get; set; }
    }
}
