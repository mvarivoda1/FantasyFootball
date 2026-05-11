using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // 1-1: korisnik ima (opcionalno) jedan fantasy tim
        [ForeignKey(nameof(FantasyTeam))]
        public int? FantasyTeamId { get; set; }
        public virtual FantasyTeam? FantasyTeam { get; set; }
    }
}
