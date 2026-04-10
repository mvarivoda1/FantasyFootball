using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class FantasyTeamController : Controller
    {
        private readonly FantasyTeamMockRepository _teamRepo;

        public FantasyTeamController(FantasyTeamMockRepository teamRepo)
        {
            _teamRepo = teamRepo;
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
    }
}
