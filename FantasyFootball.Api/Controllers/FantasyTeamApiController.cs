using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers.Api
{
    [Route("api/fantasyteam")]
    [ApiController]
    public class FantasyTeamApiController : ControllerBase
    {
        private readonly FantasyFootballDbContext _ctx;

        public FantasyTeamApiController(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET /api/fantasyteam  (opcionalna pretraga: ?q=...)
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<FantasyTeamDTO>> GetAll([FromQuery] string? q = null)
        {
            var query = _ctx.FantasyTeams
                .Include(t => t.League)
                .Include(t => t.Players)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(t => t.Name.Contains(term) || t.OwnerName.Contains(term));
            }

            var teams = query.OrderBy(t => t.Name).ToList().Select(ToDTO).ToList();
            return Ok(teams);
        }

        // GET /api/fantasyteam/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<FantasyTeamDTO> GetById(int id)
        {
            var team = _ctx.FantasyTeams
                .Include(t => t.League)
                .Include(t => t.Players)
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
            if (team == null) return NotFound();
            return Ok(ToDTO(team));
        }

        // POST /api/fantasyteam
        [HttpPost]
        [Authorize]
        public ActionResult<FantasyTeamDTO> Create([FromBody] FantasyTeamInputDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.LeagueId.HasValue && !_ctx.Leagues.Any(l => l.Id == model.LeagueId.Value))
                return BadRequest($"Liga s ID-em {model.LeagueId} ne postoji.");

            var team = new FantasyTeam
            {
                Name = model.Name.Trim(),
                OwnerName = model.OwnerName.Trim(),
                CreatedAt = DateTime.UtcNow,
                LeagueId = model.LeagueId,
                SquadValue = 0,
                TotalPoints = 0
            };

            _ctx.FantasyTeams.Add(team);
            _ctx.SaveChanges();

            var created = _ctx.FantasyTeams.Include(t => t.League).Include(t => t.Players).First(t => t.Id == team.Id);
            return CreatedAtAction(nameof(GetById), new { id = team.Id }, ToDTO(created));
        }

        // PUT /api/fantasyteam/5
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<FantasyTeamDTO> Update(int id, [FromBody] FantasyTeamInputDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var team = _ctx.FantasyTeams.Include(t => t.League).Include(t => t.Players).FirstOrDefault(t => t.Id == id);
            if (team == null) return NotFound();

            if (model.LeagueId.HasValue && !_ctx.Leagues.Any(l => l.Id == model.LeagueId.Value))
                return BadRequest($"Liga s ID-em {model.LeagueId} ne postoji.");

            team.Name = model.Name.Trim();
            team.OwnerName = model.OwnerName.Trim();
            team.LeagueId = model.LeagueId;

            _ctx.SaveChanges();
            return Ok(ToDTO(team));
        }

        // DELETE /api/fantasyteam/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var team = _ctx.FantasyTeams.Include(t => t.Players).FirstOrDefault(t => t.Id == id);
            if (team == null) return NotFound();

            // Odveži korisnika (1-1) ako postoji
            var owner = _ctx.Users.FirstOrDefault(u => u.FantasyTeamId == id);
            if (owner != null) owner.FantasyTeamId = null;

            team.Players.Clear();
            _ctx.FantasyTeams.Remove(team);
            _ctx.SaveChanges();
            return NoContent();
        }

        private static FantasyTeamDTO ToDTO(FantasyTeam t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            OwnerName = t.OwnerName,
            CreatedAt = t.CreatedAt,
            SquadValue = t.SquadValue,
            TotalPoints = t.TotalPoints,
            League = t.League == null ? null : new LeagueDTO
            {
                Id = t.League.Id,
                Name = t.League.Name,
                Season = t.League.Season,
                MaxTeams = t.League.MaxTeams,
                Description = t.League.Description,
                JoinCode = t.League.JoinCode
            },
            Players = (t.Players ?? new List<Player>())
                .OrderBy(p => p.Position)
                .Select(p => new PlayerDTO
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
                })
                .ToList()
        };
    }
}
