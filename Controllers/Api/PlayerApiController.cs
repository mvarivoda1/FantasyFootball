using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers.Api
{
    [Route("api/player")]
    [ApiController]
    public class PlayerApiController : ControllerBase
    {
        private readonly FantasyFootballDbContext _ctx;

        public PlayerApiController(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET /api/player  (opcionalna pretraga: /api/player?q=...)
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<PlayerDTO>> GetAll([FromQuery] string? q = null)
        {
            var query = _ctx.Players.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p =>
                    p.FirstName.Contains(term) ||
                    p.LastName.Contains(term) ||
                    p.Club.Contains(term) ||
                    p.Nationality.Contains(term));
            }

            var players = query.OrderBy(p => p.LastName).ToList().Select(ToDTO).ToList();
            return Ok(players);
        }

        // GET /api/player/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<PlayerDTO> GetById(int id)
        {
            var player = _ctx.Players.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (player == null) return NotFound();
            return Ok(ToDTO(player));
        }

        // POST /api/player
        [HttpPost]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public ActionResult<PlayerDTO> Create([FromBody] Player model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var player = new Player
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Position = model.Position,
                Club = model.Club,
                Nationality = model.Nationality,
                DateOfBirth = model.DateOfBirth,
                MarketValue = model.MarketValue,
                Goals = model.Goals,
                Assists = model.Assists,
                CleanSheets = model.CleanSheets,
                TotalPoints = model.TotalPoints
            };

            _ctx.Players.Add(player);
            _ctx.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = player.Id }, ToDTO(player));
        }

        // PUT /api/player/5
        [HttpPut("{id}")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public ActionResult<PlayerDTO> Update(int id, [FromBody] Player model)
        {
            if (id != model.Id) return BadRequest("ID u ruti i tijelu zahtjeva se ne podudaraju.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var player = _ctx.Players.FirstOrDefault(p => p.Id == id);
            if (player == null) return NotFound();

            player.FirstName = model.FirstName;
            player.LastName = model.LastName;
            player.Position = model.Position;
            player.Club = model.Club;
            player.Nationality = model.Nationality;
            player.DateOfBirth = model.DateOfBirth;
            player.MarketValue = model.MarketValue;
            player.Goals = model.Goals;
            player.Assists = model.Assists;
            player.CleanSheets = model.CleanSheets;
            player.TotalPoints = model.TotalPoints;

            _ctx.SaveChanges();
            return Ok(ToDTO(player));
        }

        // DELETE /api/player/5
        [HttpDelete("{id}")]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public IActionResult Delete(int id)
        {
            var player = _ctx.Players.Include(p => p.FantasyTeams).FirstOrDefault(p => p.Id == id);
            if (player == null) return NotFound();

            if (player.FantasyTeams.Any())
                return Conflict($"Igrač je u {player.FantasyTeams.Count} timova i ne može se obrisati.");

            _ctx.Players.Remove(player);
            _ctx.SaveChanges();
            return NoContent();
        }

        private static PlayerDTO ToDTO(Player p) => new()
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            FullName = p.FullName,
            Position = p.Position.ToString(),
            Club = p.Club,
            Nationality = p.Nationality,
            DateOfBirth = p.DateOfBirth,
            MarketValue = p.MarketValue,
            Goals = p.Goals,
            Assists = p.Assists,
            CleanSheets = p.CleanSheets,
            TotalPoints = p.TotalPoints
        };
    }
}
