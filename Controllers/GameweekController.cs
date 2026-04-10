using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class GameweekController : Controller
    {
        private readonly GameweekMockRepository _gameweekRepo;

        public GameweekController(GameweekMockRepository gameweekRepo)
        {
            _gameweekRepo = gameweekRepo;
        }

        public IActionResult Index()
        {
            var gameweeks = _gameweekRepo.GetAll();
            return View(gameweeks);
        }

        public IActionResult Details(int id)
        {
            var gameweek = _gameweekRepo.GetById(id);
            if (gameweek == null) return NotFound();
            return View(gameweek);
        }
    }
}
