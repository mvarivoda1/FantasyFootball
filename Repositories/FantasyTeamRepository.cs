using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Repositories
{
    public class FantasyTeamRepository
    {
        private readonly FantasyFootballDbContext _ctx;

        public FantasyTeamRepository(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        public List<FantasyTeam> GetAll() =>
            _ctx.FantasyTeams
                .Include(t => t.League)
                .Include(t => t.Players)
                .AsNoTracking()
                .ToList();

        public FantasyTeam? GetById(int id) =>
            _ctx.FantasyTeams
                .Include(t => t.League)
                .Include(t => t.Players)
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
    }
}
