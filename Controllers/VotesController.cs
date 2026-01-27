using DebatePlatform.Api.Controllers.Request;
using DebatePlatform.Api.Domain.Entities;
using DebatePlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DebatePlatform.Api.Controllers
{
    [ApiController]
    [Route("api/debates/{debateId:guid}/votes")]
    public class VotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VotesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/debates/{debateId}/votes
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CastVote(Guid debateId, [FromBody] CastVoteRequest request)
        {
            // userId dal token (ASP.NET standard)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Token non valido: NameIdentifier mancante.");

            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Token non valido: userId non è un Guid.");

            // debate esiste?
            var debateExists = await _context.Debates.AnyAsync(d => d.Id == debateId);
            if (!debateExists)
                return NotFound("Dibattito inesistente.");

            // già votato?
            var alreadyVoted = await _context.Votes.AnyAsync(v => v.DebateId == debateId && v.UserId == userId);
            if (alreadyVoted)
                return Conflict("Hai già votato per questo dibattito.");

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                DebateId = debateId,
                UserId = userId,
                Value = request.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Votes.Add(vote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Protezione extra: vincolo unico (DebateId, UserId)
                return Conflict("Hai già votato per questo dibattito.");
            }

            return Ok(new
            {
                vote.Id,
                vote.DebateId,
                vote.UserId,
                vote.Value,
                vote.CreatedAt
            });
        }
    }
}
