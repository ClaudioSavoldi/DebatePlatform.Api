using DebatePlatform.Api.Controllers.Request;
using DebatePlatform.Api.Domain.Entities;
using DebatePlatform.Api.Domain.Enums;
using DebatePlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DebatePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/matches/{matchId:guid}/votes")]
    public class VotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VotesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/matches/{matchId}/votes
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CastVote(Guid matchId, [FromBody] CastVoteRequest request)
        {
            // 1) userId dal JWT
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Token non valido.");

            // 2) match esiste?
            var match = await _context.DebateMatches
                .AsNoTracking()
                .Where(m => m.Id == matchId)
                .Select(m => new { m.Id, m.Phase, m.ProUserId, m.ControUserId })
                .FirstOrDefaultAsync();

            if (match is null)
                return NotFound("Match inesistente.");

            // 3) regola: si vota solo quando il match è in Voting
            if (match.Phase != MatchPhase.Voting)
                return BadRequest("Non puoi votare: il match non è in fase Voting.");

            // 4) anti-cheat: i partecipanti NON possono votare
            if (match.ProUserId == userId || match.ControUserId == userId)
                return Forbid();

            // 5) già votato?
            var alreadyVoted = await _context.Votes
                .AsNoTracking()
                .AnyAsync(v => v.MatchId == matchId && v.UserId == userId);

            if (alreadyVoted)
                return Conflict("Hai già votato per questo match.");

            // 6) salva voto
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                UserId = userId,
                Value = request.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                vote.Id,
                vote.MatchId,
                vote.Value,
                vote.CreatedAt
            });
        }
    }
}

