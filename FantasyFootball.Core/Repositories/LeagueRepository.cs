using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Repositories
{
    public class LeagueRepository
    {
        private readonly FantasyFootballDbContext _ctx;

        public LeagueRepository(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        public List<League> GetAll() =>
            _ctx.Leagues
                .Include(l => l.Teams)
                .Include(l => l.Transfers)
                    .ThenInclude(tr => tr.Player)
                .AsNoTracking()
                .ToList();

        public League? GetById(int id) =>
            _ctx.Leagues
                .Include(l => l.Teams)
                    .ThenInclude(t => t.Players)
                .Include(l => l.Transfers)
                    .ThenInclude(tr => tr.Player)
                .Include(l => l.Transfers)
                    .ThenInclude(tr => tr.Team)
                .AsNoTracking()
                .FirstOrDefault(l => l.Id == id);

        // Pretraga liga po nazivu, sezoni, opisu ili šifri za pridruživanje.
        public List<League> Search(string term, int take = 10)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<League>();
            term = term.Trim();

            return _ctx.Leagues
                .Include(l => l.Teams)
                .AsNoTracking()
                .Where(l =>
                    l.Name.Contains(term) ||
                    l.Season.Contains(term) ||
                    l.Description.Contains(term) ||
                    l.JoinCode.Contains(term))
                .OrderBy(l => l.Name)
                .Take(take)
                .ToList();
        }
    }
}
