using System.Security.Cryptography;
using FantasyFootball.DAL;
using FantasyFootball.Models;
using FantasyFootball.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FantasyFootball.Controllers.Api
{
    [Route("api/league")]
    [ApiController]
    public class LeagueApiController : ControllerBase
    {
        private const string JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int JoinCodeLength = 6;

        private readonly FantasyFootballDbContext _ctx;

        public LeagueApiController(FantasyFootballDbContext ctx)
        {
            _ctx = ctx;
        }

        // GET /api/league  (opcionalna pretraga: ?q=...)
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<LeagueDTO>> GetAll([FromQuery] string? q = null)
        {
            var query = _ctx.Leagues.Include(l => l.Teams).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(l => l.Name.Contains(term) || l.JoinCode.Contains(term));
            }

            var leagues = query.OrderBy(l => l.Name).ToList().Select(ToDTO).ToList();
            return Ok(leagues);
        }

        // GET /api/league/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<LeagueDTO> GetById(int id)
        {
            var league = _ctx.Leagues.Include(l => l.Teams).AsNoTracking().FirstOrDefault(l => l.Id == id);
            if (league == null) return NotFound();
            return Ok(ToDTO(league));
        }

        // POST /api/league
        [HttpPost]
        [Authorize]
        public ActionResult<LeagueDTO> Create([FromBody] LeagueInputDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var now = DateTime.UtcNow;
            var league = new League
            {
                Name = model.Name.Trim(),
                Season = string.IsNullOrWhiteSpace(model.Season) ? $"{now.Year}/{now.Year + 1}" : model.Season.Trim(),
                MaxTeams = model.MaxTeams,
                Description = model.Description?.Trim() ?? string.Empty,
                CreatedAt = now,
                JoinCode = GenerateUniqueJoinCode()
            };

            _ctx.Leagues.Add(league);
            _ctx.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = league.Id }, ToDTO(league));
        }

        // PUT /api/league/5
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<LeagueDTO> Update(int id, [FromBody] LeagueInputDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var league = _ctx.Leagues.Include(l => l.Teams).FirstOrDefault(l => l.Id == id);
            if (league == null) return NotFound();

            if (model.MaxTeams < league.Teams.Count)
                return BadRequest($"Liga već ima {league.Teams.Count} timova — maksimum ne može biti manji.");

            league.Name = model.Name.Trim();
            league.MaxTeams = model.MaxTeams;
            league.Description = model.Description?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(model.Season)) league.Season = model.Season.Trim();

            _ctx.SaveChanges();
            return Ok(ToDTO(league));
        }

        // DELETE /api/league/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var league = _ctx.Leagues
                .Include(l => l.Teams)
                .Include(l => l.Transfers)
                .FirstOrDefault(l => l.Id == id);
            if (league == null) return NotFound();

            foreach (var team in league.Teams) team.LeagueId = null;
            foreach (var transfer in league.Transfers) transfer.LeagueId = null;

            _ctx.Leagues.Remove(league);
            _ctx.SaveChanges();
            return NoContent();
        }

        private string GenerateUniqueJoinCode()
        {
            for (int attempt = 0; attempt < 25; attempt++)
            {
                var code = GenerateCode();
                if (!_ctx.Leagues.Any(l => l.JoinCode == code)) return code;
            }
            throw new InvalidOperationException("Nije moguće generirati jedinstvenu šifru lige.");
        }

        private static string GenerateCode()
        {
            var chars = new char[JoinCodeLength];
            var bytes = new byte[JoinCodeLength];
            RandomNumberGenerator.Fill(bytes);
            for (int i = 0; i < JoinCodeLength; i++)
                chars[i] = JoinCodeAlphabet[bytes[i] % JoinCodeAlphabet.Length];
            return new string(chars);
        }

        private static LeagueDTO ToDTO(League l) => new()
        {
            Id = l.Id,
            Name = l.Name,
            Season = l.Season,
            MaxTeams = l.MaxTeams,
            Description = l.Description,
            JoinCode = l.JoinCode,
            TeamsCount = l.Teams?.Count ?? 0
        };
    }
}
