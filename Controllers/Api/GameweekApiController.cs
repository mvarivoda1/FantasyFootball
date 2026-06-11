using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers.Api
{
    [Route("api/gameweek")]
    [ApiController]
    public class GameweekApiController : ControllerBase
    {
        private readonly FantasyFootballDbContext _ctx;

        public GameweekApiController(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET /api/gameweek  (opcionalna pretraga po broju kola: ?q=5)
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<GameweekDTO>> GetAll([FromQuery] string? q = null)
        {
            var query = _ctx.Gameweeks.Include(g => g.Performances).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q) && int.TryParse(q.Trim(), out var num))
                query = query.Where(g => g.WeekNumber == num);

            var gameweeks = query.OrderBy(g => g.WeekNumber).ToList().Select(ToDTO).ToList();
            return Ok(gameweeks);
        }

        // GET /api/gameweek/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<GameweekDTO> GetById(int id)
        {
            var gw = _ctx.Gameweeks.Include(g => g.Performances).AsNoTracking().FirstOrDefault(g => g.Id == id);
            if (gw == null) return NotFound();
            return Ok(ToDTO(gw));
        }

        // POST /api/gameweek
        [HttpPost]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public ActionResult<GameweekDTO> Create([FromBody] Gameweek model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_ctx.Gameweeks.Any(g => g.WeekNumber == model.WeekNumber))
                return Conflict($"Kolo broj {model.WeekNumber} već postoji.");

            var gw = new Gameweek
            {
                WeekNumber = model.WeekNumber,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            _ctx.Gameweeks.Add(gw);
            _ctx.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = gw.Id }, ToDTO(gw));
        }

        // PUT /api/gameweek/5
        [HttpPut("{id}")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public ActionResult<GameweekDTO> Update(int id, [FromBody] Gameweek model)
        {
            if (id != model.Id) return BadRequest("ID u ruti i tijelu zahtjeva se ne podudaraju.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gw = _ctx.Gameweeks.FirstOrDefault(g => g.Id == id);
            if (gw == null) return NotFound();

            if (model.WeekNumber != gw.WeekNumber &&
                _ctx.Gameweeks.Any(g => g.WeekNumber == model.WeekNumber && g.Id != id))
                return Conflict($"Kolo broj {model.WeekNumber} već postoji.");

            gw.WeekNumber = model.WeekNumber;
            gw.StartDate = model.StartDate;
            gw.EndDate = model.EndDate;

            _ctx.SaveChanges();
            return Ok(ToDTO(gw));
        }

        // DELETE /api/gameweek/5
        [HttpDelete("{id}")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public IActionResult Delete(int id)
        {
            var gw = _ctx.Gameweeks.Include(g => g.Performances).FirstOrDefault(g => g.Id == id);
            if (gw == null) return NotFound();

            if (gw.Performances.Any())
                return Conflict($"Kolo {gw.WeekNumber} ima povezane utakmice i ne može se obrisati.");

            _ctx.Gameweeks.Remove(gw);
            _ctx.SaveChanges();
            return NoContent();
        }

        private static GameweekDTO ToDTO(Gameweek g) => new()
        {
            Id = g.Id,
            WeekNumber = g.WeekNumber,
            StartDate = g.StartDate,
            EndDate = g.EndDate,
            PerformancesCount = g.Performances?.Count ?? 0
        };
    }
}
