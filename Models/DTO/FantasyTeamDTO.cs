namespace FantasyFootball.Models.DTO
{
    // DTO za fantasy tim — ugniježđeno sadrži ligu i popis igrača.
    public class FantasyTeamDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public double SquadValue { get; set; }
        public int TotalPoints { get; set; }

        public LeagueDTO? League { get; set; }
        public List<PlayerDTO> Players { get; set; } = new();
    }
}
