using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Repositories
{
    public class GameweekRepository
    {
        private readonly FantasyFootballDbContext _ctx;

        public GameweekRepository(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        public List<Gameweek> GetAll() =>
            _ctx.Gameweeks
                .Include(g => g.Performances)
                    .ThenInclude(p => p.Player)
                .AsNoTracking()
                .ToList();

        public Gameweek? GetById(int id) =>
            _ctx.Gameweeks
                .Include(g => g.Performances)
                    .ThenInclude(p => p.Player)
                .Include(g => g.Fixtures)
                .AsNoTracking()
                .FirstOrDefault(g => g.Id == id);

        // Pretraga kola po broju (npr. "5" ili "kolo 5").
        public List<Gameweek> Search(string term, int take = 10)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<Gameweek>();

            // Izvuci prvi broj iz upita; ako ga nema, nema rezultata kola.
            var digits = new string(term.Where(char.IsDigit).ToArray());
            if (!int.TryParse(digits, out var weekNumber)) return new List<Gameweek>();

            return _ctx.Gameweeks
                .AsNoTracking()
                .Where(g => g.WeekNumber == weekNumber)
                .OrderBy(g => g.WeekNumber)
                .Take(take)
                .ToList();
        }
    }
}
