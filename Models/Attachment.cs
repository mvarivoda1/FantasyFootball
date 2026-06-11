using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    // Priložena datoteka vezana uz fantasy tim (upload preko Dropzone komponente).
    // Datoteka se sprema na disk, a u bazu idu metapodaci i putanja.
    public class Attachment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(FantasyTeam))]
        public int FantasyTeamId { get; set; }
        public virtual FantasyTeam? FantasyTeam { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
