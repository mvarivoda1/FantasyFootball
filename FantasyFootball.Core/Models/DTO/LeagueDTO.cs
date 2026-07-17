namespace FantasyFootball.Models.DTO
{
    // DTO za ligu (koristi se i ugniježđeno unutar FantasyTeamDTO).
    public class LeagueDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public int MaxTeams { get; set; }
        public string Description { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public int TeamsCount { get; set; }
    }
}
