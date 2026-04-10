using FantasyFootball.Models;
using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FantasyFootball.Controllers
{
    public class HomeController : Controller
    {
        private readonly PlayerMockRepository _playerRepo;
        private readonly LeagueMockRepository _leagueRepo;
        private readonly FantasyTeamMockRepository _teamRepo;
        private readonly GameweekMockRepository _gameweekRepo;

        public HomeController(
            PlayerMockRepository playerRepo,
            LeagueMockRepository leagueRepo,
            FantasyTeamMockRepository teamRepo,
            GameweekMockRepository gameweekRepo)
        {
            _playerRepo = playerRepo;
            _leagueRepo = leagueRepo;
            _teamRepo = teamRepo;
            _gameweekRepo = gameweekRepo;
        }

        public IActionResult Index()
        {
            ViewBag.TopPlayers = _playerRepo.GetAll().OrderByDescending(p => p.TotalPoints).Take(5).ToList();
            ViewBag.Leagues = _leagueRepo.GetAll();
            ViewBag.Teams = _teamRepo.GetAll().OrderByDescending(t => t.TotalPoints).Take(5).ToList();
            ViewBag.LatestGameweek = _gameweekRepo.GetAll().OrderByDescending(g => g.WeekNumber).FirstOrDefault();
            ViewBag.TotalPlayers = _playerRepo.GetAll().Count;
            ViewBag.TotalTeams = _teamRepo.GetAll().Count;
            ViewBag.TotalLeagues = _leagueRepo.GetAll().Count;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
