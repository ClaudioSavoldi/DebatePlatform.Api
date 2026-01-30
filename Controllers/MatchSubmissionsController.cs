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
    [Route("api/matches/{matchId:guid}/submissions")]
    [ApiController]
    public class MatchSubmissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchSubmissionsController(AppDbContext context)
        {
            _context = context;
        }

        private bool TryGetUserId(out Guid userId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out userId);
        }

        private static string NormalizeBody(string body) => body.Trim();

        // ---------------------------
        // OPENING: DRAFT
        // PUT: api/matches/{matchId}/submissions/opening/draft
        // ---------------------------
        [HttpPut("opening/draft")]
        [Authorize]
        public async Task<IActionResult> SaveOpeningDraft(Guid matchId, [FromBody] SubmitOpeningRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token non valido.");

            var match = await _context.DebateMatches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match is null) return NotFound("Match inesistente.");

            if (match.Phase != MatchPhase.Opening)
                return BadRequest("Non puoi modificare l'opening: il match non è in fase Opening.");

            var isParticipant = match.ProUserId == userId || match.ControUserId == userId;
            if (!isParticipant) return Forbid();

            var submission = await _context.MatchSubmissions
                .FirstOrDefaultAsync(s =>
                    s.MatchId == matchId &&
                    s.UserId == userId &&
                    s.Phase == SubmissionPhase.Opening);

            if (submission is null)
            {
                submission = new MatchSubmission
                {
                    Id = Guid.NewGuid(),
                    MatchId = matchId,
                    UserId = userId,
                    Phase = SubmissionPhase.Opening,
                    Body = NormalizeBody(request.Body),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSubmitted = false,
                    SubmittedAt = null
                };

                _context.MatchSubmissions.Add(submission);
            }
            else
            {
                if (submission.IsSubmitted)
                    return Conflict("Opening già consegnato: non puoi modificarlo.");

                submission.Body = NormalizeBody(request.Body);
                submission.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                submission.Id,
                submission.MatchId,
                submission.Phase,
                submission.IsSubmitted,
                submission.UpdatedAt
            });
        }

        // ---------------------------
        // OPENING: SUBMIT
        // POST: api/matches/{matchId}/submissions/opening/submit
        // ---------------------------
        [HttpPost("opening/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitOpening(Guid matchId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token non valido.");

            var match = await _context.DebateMatches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match is null) return NotFound("Match inesistente.");

            if (match.Phase != MatchPhase.Opening)
                return BadRequest("Non puoi consegnare l'opening: il match non è in fase Opening.");

            var isParticipant = match.ProUserId == userId || match.ControUserId == userId;
            if (!isParticipant) return Forbid();

            var submission = await _context.MatchSubmissions
                .FirstOrDefaultAsync(s =>
                    s.MatchId == matchId &&
                    s.UserId == userId &&
                    s.Phase == SubmissionPhase.Opening);

            if (submission is null)
                return BadRequest("Nessuna bozza opening trovata: salva una draft prima di consegnare.");

            if (submission.IsSubmitted)
                return Conflict("Opening già consegnato.");

            submission.IsSubmitted = true;
            submission.SubmittedAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            // Salvo prima la consegna
            await _context.SaveChangesAsync();

            // Se entrambi hanno consegnato => avanza fase
            var submittedCount = await _context.MatchSubmissions
                .AsNoTracking()
                .CountAsync(s =>
                    s.MatchId == matchId &&
                    s.Phase == SubmissionPhase.Opening &&
                    s.IsSubmitted);

            if (submittedCount >= 2)
            {
                match.Phase = MatchPhase.Rebuttal;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                submission.Id,
                submission.MatchId,
                submission.Phase,
                submission.IsSubmitted,
                submission.SubmittedAt,
                MatchPhase = match.Phase
            });
        }

        // ---------------------------
        // REBUTTAL: DRAFT
        // PUT: api/matches/{matchId}/submissions/rebuttal/draft
        // ---------------------------
        [HttpPut("rebuttal/draft")]
        [Authorize]
        public async Task<IActionResult> SaveRebuttalDraft(Guid matchId, [FromBody] SubmitRebuttalRequest request)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token non valido.");

            var match = await _context.DebateMatches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match is null) return NotFound("Match inesistente.");

            if (match.Phase != MatchPhase.Rebuttal)
                return BadRequest("Non puoi modificare il rebuttal: il match non è in fase Rebuttal.");

            var isParticipant = match.ProUserId == userId || match.ControUserId == userId;
            if (!isParticipant) return Forbid();

            var submission = await _context.MatchSubmissions
                .FirstOrDefaultAsync(s =>
                    s.MatchId == matchId &&
                    s.UserId == userId &&
                    s.Phase == SubmissionPhase.Rebuttal);

            if (submission is null)
            {
                submission = new MatchSubmission
                {
                    Id = Guid.NewGuid(),
                    MatchId = matchId,
                    UserId = userId,
                    Phase = SubmissionPhase.Rebuttal,
                    Body = NormalizeBody(request.Body),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSubmitted = false,
                    SubmittedAt = null
                };

                _context.MatchSubmissions.Add(submission);
            }
            else
            {
                if (submission.IsSubmitted)
                    return Conflict("Rebuttal già consegnato: non puoi modificarlo.");

                submission.Body = NormalizeBody(request.Body);
                submission.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                submission.Id,
                submission.MatchId,
                submission.Phase,
                submission.IsSubmitted,
                submission.UpdatedAt
            });
        }

        // ---------------------------
        // REBUTTAL: SUBMIT
        // POST: api/matches/{matchId}/submissions/rebuttal/submit
        // ---------------------------
        [HttpPost("rebuttal/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitRebuttal(Guid matchId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized("Token non valido.");

            var match = await _context.DebateMatches
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match is null) return NotFound("Match inesistente.");

            if (match.Phase != MatchPhase.Rebuttal)
                return BadRequest("Non puoi consegnare il rebuttal: il match non è in fase Rebuttal.");

            var isParticipant = match.ProUserId == userId || match.ControUserId == userId;
            if (!isParticipant) return Forbid();

            var submission = await _context.MatchSubmissions
                .FirstOrDefaultAsync(s =>
                    s.MatchId == matchId &&
                    s.UserId == userId &&
                    s.Phase == SubmissionPhase.Rebuttal);

            if (submission is null)
                return BadRequest("Nessuna bozza rebuttal trovata: salva una draft prima di consegnare.");

            if (submission.IsSubmitted)
                return Conflict("Rebuttal già consegnato.");

            submission.IsSubmitted = true;
            submission.SubmittedAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var submittedCount = await _context.MatchSubmissions
                .AsNoTracking()
                .CountAsync(s =>
                    s.MatchId == matchId &&
                    s.Phase == SubmissionPhase.Rebuttal &&
                    s.IsSubmitted);

            if (submittedCount == 2)
            {
                match.Phase = MatchPhase.Voting;
                match.VotingEndsAt = DateTime.UtcNow.AddHours(24);
                match.ClosedAt = null;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                submission.Id,
                submission.MatchId,
                submission.Phase,
                submission.IsSubmitted,
                submission.SubmittedAt,
                MatchPhase = match.Phase
            });
        }
    }
}
