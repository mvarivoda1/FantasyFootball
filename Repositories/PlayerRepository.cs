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
    }
}
