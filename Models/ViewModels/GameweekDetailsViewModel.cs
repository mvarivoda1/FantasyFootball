namespace FantasyFootball.Models.ViewModels
{
    // Model za prikaz statistike jednog kola (Gameweek/Details).
    public class GameweekDetailsViewModel
    {
        public Gameweek Gameweek { get; set; } = null!;

        // Odigrane utakmice kola (10 rezultata), poredane.
        public List<Fixture> Fixtures { get; set; } = new();

        // Kolo je odigrano ako ima utakmica.
        public bool IsSimulated => Fixtures.Count > 0;

        // Admin smije simulirati samo još neodigrano kolo.
        public bool CanSimulate { get; set; }
    }
}
