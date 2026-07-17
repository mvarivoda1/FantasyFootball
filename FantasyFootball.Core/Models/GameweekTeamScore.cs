using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FantasyFootball.Models
{
    // Snapshot rezultata fantasy tima za jedno kolo — "bodovi koje si upisao".
    // Kreira se pri potvrdi simulacije i koristi ga slider u "Moja momčad" te
    // poništavanje (brisanje) kola. LineupIds čuva startnih 11 u trenutku potvrde.
    public class GameweekTeamScore
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(FantasyTeam))]
        public int FantasyTeamId { get; set; }
        public virtual FantasyTeam FantasyTeam { get; set; } = null!;

        [ForeignKey(nameof(Gameweek))]
        public int GameweekId { get; set; }
        public virtual Gameweek Gameweek { get; set; } = null!;

        public int Points { get; set; }

        // CSV ID-eva startnih 11 igrača u trenutku potvrde kola (povijesni snapshot).
        public string? LineupIds { get; set; }
    }
}
