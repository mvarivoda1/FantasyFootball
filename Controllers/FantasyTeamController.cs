using System.Security.Claims;
using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Identity;
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

        public const int StartingXISize = 11;
        public const int BenchSize = SquadSize - StartingXISize;
        public const int MinStartGk = 1;
        public const int MinStartDef = 3;
        public const int MinStartMid = 2;
        public const int MinStartFwd = 1;
        public const int MaxStartGk = 1;

        private readonly FantasyFootballDbContext _ctx;
        private readonly FantasyTeamRepository _teamRepo;
        private readonly PlayerRepository _playerRepo;
        private readonly SignInManager<AppUser> _signInManager;

        public FantasyTeamController(
            FantasyFootballDbContext ctx,
            FantasyTeamRepository teamRepo,
            PlayerRepository playerRepo,
            SignInManager<AppUser> signInManager)
        {
            _ctx = ctx;
            _teamRepo = teamRepo;
            _playerRepo = playerRepo;
            _signInManager = signInManager;
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

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
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

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
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
                OwnerName = user.Email ?? user.UserName ?? string.Empty,
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

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> MyTeam()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");
            if (!user.FantasyTeamId.HasValue) return RedirectToAction("Build");

            var team = await _ctx.FantasyTeams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == user.FantasyTeamId.Value);
            if (team == null) return RedirectToAction("Build");

            var allPlayers = team.Players.ToList();
            var starterIds = ParseLineupIds(team.StartingLineupIds);

            List<Player> starters;
            List<Player> bench;
            if (starterIds.Count == StartingXISize && starterIds.All(id => allPlayers.Any(p => p.Id == id)))
            {
                starters = starterIds.Select(id => allPlayers.First(p => p.Id == id)).ToList();
                bench = allPlayers.Where(p => !starterIds.Contains(p.Id)).ToList();
            }
            else
            {
                (starters, bench) = BuildDefaultLineup(allPlayers);
            }

            var vm = BuildMyTeamViewModel(team, starters, bench);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLineup(int teamId, List<int> starterIds)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");
            if (user.FantasyTeamId != teamId) return Forbid();

            var team = await _ctx.FantasyTeams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) return NotFound();

            var distinct = (starterIds ?? new List<int>()).Distinct().ToList();
            var squadIds = team.Players.Select(p => p.Id).ToHashSet();

            if (distinct.Count != StartingXISize)
                ModelState.AddModelError(string.Empty, $"Početni sastav mora imati točno {StartingXISize} igrača.");

            if (!distinct.All(id => squadIds.Contains(id)))
                ModelState.AddModelError(string.Empty, "Svi odabrani igrači moraju biti dio tvoje momčadi.");

            var selected = team.Players.Where(p => distinct.Contains(p.Id)).ToList();
            var gk = selected.Count(p => p.Position == Position.Goalkeeper);
            var def = selected.Count(p => p.Position == Position.Defender);
            var mid = selected.Count(p => p.Position == Position.Midfielder);
            var fwd = selected.Count(p => p.Position == Position.Forward);

            if (gk != MaxStartGk)
                ModelState.AddModelError(string.Empty, $"Početni sastav mora imati točno {MaxStartGk} vratara (trenutno: {gk}).");
            if (def < MinStartDef)
                ModelState.AddModelError(string.Empty, $"Početni sastav mora imati najmanje {MinStartDef} obrambena igrača (trenutno: {def}).");
            if (mid < MinStartMid)
                ModelState.AddModelError(string.Empty, $"Početni sastav mora imati najmanje {MinStartMid} vezna igrača (trenutno: {mid}).");
            if (fwd < MinStartFwd)
                ModelState.AddModelError(string.Empty, $"Početni sastav mora imati najmanje {MinStartFwd} napadača (trenutno: {fwd}).");

            if (!ModelState.IsValid)
            {
                var startersFallback = team.Players.Where(p => distinct.Contains(p.Id)).ToList();
                var benchFallback = team.Players.Where(p => !distinct.Contains(p.Id)).ToList();
                if (startersFallback.Count != StartingXISize)
                    (startersFallback, benchFallback) = BuildDefaultLineup(team.Players.ToList());

                var vm = BuildMyTeamViewModel(team, startersFallback, benchFallback);
                return View(nameof(MyTeam), vm);
            }

            team.StartingLineupIds = string.Join(",", distinct);
            await _ctx.SaveChangesAsync();

            TempData["LineupSaved"] = "Početni sastav uspješno spremljen.";
            return RedirectToAction(nameof(MyTeam));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var team = await _ctx.FantasyTeams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return NotFound();

            // Samo vlasnik smije uređivati tim
            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.FantasyTeamId != team.Id) return Forbid();

            var vm = new EditFantasyTeamViewModel
            {
                Id = team.Id,
                Name = team.Name,
                OwnerName = team.OwnerName
            };
            return View(vm);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var team = await _ctx.FantasyTeams.FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return NotFound();

            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.FantasyTeamId != team.Id) return Forbid();

            var vm = new EditFantasyTeamViewModel { Id = team.Id };
            var ok = await TryUpdateModelAsync(vm, string.Empty,
                v => v.Name, v => v.OwnerName);

            if (!ok || !ModelState.IsValid)
                return View(vm);

            team.Name = vm.Name.Trim();
            team.OwnerName = vm.OwnerName.Trim();

            await _ctx.SaveChangesAsync();
            TempData["TeamInfo"] = $"Tim '{team.Name}' je uspješno ažuriran.";
            return RedirectToAction(nameof(MyTeam));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var team = await _ctx.FantasyTeams
                .Include(t => t.Players)
                .Include(t => t.League)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return NotFound();

            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.FantasyTeamId != team.Id) return Forbid();

            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var team = await _ctx.FantasyTeams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (team == null) return NotFound();

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user?.FantasyTeamId != team.Id) return Forbid();

            team.Players.Clear();
            user.FantasyTeamId = null;
            user.Budget = InitialBudget;

            _ctx.FantasyTeams.Remove(team);
            await _ctx.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);

            TempData["TeamInfo"] = $"Tim '{team.Name}' je obrisan. Možeš kreirati novi tim.";
            return RedirectToAction(nameof(Build));
        }

        private static List<int> ParseLineupIds(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new List<int>();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }

        private static (List<Player> starters, List<Player> bench) BuildDefaultLineup(List<Player> squad)
        {
            var byPos = squad
                .GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.TotalPoints).ThenByDescending(p => p.MarketValue).ToList());

            List<Player> Pick(Position pos, int n)
            {
                if (!byPos.TryGetValue(pos, out var list)) return new List<Player>();
                return list.Take(n).ToList();
            }

            // Default formacija 1-4-4-2
            var starters = new List<Player>();
            starters.AddRange(Pick(Position.Goalkeeper, 1));
            starters.AddRange(Pick(Position.Defender, 4));
            starters.AddRange(Pick(Position.Midfielder, 4));
            starters.AddRange(Pick(Position.Forward, 2));

            var starterIds = starters.Select(p => p.Id).ToHashSet();
            var bench = squad.Where(p => !starterIds.Contains(p.Id)).ToList();
            return (starters, bench);
        }

        private static MyTeamViewModel BuildMyTeamViewModel(FantasyTeam team, List<Player> starters, List<Player> bench) => new()
        {
            TeamId = team.Id,
            TeamName = team.Name,
            Starters = starters,
            Bench = bench,
            MinGk = MinStartGk,
            MinDef = MinStartDef,
            MinMid = MinStartMid,
            MinFwd = MinStartFwd,
            MaxGk = MaxStartGk,
            MaxDef = RequiredDef,
            MaxMid = RequiredMid,
            MaxFwd = RequiredFwd
        };

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

        private string? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(raw) ? null : raw;
        }

        // ===== Upload datoteka (Dropzone) — vezano uz fantasy tim =====

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".txt" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        // Provjerava je li trenutni korisnik vlasnik zadanog tima.
        private async Task<bool> IsOwnerAsync(int teamId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return false;
            var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            return user?.FantasyTeamId == teamId;
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(int teamId, IFormFile file)
        {
            if (!await IsOwnerAsync(teamId)) return Forbid();

            var team = await _ctx.FantasyTeams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("Datoteka je prazna.");
            if (file.Length > MaxFileSize)
                return BadRequest("Datoteka je prevelika (maksimalno 5 MB).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest("Nedozvoljen tip datoteke.");

            var uploadsPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "uploads", "teams", teamId.ToString());
            Directory.CreateDirectory(uploadsPath);

            var storedName = Guid.NewGuid().ToString() + ext;
            var fullPath = Path.Combine(uploadsPath, storedName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new Attachment
            {
                FantasyTeamId = teamId,
                FileName = Path.GetFileName(file.FileName),
                FilePath = $"/uploads/teams/{teamId}/{storedName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow
            };

            _ctx.Attachments.Add(attachment);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachments(int teamId)
        {
            if (!await IsOwnerAsync(teamId)) return Forbid();

            var attachments = await _ctx.Attachments
                .Where(a => a.FantasyTeamId == teamId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return PartialView("_AttachmentList", attachments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _ctx.Attachments.FirstOrDefaultAsync(a => a.Id == id);
            if (attachment == null) return NotFound();

            if (!await IsOwnerAsync(attachment.FantasyTeamId)) return Forbid();

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _ctx.Attachments.Remove(attachment);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
