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
    }
}
