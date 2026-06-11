namespace FantasyFootball.Models.DTO
{
    // DTO za kolo (gameweek).
    public class GameweekDTO
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PerformancesCount { get; set; }
    }
}
