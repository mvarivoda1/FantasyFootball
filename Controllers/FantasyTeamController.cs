using System.Security.Claims;
using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers
{
    public class FantasyTeamController : Controller
    {
        public const double InitialBudget = 100.0;
        public const int RequiredGk = 2;
        public const int RequiredDef = 5;
        public const int RequiredMid = 5;
        public const int RequiredFwd = 3;
        public const int SquadSize = RequiredGk + RequiredDef + RequiredMid + RequiredFwd;
        public const int MaxPerClub = 3;

        private readonly FantasyFootballDbContext _ctx;
        private readonly FantasyTeamRepository _teamRepo;
        private readonly PlayerRepository _playerRepo;

        public FantasyTeamController(
            FantasyFootballDbContext ctx,
            FantasyTeamRepository teamRepo,
            PlayerRepository playerRepo)
        {
            _ctx = ctx;
            _teamRepo = teamRepo;
            _playerRepo = playerRepo;
        }

        public IActionResult Index()
        {
            var teams = _teamRepo.GetAll();
            return View(teams);
        }

        public IActionResult Details(int id)
        {
            var team = _teamRepo.GetById(id);
            if (team == null) return NotFound();
            return View(team);
        }

        [HttpGet]
        public async Task<IActionResult> Build()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");
            if (user.FantasyTeamId.HasValue) return RedirectToAction("Index", "Home");

            var vm = BuildEmptyViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Build(BuildFantasyTeamViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");
            if (user.FantasyTeamId.HasValue) return RedirectToAction("Index", "Home");

            var selectedIds = (model.SelectedPlayerIds ?? new List<int>()).Distinct().ToList();

            if (selectedIds.Count != SquadSize)
                ModelState.AddModelError(string.Empty, $"Tim mora imati točno {SquadSize} igrača (odabrano: {selectedIds.Count}).");

            var players = _ctx.Players.Where(p => selectedIds.Contains(p.Id)).ToList();
            if (players.Count != selectedIds.Count)
                ModelState.AddModelError(string.Empty, "Neki odabrani igrači ne postoje u bazi.");

            var byPos = players.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.Count());
            int gk = byPos.GetValueOrDefault(Position.Goalkeeper);
            int def = byPos.GetValueOrDefault(Position.Defender);
            int mid = byPos.GetValueOrDefault(Position.Midfielder);
            int fwd = byPos.GetValueOrDefault(Position.Forward);

            if (gk != RequiredGk || def != RequiredDef || mid != RequiredMid || fwd != RequiredFwd)
            {
                ModelState.AddModelError(string.Empty,
                    $"Formacija mora biti {RequiredGk} GK / {RequiredDef} DEF / {RequiredMid} MID / {RequiredFwd} FWD (trenutno: {gk}/{def}/{mid}/{fwd}).");
            }

            var totalCost = players.Sum(p => p.MarketValue);
            if (totalCost > InitialBudget)
                ModelState.AddModelError(string.Empty,
                    $"Vrijednost momčadi ({totalCost:F1}M) prelazi budžet ({InitialBudget:F1}M).");

            var overClub = players
                .GroupBy(p => p.Club)
                .Where(g => g.Count() > MaxPerClub)
                .Select(g => $"{g.Key} ({g.Count()})")
                .ToList();
            if (overClub.Any())
                ModelState.AddModelError(string.Empty,
                    $"Maksimalno {MaxPerClub} igrača iz istog kluba. Prekoračeno: {string.Join(", ", overClub)}.");

            if (!ModelState.IsValid)
            {
                var vm = BuildEmptyViewModel();
                vm.TeamName = model.TeamName;
                vm.SelectedPlayerIds = selectedIds;
                return View(vm);
            }

            var team = new FantasyTeam
            {
                Name = model.TeamName.Trim(),
                OwnerName = user.Email,
                CreatedAt = DateTime.UtcNow,
                SquadValue = totalCost,
                TotalPoints = 0,
                Players = players
            };

            _ctx.FantasyTeams.Add(team);
            await _ctx.SaveChangesAsync();

            user.FantasyTeamId = team.Id;
            user.Budget = InitialBudget - totalCost;
            await _ctx.SaveChangesAsync();

            await ResignInWithTeamClaimAsync(user);

            return RedirectToAction("Index", "Home");
        }

        private BuildFantasyTeamViewModel BuildEmptyViewModel() => new()
        {
            AvailablePlayers = _playerRepo.GetAll().OrderBy(p => p.MarketValue).ToList(),
            Budget = InitialBudget,
            RequiredGk = RequiredGk,
            RequiredDef = RequiredDef,
            RequiredMid = RequiredMid,
            RequiredFwd = RequiredFwd,
            MaxPerClub = MaxPerClub
        };

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }

        private async Task ResignInWithTeamClaimAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Email),
            };
            if (user.FantasyTeamId.HasValue)
                claims.Add(new Claim("FantasyTeamId", user.FantasyTeamId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
        }
    }
}
