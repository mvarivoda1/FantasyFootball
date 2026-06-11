using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models
{
    public class Gameweek : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Broj kola je obavezan.")]
        [Range(1, 38, ErrorMessage = "Broj kola mora biti između 1 i 38.")]
        [Display(Name = "Broj kola")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum početka")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Datum završetka je obavezan.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Datum završetka")]
        public DateTime EndDate { get; set; }

        public virtual ICollection<MatchPerformance> Performances { get; set; }

        public Gameweek()
        {
            Performances = new List<MatchPerformance>();
        }

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
