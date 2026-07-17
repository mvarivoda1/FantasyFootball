namespace FantasyFootball.Models.DTO
{
    // DTO za transfer — ugniježđeno sadrži osnovne podatke igrača i tima.
    public class TransferDTO
    {
        public int Id { get; set; }
        public string Direction { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public double Price { get; set; }

        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;

        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;

        public int? LeagueId { get; set; }
        public string? LeagueName { get; set; }
    }
}
