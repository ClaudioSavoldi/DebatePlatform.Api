using DebatePlatform.Api.Controllers.Request;
using DebatePlatform.Api.Controllers.Response;
using DebatePlatform.Api.Domain.Entities;
using DebatePlatform.Api.Domain.Enums;
using DebatePlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DebatePlatform.Api.Controllers
{
    [Route("api/debates/{debateId:guid}")]
    [ApiController]
    public class DebateQueueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DebateQueueController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/debates/{debateId}/join
        [HttpPost("join")]
        [Authorize]
        public async Task<IActionResult> Join(Guid debateId, [FromBody] JoinQueueRequest request)
        {
            // 0) UserId dal JWT
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Token non valido.");

            // 1) Il topic deve esistere ed essere APPROVED (pubblico)
            var debateStatus = await _context.Debates
                .AsNoTracking()
                .Where(d => d.Id == debateId)
                .Select(d => (DebateStatus?)d.Status)
                .FirstOrDefaultAsync();

            if (debateStatus is null) return NotFound("Topic inesistente.");
            if (debateStatus != DebateStatus.Approved)
                return BadRequest("Non puoi iscriverti: il topic non è approvato.");

            // 2) Regola: l’utente non può mettersi in coda due volte nello stesso topic
            var alreadyQueued = await _context.DebateQueueEntries
                .AsNoTracking()
                .AnyAsync(q => q.DebateId == debateId && q.UserId == userId);

            if (alreadyQueued)
                return Conflict("Sei già in coda per questo topic.");

            // 3) Regola: evita che l’utente partecipi già a un match attivo su questo topic
            var alreadyInMatch = await _context.DebateMatches
                .AsNoTracking()
                .AnyAsync(m =>
                    m.DebateId == debateId &&
                    m.Phase != MatchPhase.Closed &&
                    (m.ProUserId == userId || m.ControUserId == userId));

            if (alreadyInMatch)
                return Conflict("Stai già partecipando a un match attivo su questo topic.");

            // 4) Transaction: metto in coda e provo ad abbinare FIFO
            await using var tx = await _context.Database.BeginTransactionAsync();

            // Inserisco in coda
            var entry = new DebateQueueEntry
            {
                Id = Guid.NewGuid(),
                DebateId = debateId,
                UserId = userId,
                Side = request.Side,
                JoinedAt = DateTime.UtcNow
            };

            _context.DebateQueueEntries.Add(entry);

            // Salvo subito: così l’entry esiste “davvero” in DB prima di matchare
            await _context.SaveChangesAsync();

            // Cerco il primo avversario in attesa sul lato opposto (FIFO)
            var opponentSide = request.Side == MatchSide.Pro ? MatchSide.Contro : MatchSide.Pro;

            var opponent = await _context.DebateQueueEntries
                .Where(q => q.DebateId == debateId && q.Side == opponentSide)
                .OrderBy(q => q.JoinedAt)
                .FirstOrDefaultAsync();

            if (opponent is null)
            {
                await tx.CommitAsync();
                return Ok(new JoinQueueResponse
                {
                    Matched = false,
                    MatchId = null,
                    Message = "Sei in attesa: nessun avversario disponibile al momento."
                });
            }

            // 5) Creo match
            var match = new DebateMatch
            {
                Id = Guid.NewGuid(),
                DebateId = debateId,
                Phase = MatchPhase.Opening,
                ProUserId = request.Side == MatchSide.Pro ? userId : opponent.UserId,
                ControUserId = request.Side == MatchSide.Contro ? userId : opponent.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.DebateMatches.Add(match);

            // 6) Tolgo entrambi dalla coda
            _context.DebateQueueEntries.Remove(entry);
            _context.DebateQueueEntries.Remove(opponent);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new JoinQueueResponse
            {
                Matched = true,
                MatchId = match.Id,
                Message = "Match trovato! Puoi iniziare la fase Opening."
            });
        }
    }
}
