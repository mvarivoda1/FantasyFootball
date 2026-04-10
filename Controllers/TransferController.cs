using FantasyFootball.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FantasyFootball.Controllers
{
    public class TransferController : Controller
    {
        private readonly TransferMockRepository _transferRepo;

        public TransferController(TransferMockRepository transferRepo)
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
