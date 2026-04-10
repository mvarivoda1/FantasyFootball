using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class PlayerController : Controller
    {
        private readonly PlayerMockRepository _playerRepo;

        public PlayerController(PlayerMockRepository playerRepo)
        {
            _playerRepo = playerRepo;
        }

        public IActionResult Index()
        {
            var players = _playerRepo.GetAll();
            return View(players);
        }

        public IActionResult Details(int id)
        {
            var player = _playerRepo.GetById(id);
            if (player == null) return NotFound();
            return View(player);
        }
    }
}
