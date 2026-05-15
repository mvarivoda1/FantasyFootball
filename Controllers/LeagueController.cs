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
            return View(leagues);
        }

        [Route("liga/{id:int}", Name = "LeagueDetails")]
        public IActionResult Details(int id)
        {
            var league = _leagueRepo.GetById(id);
            if (league == null) return NotFound();
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

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
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

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
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

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
