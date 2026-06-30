using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.ViewModels;
using FantasyFootball.Repositories;
using FantasyFootball.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers
{
    public class GameweekController : Controller
    {
        private readonly GameweekRepository _gameweekRepo;
        private readonly FantasyFootballDbContext _ctx;
        private readonly GameweekSimulationService _sim;

        public GameweekController(
            GameweekRepository gameweekRepo,
            FantasyFootballDbContext ctx,
            GameweekSimulationService sim)
        {
            _gameweekRepo = gameweekRepo;
            _ctx = ctx;
            _sim = sim;
        }

        [Route("kola", Name = "GameweekIndex")]
        public IActionResult Index()
        {
            var gameweeks = _gameweekRepo.GetAll();
            return View(gameweeks);
        }

        [Route("kolo/{id:int}", Name = "GameweekDetails")]
        public IActionResult Details(int id)
        {
            var gameweek = _gameweekRepo.GetById(id);
            if (gameweek == null) return NotFound();

            var vm = new GameweekDetailsViewModel
            {
                Gameweek = gameweek,
                Fixtures = gameweek.Fixtures
                    .OrderBy(f => f.HomeClub)
                    .ToList(),
                CanSimulate = User.IsInRole(DbSeeder.AdminRole) && gameweek.Fixtures.Count == 0
            };
            return View(vm);
        }

        // ===== Simulacija rezultata kola (admin) =====

        // Generira nasumične rezultate i prikazuje ih za pregled — ništa se ne sprema.
        [HttpPost]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simulate(int id)
        {
            var gw = await _ctx.Gameweeks.Include(g => g.Fixtures)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (gw == null) return NotFound();

            if (gw.Fixtures.Any())
            {
                TempData["GameweekError"] = $"Kolo {gw.WeekNumber} je već odigrano.";
                return RedirectToAction(nameof(Details), new { id });
            }

            int seed = Random.Shared.Next(1, int.MaxValue);
            var draft = await _sim.PreviewAsync(id, seed);

            var vm = new SimulatePreviewViewModel
            {
                GameweekId = id,
                WeekNumber = gw.WeekNumber,
                Seed = seed,
                Draft = draft
            };
            return View("SimulatePreview", vm);
        }

        // Potvrđuje pregledani rezultat (po seedu) — sprema učinke i bodove.
        [HttpPost]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, int seed)
        {
            var gw = await _ctx.Gameweeks.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (gw == null) return NotFound();

            await _sim.ConfirmAsync(id, seed);

            TempData["GameweekInfo"] = $"Kolo {gw.WeekNumber} je simulirano i potvrđeno.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public IActionResult Create()
        {
            var nextWeekNumber = (_ctx.Gameweeks.Max(g => (int?)g.WeekNumber) ?? 0) + 1;
            return View(new GameweekFormViewModel
            {
                WeekNumber = nextWeekNumber,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(8)
            });
        }

        [HttpPost]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameweekFormViewModel model)
        {
            if (await _ctx.Gameweeks.AnyAsync(g => g.WeekNumber == model.WeekNumber))
            {
                ModelState.AddModelError(nameof(model.WeekNumber),
                    $"Kolo broj {model.WeekNumber} već postoji.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var gw = new Gameweek
            {
                WeekNumber = model.WeekNumber,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            _ctx.Gameweeks.Add(gw);
            await _ctx.SaveChangesAsync();

            TempData["GameweekInfo"] = $"Kolo {gw.WeekNumber} je kreirano.";
            return RedirectToRoute("GameweekIndex");
        }

        [HttpGet]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var gw = await _ctx.Gameweeks.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (gw == null) return NotFound();

            var vm = new GameweekFormViewModel
            {
                Id = gw.Id,
                WeekNumber = gw.WeekNumber,
                StartDate = gw.StartDate,
                EndDate = gw.EndDate
            };
            return View(vm);
        }

        [HttpPost, ActionName("Edit")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var gw = await _ctx.Gameweeks.FirstOrDefaultAsync(g => g.Id == id);
            if (gw == null) return NotFound();

            var vm = new GameweekFormViewModel { Id = gw.Id };
            var ok = await TryUpdateModelAsync(vm, string.Empty,
                v => v.WeekNumber, v => v.StartDate, v => v.EndDate);

            if (ok && vm.WeekNumber != gw.WeekNumber &&
                await _ctx.Gameweeks.AnyAsync(g => g.WeekNumber == vm.WeekNumber && g.Id != id))
            {
                ModelState.AddModelError(nameof(vm.WeekNumber),
                    $"Kolo broj {vm.WeekNumber} već postoji.");
            }

            if (!ok || !ModelState.IsValid)
                return View(vm);

            gw.WeekNumber = vm.WeekNumber;
            gw.StartDate = vm.StartDate;
            gw.EndDate = vm.EndDate;

            await _ctx.SaveChangesAsync();
            TempData["GameweekInfo"] = $"Kolo {gw.WeekNumber} je ažurirano.";
            return RedirectToAction(nameof(Details), new { id = gw.Id });
        }

        [HttpGet]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var gw = await _ctx.Gameweeks
                .Include(g => g.Performances)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
            if (gw == null) return NotFound();
            return View(gw);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gw = await _ctx.Gameweeks.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
            if (gw == null) return NotFound();

            // Brisanje poništava sve bodove kola (igrači + timovi).
            await _sim.DeleteWithReversalAsync(id);

            TempData["GameweekInfo"] = $"Kolo {gw.WeekNumber} je obrisano i bodovi su poništeni.";
            return RedirectToRoute("GameweekIndex");
        }

        // AJAX pretraga kola — po broju ili rasponu datuma
        [HttpGet]
        [Route("Gameweek/Search")]
        public async Task<IActionResult> Search(string? q, int take = 20)
        {
            take = Math.Clamp(take, 1, 50);
            var query = _ctx.Gameweeks.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                if (int.TryParse(term, out var num))
                    query = query.Where(g => g.WeekNumber == num);
            }

            var results = await query
                .OrderBy(g => g.WeekNumber)
                .Take(take)
                .Select(g => new
                {
                    id = g.Id,
                    weekNumber = g.WeekNumber,
                    startDate = g.StartDate,
                    endDate = g.EndDate,
                    performancesCount = g.Performances.Count
                })
                .ToListAsync();

            return Json(results);
        }
    }
}
