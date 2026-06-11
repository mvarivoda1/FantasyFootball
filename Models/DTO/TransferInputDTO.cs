using System.ComponentModel.DataAnnotations;

namespace FantasyFootball.Models.DTO
{
    // Ulazni DTO za kreiranje/izmjenu transfera preko API-ja.
    public class TransferInputDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "PlayerId je obavezan.")]
        public int PlayerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "TeamId je obavezan.")]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Smjer transfera (In/Out) je obavezan.")]
        public TransferDirection Direction { get; set; }

        [Range(0, 500, ErrorMessage = "Cijena mora biti između 0 i 500.")]
        public double Price { get; set; }

        public int? LeagueId { get; set; }
    }
}
