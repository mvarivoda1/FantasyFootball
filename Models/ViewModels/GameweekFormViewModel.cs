using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.ViewModels
{
    public class GameweekFormViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Broj kola je obavezan.")]
        [Range(1, 38, ErrorMessage = "Broj kola mora biti između 1 i 38.")]
        [Display(Name = "Broj kola")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum početka")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Datum završetka je obavezan.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum završetka")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        /// <summary>Najraniji dopušteni datum početka (dan nakon završetka zadnjeg kola). Samo za prikaz u datepickeru.</summary>
        public DateTime? MinStartDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "Datum završetka mora biti nakon datuma početka.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}
