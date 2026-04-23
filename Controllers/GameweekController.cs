using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class GameweekController : Controller
    {
        private readonly GameweekRepository _gameweekRepo;

        public GameweekController(GameweekRepository gameweekRepo)
        {
            _gameweekRepo = gameweekRepo;
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
            return View(gameweek);
        }
    }
}
