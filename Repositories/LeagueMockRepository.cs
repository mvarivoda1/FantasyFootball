using FantasyFootball.Models;

namespace FantasyFootball.Repositories
{
    public class LeagueMockRepository
    {
        private readonly List<League> _leagues;

        public LeagueMockRepository(FantasyTeamMockRepository teamRepo, TransferMockRepository transferRepo)
        {
            var teams = teamRepo.GetAll();
            var transfers = transferRepo.GetAll();

            var league1 = new League
            {
                Id = 1, Name = "Premier Fantasy Liga", Season = "2025/2026",
                CreatedAt = new DateTime(2025, 8, 1), MaxTeams = 8,
                Description = "Glavna liga za ozbiljne fantasy managere"
            };
            var league2 = new League
            {
                Id = 2, Name = "Prijatelji Liga", Season = "2025/2026",
                CreatedAt = new DateTime(2025, 8, 10), MaxTeams = 6,
                Description = "Liga za prijatelje"
            };
            var league3 = new League
            {
                Id = 3, Name = "Studentska Liga", Season = "2025/2026",
                CreatedAt = new DateTime(2025, 9, 1), MaxTeams = 10,
                Description = "Liga za studente"
            };

            // Assign teams to leagues (5 teams each)
            foreach (var t in teams.Where(t => t.Id >= 1  && t.Id <= 5))  { league1.Teams.Add(t); t.League = league1; }
            foreach (var t in teams.Where(t => t.Id >= 6  && t.Id <= 10)) { league2.Teams.Add(t); t.League = league2; }
            foreach (var t in teams.Where(t => t.Id >= 11 && t.Id <= 15)) { league3.Teams.Add(t); t.League = league3; }

            // Assign transfers to leagues (5 transfers each)
            foreach (var tr in transfers.Where(t => t.Id >= 1  && t.Id <= 5))  league1.Transfers.Add(tr);
            foreach (var tr in transfers.Where(t => t.Id >= 6  && t.Id <= 10)) league2.Transfers.Add(tr);
            foreach (var tr in transfers.Where(t => t.Id >= 11 && t.Id <= 15)) league3.Transfers.Add(tr);

            _leagues = new List<League> { league1, league2, league3 };
        }

        public List<League> GetAll() => _leagues;
        public League? GetById(int id) => _leagues.FirstOrDefault(l => l.Id == id);
    }
}
