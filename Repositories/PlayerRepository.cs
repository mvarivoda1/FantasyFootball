using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Repositories
{
    public class PlayerRepository
    {
        private readonly FantasyFootballDbContext _ctx;

        public PlayerRepository(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        public List<Player> GetAll() =>
            _ctx.Players
                .Include(p => p.FantasyTeams)
                .AsNoTracking()
                .ToList();

        public Player? GetById(int id) =>
            _ctx.Players
                .Include(p => p.FantasyTeams)
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == id);

        // Pretraga igrača po imenu, prezimenu, punom imenu, klubu ili nacionalnosti.
        public List<Player> Search(string term, int take = 10)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Player>();
            term = term.Trim();

            return _ctx.Players
                .AsNoTracking()
                .Where(p =>
                    p.FirstName.Contains(term) ||
                    p.LastName.Contains(term) ||
                    (p.FirstName + " " + p.LastName).Contains(term) ||
                    p.Club.Contains(term) ||
                    p.Nationality.Contains(term))
                .OrderBy(p => p.LastName)
                .Take(take)
                .ToList();
        }
    }
}
