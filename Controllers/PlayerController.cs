using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Authorization;
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
            return View(players);
        }

        [Route("igrac/{id:int}", Name = "PlayerDetails")]
        public IActionResult Details(int id)
        {
            var player = _playerRepo.GetById(id);
            if (player == null) return NotFound();
            return View(player);
        }

        // ----- CRUD (samo Admin) -----

        [HttpGet]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public IActionResult Create()
        {
            return View(new PlayerFormViewModel());
        }

        [HttpPost]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlayerFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var player = new Player
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Position = model.Position,
                Club = model.Club.Trim(),
                Nationality = model.Nationality.Trim(),
                DateOfBirth = model.DateOfBirth,
                MarketValue = model.MarketValue,
                Goals = model.Goals,
                Assists = model.Assists,
                CleanSheets = model.CleanSheets,
                TotalPoints = model.TotalPoints
            };

            _ctx.Players.Add(player);
            await _ctx.SaveChangesAsync();

            TempData["PlayerInfo"] = $"Igrač {player.FullName} je dodan.";
            return RedirectToRoute("PlayerIndex");
        }

        [HttpGet]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var player = await _ctx.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (player == null) return NotFound();

            var vm = new PlayerFormViewModel
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Position = player.Position,
                Club = player.Club,
                Nationality = player.Nationality,
                DateOfBirth = player.DateOfBirth,
                MarketValue = player.MarketValue,
                Goals = player.Goals,
                Assists = player.Assists,
                CleanSheets = player.CleanSheets,
                TotalPoints = player.TotalPoints
            };
            return View(vm);
        }

        [HttpPost, ActionName("Edit")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var player = await _ctx.Players.FirstOrDefaultAsync(p => p.Id == id);
            if (player == null) return NotFound();

            var vm = new PlayerFormViewModel { Id = player.Id };
            var ok = await TryUpdateModelAsync(vm, string.Empty,
                v => v.FirstName, v => v.LastName, v => v.Position,
                v => v.Club, v => v.Nationality, v => v.DateOfBirth,
                v => v.MarketValue, v => v.Goals, v => v.Assists,
                v => v.CleanSheets, v => v.TotalPoints);

            if (!ok || !ModelState.IsValid)
                return View(vm);

            player.FirstName = vm.FirstName.Trim();
            player.LastName = vm.LastName.Trim();
            player.Position = vm.Position;
            player.Club = vm.Club.Trim();
            player.Nationality = vm.Nationality.Trim();
            player.DateOfBirth = vm.DateOfBirth;
            player.MarketValue = vm.MarketValue;
            player.Goals = vm.Goals;
            player.Assists = vm.Assists;
            player.CleanSheets = vm.CleanSheets;
            player.TotalPoints = vm.TotalPoints;

            await _ctx.SaveChangesAsync();
            TempData["PlayerInfo"] = $"Igrač {player.FullName} je ažuriran.";
            return RedirectToAction(nameof(Details), new { id = player.Id });
        }

        [HttpGet]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var player = await _ctx.Players
                .Include(p => p.FantasyTeams)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (player == null) return NotFound();
            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _ctx.Players
                .Include(p => p.FantasyTeams)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (player == null) return NotFound();

            if (player.FantasyTeams.Any())
            {
                TempData["PlayerError"] = $"Nije moguće obrisati {player.FullName} — igrač je u {player.FantasyTeams.Count} timova.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _ctx.Players.Remove(player);
            await _ctx.SaveChangesAsync();

            TempData["PlayerInfo"] = $"Igrač {player.FullName} je obrisan.";
            return RedirectToRoute("PlayerIndex");
        }

        // AJAX pretraga igrača — autocomplete + Index live search
        [HttpGet]
        [Route("Player/Search")]
        public async Task<IActionResult> Search(string? q, string? position, string? club, int take = 10)
        {
            take = Math.Clamp(take, 1, 50);
            var query = _ctx.Players.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p =>
                    p.FirstName.Contains(term) ||
                    p.LastName.Contains(term) ||
                    p.Club.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(position) &&
                Enum.TryParse<Position>(position, true, out var pos))
            {
                query = query.Where(p => p.Position == pos);
            }

            if (!string.IsNullOrWhiteSpace(club))
                query = query.Where(p => p.Club == club);

            var results = await query
                .OrderBy(p => p.LastName)
                .Take(take)
                .Select(p => new
                {
                    id = p.Id,
                    fullName = p.FirstName + " " + p.LastName,
                    club = p.Club,
                    position = p.Position.ToString(),
                    marketValue = p.MarketValue,
                    totalPoints = p.TotalPoints
                })
                .ToListAsync();

            return Json(results);
        }

        // AJAX dohvat liste klubova — za autocomplete u Player Create/Edit
        [HttpGet]
        [Route("Player/Clubs")]
        public async Task<IActionResult> Clubs(string? q, int take = 10)
        {
            take = Math.Clamp(take, 1, 30);
            var query = _ctx.Players.AsNoTracking().Select(p => p.Club).Distinct();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c => c.Contains(term));
            }

            var results = await query
                .OrderBy(c => c)
                .Take(take)
                .Select(c => new { id = c, label = c })
                .ToListAsync();

            return Json(results);
        }
    }
}
