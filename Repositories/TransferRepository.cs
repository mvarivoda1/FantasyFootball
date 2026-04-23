using FantasyFootball.DAL;
using FantasyFootball.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Repositories
{
    public class TransferRepository
    {
        private readonly FantasyFootballDbContext _ctx;

        public TransferRepository(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        public List<Transfer> GetAll() =>
            _ctx.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromTeam)
                .Include(t => t.ToTeam)
                .Include(t => t.League)
                .AsNoTracking()
                .ToList();

        public Transfer? GetById(int id) =>
            _ctx.Transfers
                .Include(t => t.Player)
                .Include(t => t.FromTeam)
                .Include(t => t.ToTeam)
                .Include(t => t.League)
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
    }
}
