using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers.Api
{
    [Route("api/transfer")]
    [ApiController]
    public class TransferApiController : ControllerBase
    {
        private readonly FantasyFootballDbContext _ctx;

        public TransferApiController(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET /api/transfer  (opcionalna pretraga po imenu igrača/tima: ?q=...)
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<TransferDTO>> GetAll([FromQuery] string? q = null)
        {
            var query = _ctx.Transfers
                .Include(t => t.Player)
                .Include(t => t.Team)
                .Include(t => t.League)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(t =>
                    t.Player.FirstName.Contains(term) ||
                    t.Player.LastName.Contains(term) ||
                    t.Team.Name.Contains(term));
            }

            var transfers = query.OrderByDescending(t => t.TransferDate).ToList().Select(ToDTO).ToList();
            return Ok(transfers);
        }

        // GET /api/transfer/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<TransferDTO> GetById(int id)
        {
            var transfer = _ctx.Transfers
                .Include(t => t.Player)
                .Include(t => t.Team)
                .Include(t => t.League)
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
            if (transfer == null) return NotFound();
            return Ok(ToDTO(transfer));
        }

        // POST /api/transfer
        [HttpPost]
        [Authorize]
        public ActionResult<TransferDTO> Create([FromBody] TransferInputDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_ctx.Players.Any(p => p.Id == model.PlayerId))
                return BadRequest($"Igrač s ID-em {model.PlayerId} ne postoji.");
            if (!_ctx.FantasyTeams.Any(t => t.Id == model.TeamId))
                return BadRequest($"Tim s ID-em {model.TeamId} ne postoji.");
            if (model.LeagueId.HasValue && !_ctx.Leagues.Any(l => l.Id == model.LeagueId.Value))
                return BadRequest($"Liga s ID-em {model.LeagueId} ne postoji.");

            var transfer = new Transfer
            {
                PlayerId = model.PlayerId,
                TeamId = model.TeamId,
                Direction = model.Direction,
                Price = model.Price,
                LeagueId = model.LeagueId,
                TransferDate = DateTime.UtcNow
            };

            _ctx.Transfers.Add(transfer);
            _ctx.SaveChanges();

            var created = _ctx.Transfers
                .Include(t => t.Player).Include(t => t.Team).Include(t => t.League)
                .First(t => t.Id == transfer.Id);
            return CreatedAtAction(nameof(GetById), new { id = transfer.Id }, ToDTO(created));
        }

        // PUT /api/transfer/5
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<TransferDTO> Update(int id, [FromBody] TransferInputDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var transfer = _ctx.Transfers.FirstOrDefault(t => t.Id == id);
            if (transfer == null) return NotFound();

            if (!_ctx.Players.Any(p => p.Id == model.PlayerId))
                return BadRequest($"Igrač s ID-em {model.PlayerId} ne postoji.");
            if (!_ctx.FantasyTeams.Any(t => t.Id == model.TeamId))
                return BadRequest($"Tim s ID-em {model.TeamId} ne postoji.");

            transfer.PlayerId = model.PlayerId;
            transfer.TeamId = model.TeamId;
            transfer.Direction = model.Direction;
            transfer.Price = model.Price;
            transfer.LeagueId = model.LeagueId;

            _ctx.SaveChanges();

            var updated = _ctx.Transfers
                .Include(t => t.Player).Include(t => t.Team).Include(t => t.League)
                .First(t => t.Id == id);
            return Ok(ToDTO(updated));
        }

        // DELETE /api/transfer/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var transfer = _ctx.Transfers.FirstOrDefault(t => t.Id == id);
            if (transfer == null) return NotFound();

            _ctx.Transfers.Remove(transfer);
            _ctx.SaveChanges();
            return NoContent();
        }

        private static TransferDTO ToDTO(Transfer t) => new()
        {
            Id = t.Id,
            Direction = t.Direction.ToString(),
            TransferDate = t.TransferDate,
            Price = t.Price,
            PlayerId = t.PlayerId,
            PlayerName = t.Player == null ? string.Empty : t.Player.FullName,
            TeamId = t.TeamId,
            TeamName = t.Team?.Name ?? string.Empty,
            LeagueId = t.LeagueId,
            LeagueName = t.League?.Name
        };
    }
}
