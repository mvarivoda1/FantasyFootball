using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class LeagueController : Controller
    {
        private readonly LeagueRepository _leagueRepo;

        public LeagueController(LeagueRepository leagueRepo)
        {
            _leagueRepo = leagueRepo;
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
    }
}
