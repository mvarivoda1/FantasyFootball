using System.Security.Claims;
using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers
{
    public class PlayerController : Controller
    {
        private readonly PlayerRepository _playerRepo;
        private readonly FantasyFootballDbContext _ctx;

        public PlayerController(PlayerRepository playerRepo, FantasyFootballDbContext ctx)
        {
            _playerRepo = playerRepo;
            _ctx = ctx;
        }

        [Route("igraci", Name = "PlayerIndex")]
        public IActionResult Index()
        {
            var players = _playerRepo.GetAll();
            ViewData["CurrentBudget"] = GetCurrentUserBudget();
            ViewData["CurrentTeamId"] = GetCurrentUserTeamId();
            return View(players);
        }

        [Route("igrac/{id:int}", Name = "PlayerDetails")]
        public IActionResult Details(int id)
        {
            var player = _playerRepo.GetById(id);
            if (player == null) return NotFound();
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _ctx.Users
                .Include(u => u.FantasyTeam)
                    .ThenInclude(t => t!.Players)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user?.FantasyTeam == null)
            {
                TempData["TransferError"] = "Nemate fantasy tim.";
                return RedirectToRoute("PlayerIndex");
            }

            var player = await _ctx.Players.FirstOrDefaultAsync(p => p.Id == id);
            if (player == null) return NotFound();

            if (user.FantasyTeam.Players.Any(p => p.Id == player.Id))
            {
                TempData["TransferError"] = $"{player.FullName} je već u vašem timu.";
                return RedirectToRoute("PlayerIndex");
            }

            if (user.Budget < player.MarketValue)
            {
                TempData["TransferError"] = $"Nedovoljno sredstava za {player.FullName} (€{player.MarketValue:N0}).";
                return RedirectToRoute("PlayerIndex");
            }

            user.Budget -= player.MarketValue;
            user.FantasyTeam.Players.Add(player);
            user.FantasyTeam.SquadValue += player.MarketValue;

            _ctx.Transfers.Add(new Transfer
            {
                PlayerId = player.Id,
                TeamId = user.FantasyTeam.Id,
                Direction = TransferDirection.In,
                TransferDate = DateTime.UtcNow,
                Price = player.MarketValue,
                LeagueId = user.FantasyTeam.LeagueId
            });

            await _ctx.SaveChangesAsync();
            TempData["TransferSuccess"] = $"Kupili ste {player.FullName} za €{player.MarketValue:N0}.";
            return RedirectToRoute("PlayerIndex");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _ctx.Users
                .Include(u => u.FantasyTeam)
                    .ThenInclude(t => t!.Players)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user?.FantasyTeam == null)
            {
                TempData["TransferError"] = "Nemate fantasy tim.";
                return RedirectToRoute("PlayerIndex");
            }

            var player = user.FantasyTeam.Players.FirstOrDefault(p => p.Id == id);
            if (player == null)
            {
                TempData["TransferError"] = "Taj igrač nije u vašem timu.";
                return RedirectToRoute("PlayerIndex");
            }

            user.Budget += player.MarketValue;
            user.FantasyTeam.Players.Remove(player);
            user.FantasyTeam.SquadValue -= player.MarketValue;

            _ctx.Transfers.Add(new Transfer
            {
                PlayerId = player.Id,
                TeamId = user.FantasyTeam.Id,
                Direction = TransferDirection.Out,
                TransferDate = DateTime.UtcNow,
                Price = player.MarketValue,
                LeagueId = user.FantasyTeam.LeagueId
            });

            await _ctx.SaveChangesAsync();
            TempData["TransferSuccess"] = $"Prodali ste {player.FullName} za €{player.MarketValue:N0}.";
            return RedirectToRoute("PlayerIndex");
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private int? GetCurrentUserTeamId()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;
            return _ctx.Users.AsNoTracking()
                .Where(u => u.Id == userId.Value)
                .Select(u => u.FantasyTeamId)
                .FirstOrDefault();
        }

        private double? GetCurrentUserBudget()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;
            return _ctx.Users.AsNoTracking()
                .Where(u => u.Id == userId.Value)
                .Select(u => (double?)u.Budget)
                .FirstOrDefault();
        }
    }
}
