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
                .AsNoTracking()
                .FirstOrDefault(g => g.Id == id);
    }
}
