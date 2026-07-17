using System.Security.Claims;
using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers
{
    public class LeagueController : Controller
    {
        private const string JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int JoinCodeLength = 6;

        private readonly LeagueRepository _leagueRepo;
        private readonly FantasyFootballDbContext _ctx;

        public LeagueController(LeagueRepository leagueRepo, FantasyFootballDbContext ctx)
        {
            _leagueRepo = leagueRepo;
            _ctx = ctx;
        }

        [Route("lige", Name = "LeagueIndex")]
        public IActionResult Index()
        {
            var leagues = _leagueRepo.GetAll();
            ViewData["CurrentUserId"] = GetCurrentUserId();
            return View(leagues);
        }

        [Route("liga/{id:int}", Name = "LeagueDetails")]
        public IActionResult Details(int id)
        {
            var league = _leagueRepo.GetById(id);
            if (league == null) return NotFound();
            var userId = GetCurrentUserId();
            ViewData["CurrentUserId"] = userId;
            ViewData["IsCreator"] = !string.IsNullOrEmpty(league.CreatorUserId) && league.CreatorUserId == userId;
            return View(league);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateLeagueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLeagueViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.FantasyTeamId == null)
                return RedirectToAction("Build", "FantasyTeam");

            var joinCode = await GenerateUniqueJoinCodeAsync();
            var now = DateTime.UtcNow;
            var season = $"{now.Year}/{now.Year + 1}";

            var league = new League
            {
                Name = model.Name.Trim(),
                MaxTeams = model.MaxTeams,
                Season = season,
                Description = string.Empty,
                CreatedAt = now,
                JoinCode = joinCode,
                CreatorUserId = user.Id
            };

            _ctx.Leagues.Add(league);
            await _ctx.SaveChangesAsync();

            // Kreator automatski dodaje vlastiti tim u ligu
            var team = await _ctx.FantasyTeams.FirstOrDefaultAsync(t => t.Id == user.FantasyTeamId!.Value);
            if (team != null)
            {
                team.LeagueId = league.Id;
                await _ctx.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Created), new { id = league.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Created(int id)
        {
            var league = await _ctx.Leagues
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
            if (league == null) return NotFound();

            var userId = GetCurrentUserId();
            if (league.CreatorUserId != userId)
                return RedirectToAction(nameof(Details), new { id = league.Id });

            return View(league);
        }

        [HttpGet]
        public IActionResult Join()
        {
            return View(new JoinLeagueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(JoinLeagueViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.FantasyTeamId == null)
                return RedirectToAction("Build", "FantasyTeam");

            var code = model.JoinCode.Trim().ToUpperInvariant();
            var league = await _ctx.Leagues
                .Include(l => l.Teams)
                .FirstOrDefaultAsync(l => l.JoinCode == code);

            if (league == null)
            {
                ModelState.AddModelError(nameof(model.JoinCode), "Liga s tom šifrom ne postoji.");
                return View(model);
            }

            var team = await _ctx.FantasyTeams.FirstOrDefaultAsync(t => t.Id == user.FantasyTeamId!.Value);
            if (team == null)
                return RedirectToAction("Build", "FantasyTeam");

            if (team.LeagueId == league.Id)
            {
                TempData["JoinInfo"] = $"Već si član lige '{league.Name}'.";
                return RedirectToAction(nameof(Details), new { id = league.Id });
            }

            if (league.Teams.Count >= league.MaxTeams)
            {
                ModelState.AddModelError(nameof(model.JoinCode), $"Liga '{league.Name}' je puna ({league.MaxTeams}/{league.MaxTeams}).");
                return View(model);
            }

            team.LeagueId = league.Id;
            await _ctx.SaveChangesAsync();

            TempData["JoinInfo"] = $"Uspješno si se pridružio ligi '{league.Name}'.";
            return RedirectToAction(nameof(Details), new { id = league.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var league = await _ctx.Leagues.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (league == null) return NotFound();

            var userId = GetCurrentUserId();
            if (league.CreatorUserId != userId) return Forbid();

            var vm = new EditLeagueViewModel
            {
                Id = league.Id,
                Name = league.Name,
                MaxTeams = league.MaxTeams,
                Description = league.Description,
                Season = league.Season,
                JoinCode = league.JoinCode,
                TeamsCount = await _ctx.FantasyTeams.CountAsync(t => t.LeagueId == league.Id)
            };
            return View(vm);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var league = await _ctx.Leagues.FirstOrDefaultAsync(l => l.Id == id);
            if (league == null) return NotFound();

            var userId = GetCurrentUserId();
            if (league.CreatorUserId != userId) return Forbid();

            var teamsCount = await _ctx.FantasyTeams.CountAsync(t => t.LeagueId == league.Id);

            // Premapiraj samo dopuštena polja kroz ViewModel + TryUpdateModel pattern
            var vm = new EditLeagueViewModel
            {
                Id = league.Id,
                Season = league.Season,
                JoinCode = league.JoinCode,
                TeamsCount = teamsCount,
                Name = league.Name,
                MaxTeams = league.MaxTeams,
                Description = league.Description
            };

            var ok = await TryUpdateModelAsync(vm, string.Empty,
                v => v.Name, v => v.MaxTeams, v => v.Description);

            if (!ok || !ModelState.IsValid)
                return View(vm);

            if (vm.MaxTeams < teamsCount)
            {
                ModelState.AddModelError(nameof(vm.MaxTeams),
                    $"Liga već ima {teamsCount} timova — maksimum ne može biti manji od toga.");
                return View(vm);
            }

            league.Name = vm.Name.Trim();
            league.MaxTeams = vm.MaxTeams;
            league.Description = vm.Description?.Trim() ?? string.Empty;

            await _ctx.SaveChangesAsync();
            TempData["LeagueInfo"] = $"Liga '{league.Name}' je uspješno ažurirana.";
            return RedirectToAction(nameof(Details), new { id = league.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var league = await _ctx.Leagues
                .Include(l => l.Teams)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
            if (league == null) return NotFound();

            var userId = GetCurrentUserId();
            if (league.CreatorUserId != userId) return Forbid();

            return View(league);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var league = await _ctx.Leagues
                .Include(l => l.Teams)
                .Include(l => l.Transfers)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (league == null) return NotFound();

            var userId = GetCurrentUserId();
            if (league.CreatorUserId != userId) return Forbid();

            // Odveži timove i transfere od lige (set null) prije brisanja
            foreach (var team in league.Teams)
                team.LeagueId = null;
            foreach (var transfer in league.Transfers)
                transfer.LeagueId = null;

            _ctx.Leagues.Remove(league);
            await _ctx.SaveChangesAsync();

            TempData["LeagueInfo"] = $"Liga '{league.Name}' je obrisana.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX pretraga liga po nazivu — za autocomplete i live search na Index-u
        [HttpGet]
        [Route("League/Search")]
        public async Task<IActionResult> Search(string? q, int take = 10)
        {
            take = Math.Clamp(take, 1, 50);
            var query = _ctx.Leagues.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(l => l.Name.Contains(term) || l.JoinCode.Contains(term));
            }

            var results = await query
                .OrderBy(l => l.Name)
                .Take(take)
                .Select(l => new
                {
                    id = l.Id,
                    name = l.Name,
                    season = l.Season,
                    joinCode = l.JoinCode,
                    maxTeams = l.MaxTeams,
                    teamsCount = _ctx.FantasyTeams.Count(t => t.LeagueId == l.Id)
                })
                .ToListAsync();

            return Json(results);
        }

        private async Task<string> GenerateUniqueJoinCodeAsync()
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var code = GenerateCode();
                var exists = await _ctx.Leagues.AnyAsync(l => l.JoinCode == code);
                if (!exists) return code;
            }
            throw new InvalidOperationException("Nije moguće generirati jedinstvenu šifru lige.");
        }

        private static string GenerateCode()
        {
            var chars = new char[JoinCodeLength];
            var bytes = new byte[JoinCodeLength];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            for (int i = 0; i < JoinCodeLength; i++)
                chars[i] = JoinCodeAlphabet[bytes[i] % JoinCodeAlphabet.Length];
            return new string(chars);
        }

        private string? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(raw) ? null : raw;
        }
    }
}
