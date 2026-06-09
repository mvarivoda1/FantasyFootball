using System.Security.Claims;
using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers
{
    public class TransferController : Controller
    {
        private readonly TransferRepository _transferRepo;
        private readonly PlayerRepository _playerRepo;
        private readonly FantasyFootballDbContext _ctx;

        public TransferController(
            TransferRepository transferRepo,
            PlayerRepository playerRepo,
            FantasyFootballDbContext ctx)
        {
            _transferRepo = transferRepo;
            _playerRepo = playerRepo;
            _ctx = ctx;
        }

        // ----- Transfer tržište (teren + kupnja/prodaja) — glavni Transfers tab -----

        [HttpGet]
        [Route("transferi", Name = "TransferIndex")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");
            if (!user.FantasyTeamId.HasValue) return RedirectToAction("Build", "FantasyTeam");

            var team = await _ctx.FantasyTeams
                .Include(t => t.Players)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == user.FantasyTeamId.Value);
            if (team == null) return RedirectToAction("Build", "FantasyTeam");

            var vm = new TransferMarketViewModel
            {
                TeamId = team.Id,
                TeamName = team.Name,
                Budget = user.Budget,
                Squad = team.Players
                    .OrderBy(p => p.Position)
                    .ThenByDescending(p => p.TotalPoints)
                    .ToList(),
                AllPlayers = _playerRepo.GetAll()
                    .OrderBy(p => p.MarketValue)
                    .ToList(),
                RequiredGk = FantasyTeamController.RequiredGk,
                RequiredDef = FantasyTeamController.RequiredDef,
                RequiredMid = FantasyTeamController.RequiredMid,
                RequiredFwd = FantasyTeamController.RequiredFwd,
                SquadSize = FantasyTeamController.SquadSize,
                MaxPerClub = FantasyTeamController.MaxPerClub
            };

            return View(vm);
        }

        // ----- Batch potvrda transfera (više OUT + IN odjednom) -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(List<int> outIds, List<int> inIds)
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
                return RedirectToRoute("TransferIndex");
            }

            outIds = (outIds ?? new List<int>()).Distinct().ToList();
            inIds = (inIds ?? new List<int>()).Distinct().ToList();

            if (outIds.Count == 0 && inIds.Count == 0)
            {
                TempData["TransferError"] = "Niste odabrali nijedan transfer.";
                return RedirectToRoute("TransferIndex");
            }

            var team = user.FantasyTeam;
            var squadIds = team.Players.Select(p => p.Id).ToHashSet();

            // OUT igrači moraju biti u sastavu
            if (!outIds.All(id => squadIds.Contains(id)))
            {
                TempData["TransferError"] = "Neki igrači označeni za prodaju nisu u vašem timu.";
                return RedirectToRoute("TransferIndex");
            }

            // IN igrači moraju postojati, ne smiju već biti u sastavu i ne smiju se preklapati s OUT
            var inPlayers = await _ctx.Players.Where(p => inIds.Contains(p.Id)).ToListAsync();
            if (inPlayers.Count != inIds.Count)
            {
                TempData["TransferError"] = "Neki igrači označeni za kupnju ne postoje.";
                return RedirectToRoute("TransferIndex");
            }
            if (inIds.Any(id => squadIds.Contains(id)))
            {
                TempData["TransferError"] = "Igrača kojeg već imate u timu ne možete ponovno kupiti.";
                return RedirectToRoute("TransferIndex");
            }

            var outPlayers = team.Players.Where(p => outIds.Contains(p.Id)).ToList();
            var newSquad = team.Players.Where(p => !outIds.Contains(p.Id)).Concat(inPlayers).ToList();

            // Sastav mora ostati validan: 15 igrača, 2/5/5/3, max 3 iz kluba, budget >= 0
            if (newSquad.Count != FantasyTeamController.SquadSize)
            {
                TempData["TransferError"] = $"Tim mora imati točno {FantasyTeamController.SquadSize} igrača nakon transfera (svako prodano mjesto mora se popuniti).";
                return RedirectToRoute("TransferIndex");
            }

            int gk = newSquad.Count(p => p.Position == Position.Goalkeeper);
            int def = newSquad.Count(p => p.Position == Position.Defender);
            int mid = newSquad.Count(p => p.Position == Position.Midfielder);
            int fwd = newSquad.Count(p => p.Position == Position.Forward);
            if (gk != FantasyTeamController.RequiredGk || def != FantasyTeamController.RequiredDef ||
                mid != FantasyTeamController.RequiredMid || fwd != FantasyTeamController.RequiredFwd)
            {
                TempData["TransferError"] =
                    $"Sastav mora ostati {FantasyTeamController.RequiredGk} GK / {FantasyTeamController.RequiredDef} DEF / " +
                    $"{FantasyTeamController.RequiredMid} MID / {FantasyTeamController.RequiredFwd} FWD (trenutno: {gk}/{def}/{mid}/{fwd}).";
                return RedirectToRoute("TransferIndex");
            }

            var overClub = newSquad
                .GroupBy(p => p.Club)
                .Where(g => g.Count() > FantasyTeamController.MaxPerClub)
                .Select(g => $"{g.Key} ({g.Count()})")
                .ToList();
            if (overClub.Any())
            {
                TempData["TransferError"] = $"Maksimalno {FantasyTeamController.MaxPerClub} igrača iz istog kluba. Prekoračeno: {string.Join(", ", overClub)}.";
                return RedirectToRoute("TransferIndex");
            }

            double outSum = outPlayers.Sum(p => p.MarketValue);
            double inSum = inPlayers.Sum(p => p.MarketValue);
            double budgetAfter = user.Budget + outSum - inSum;
            if (budgetAfter < 0)
            {
                TempData["TransferError"] = $"Nedovoljno sredstava — nedostaje €{(-budgetAfter):N1}M.";
                return RedirectToRoute("TransferIndex");
            }

            var now = DateTime.UtcNow;
            foreach (var p in outPlayers)
            {
                team.Players.Remove(p);
                _ctx.Transfers.Add(new Transfer
                {
                    PlayerId = p.Id,
                    TeamId = team.Id,
                    Direction = TransferDirection.Out,
                    TransferDate = now,
                    Price = p.MarketValue,
                    LeagueId = team.LeagueId
                });
            }
            foreach (var p in inPlayers)
            {
                team.Players.Add(p);
                _ctx.Transfers.Add(new Transfer
                {
                    PlayerId = p.Id,
                    TeamId = team.Id,
                    Direction = TransferDirection.In,
                    TransferDate = now,
                    Price = p.MarketValue,
                    LeagueId = team.LeagueId
                });
            }

            team.SquadValue = newSquad.Sum(p => p.MarketValue);
            user.Budget = budgetAfter;

            // Ako je prodan igrač bio u startnoj postavi, resetiraj lineup (My Team će složiti novi default)
            if (!string.IsNullOrWhiteSpace(team.StartingLineupIds))
            {
                var lineup = team.StartingLineupIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var v) ? v : 0)
                    .Where(v => v > 0)
                    .ToList();
                if (lineup.Any(id => outIds.Contains(id)))
                    team.StartingLineupIds = null;
            }

            await _ctx.SaveChangesAsync();
            TempData["TransferSuccess"] =
                $"Transfer dovršen: {outPlayers.Count} OUT, {inPlayers.Count} IN. Novi budget: €{budgetAfter:N1}M.";
            return RedirectToRoute("TransferIndex");
        }

        // ----- Statistika transfera (trendovi) — kasnije samo za admin rolu -----

        [HttpGet]
        [Route("transferi/statistika", Name = "TransferStats")]
        public IActionResult Stats()
        {
            var transfers = _transferRepo.GetAll();
            return View(transfers);
        }

        public IActionResult Details(int id)
        {
            var transfer = _transferRepo.GetById(id);
            if (transfer == null) return NotFound();
            return View(transfer);
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
