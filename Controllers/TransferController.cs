using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class TransferController : Controller
    {
        private readonly TransferRepository _transferRepo;

        public TransferController(TransferRepository transferRepo)
        {
            _transferRepo = transferRepo;
        }

        public IActionResult Index()
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
    }
}
