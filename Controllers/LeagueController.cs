using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class LeagueController : Controller
    {
        private readonly LeagueMockRepository _leagueRepo;

        public LeagueController(LeagueMockRepository leagueRepo)
        {
            _leagueRepo = leagueRepo;
        }

        public IActionResult Index()
        {
            var leagues = _leagueRepo.GetAll();
            return View(leagues);
        }

        public IActionResult Details(int id)
        {
            var league = _leagueRepo.GetById(id);
            if (league == null) return NotFound();
            return View(league);
        }
    }
}
