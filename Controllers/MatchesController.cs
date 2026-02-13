using DebatePlatform.Api.Controllers.Response;
using DebatePlatform.Api.Domain.Entities;
using DebatePlatform.Api.Domain.Enums;
using DebatePlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DebatePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
// Mostra SOLO match votabili dal pubblico: Voting + non scaduti + topic Approved + testi consegnati
        // GET: api/matches/public       
        [HttpGet("public")]
        public async Task<IActionResult> GetPublicVotable()
        {
            var now = DateTime.UtcNow;

            var matches = await _context.DebateMatches
                .AsNoTracking()
                .Where(m =>
                    m.Phase == MatchPhase.Voting &&
                    m.Debate.Status == DebateStatus.Approved &&

                    // finestra di voto: deve esistere ed essere futura
                    m.VotingEndsAt != null &&
                    m.VotingEndsAt > now &&

                    // Opening: entrambi SUBMITTED
                    _context.MatchSubmissions.Count(s =>
                        s.MatchId == m.Id &&
                        s.Phase == SubmissionPhase.Opening &&
                        s.IsSubmitted) == 2 &&

                    // Rebuttal: entrambi SUBMITTED
                    _context.MatchSubmissions.Count(s =>
                        s.MatchId == m.Id &&
                        s.Phase == SubmissionPhase.Rebuttal &&
                        s.IsSubmitted) == 2
                )
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new PublicMatchListItemResponse
                {
                    MatchId = m.Id,
                    DebateId = m.DebateId,
                    DebateTitle = m.Debate.Title,
                    Phase = m.Phase,
                    CreatedAt = m.CreatedAt,

                    ProCount = _context.Votes.Count(v => v.MatchId == m.Id && v.Value == VoteValue.Pro),
                    ControCount = _context.Votes.Count(v => v.MatchId == m.Id && v.Value == VoteValue.Contro),
                    TotalVotes = _context.Votes.Count(v => v.MatchId == m.Id)
                })
                .ToListAsync();

            return Ok(matches);
        }
// Risultati pubblici:
        // - match Closed
        // - oppure match in Voting scaduti (VotingEndsAt <= now), anche se non sono ancora stati lazy-closed
        // GET: api/matches/results
        [HttpGet("results")]
        public async Task<IActionResult> GetPublicResults()
        {
            var now = DateTime.UtcNow;

            var matches = await _context.DebateMatches
                .AsNoTracking()
                .Where(m =>
                    m.Debate.Status == DebateStatus.Approved &&

                    (
                        m.Phase == MatchPhase.Closed ||
                        (m.Phase == MatchPhase.Voting && m.VotingEndsAt != null && m.VotingEndsAt <= now)
                    ) &&

                    _context.MatchSubmissions.Count(s =>
                        s.MatchId == m.Id &&
                        s.Phase == SubmissionPhase.Opening &&
                        s.IsSubmitted) == 2 &&

                    _context.MatchSubmissions.Count(s =>
                        s.MatchId == m.Id &&
                        s.Phase == SubmissionPhase.Rebuttal &&
                        s.IsSubmitted) == 2
                )
                .OrderByDescending(m => m.ClosedAt ?? m.VotingEndsAt ?? m.CreatedAt)
                .Select(m => new PublicMatchListItemResponse
                {
                    MatchId = m.Id,
                    DebateId = m.DebateId,
                    DebateTitle = m.Debate.Title,
                    Phase = m.Phase,
                    CreatedAt = m.CreatedAt,

                    ProCount = _context.Votes.Count(v => v.MatchId == m.Id && v.Value == VoteValue.Pro),
                    ControCount = _context.Votes.Count(v => v.MatchId == m.Id && v.Value == VoteValue.Contro),
                    TotalVotes = _context.Votes.Count(v => v.MatchId == m.Id)
                })
                .ToListAsync();

            return Ok(matches);
        }

        // GET: api/matches/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // 1) Carico l'entity per poter fare lazy-close (serve tracking)
            var matchEntity = await _context.DebateMatches
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matchEntity is null) return NotFound();

            // 2) Lazy-close: se è Voting ma scaduto, lo chiudo ora
            var now = DateTime.UtcNow;

            if (matchEntity.Phase == MatchPhase.Voting &&
                matchEntity.VotingEndsAt != null &&
                matchEntity.VotingEndsAt <= now)
            {
                matchEntity.Phase = MatchPhase.Closed;
                matchEntity.ClosedAt = now;

                await _context.SaveChangesAsync();
            }

            // 3) Ora costruisco il DTO
            var match = await _context.DebateMatches
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MatchDetailResponse
                {
                    Id = m.Id,
                    DebateId = m.DebateId,
                    Phase = m.Phase,
                    CreatedAt = m.CreatedAt,
                    ProUserId = m.ProUserId,
                    ControUserId = m.ControUserId,

                    VotingEndsAt = m.VotingEndsAt,
                    ClosedAt = m.ClosedAt
                })
                .FirstOrDefaultAsync();

            if (match is null) return NotFound(); 

            // conteggi voti
            match.ProCount = await _context.Votes
                .AsNoTracking()
                .CountAsync(v => v.MatchId == id && v.Value == VoteValue.Pro);

            match.ControCount = await _context.Votes
                .AsNoTracking()
                .CountAsync(v => v.MatchId == id && v.Value == VoteValue.Contro);

            match.TotalVotes = match.ProCount + match.ControCount;

            // vincitore / pareggio
            if (match.TotalVotes == 0)
            {
                match.WinnerUserId = null;
                match.WinnerUsername = null;
                match.IsDraw = false;
            }
            else if (match.ProCount == match.ControCount)
            {
                match.WinnerUserId = null;
                match.WinnerUsername = null;
                match.IsDraw = true;
            }
            else
            {
                var winnerUserId = match.ProCount > match.ControCount
                    ? match.ProUserId
                    : match.ControUserId;

                match.WinnerUserId = winnerUserId;
                match.IsDraw = false;

                var winnerUser = await _userManager.FindByIdAsync(winnerUserId.ToString());
                match.WinnerUsername = winnerUser?.UserName;
            }

            // opening: visibili solo se entrambi SUBMITTED
            var opening = await _context.MatchSubmissions
                .AsNoTracking()
                .Where(s => s.MatchId == id
                    && s.Phase == SubmissionPhase.Opening
                    && s.IsSubmitted)
                .OrderBy(s => s.SubmittedAt)
                .Select(s => new MatchSubmissionResponse
                {
                    UserId = s.UserId,
                    Phase = s.Phase,
                    Body = s.Body,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            if (opening.Count == 2)
                match.OpeningSubmissions = opening;

            // rebuttal: visibili solo se entrambi SUBMITTED
            var rebuttal = await _context.MatchSubmissions
                .AsNoTracking()
                .Where(s => s.MatchId == id
                    && s.Phase == SubmissionPhase.Rebuttal
                    && s.IsSubmitted)
                .OrderBy(s => s.SubmittedAt)
                .Select(s => new MatchSubmissionResponse
                {
                    UserId = s.UserId,
                    Phase = s.Phase,
                    Body = s.Body,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            if (rebuttal.Count == 2)
                match.RebuttalSubmissions = rebuttal;

            return Ok(match);
        }

        // GET: api/matches/mine
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Token non valido.");

            var matches = await _context.DebateMatches
                .AsNoTracking()
                .Where(m => m.ProUserId == userId || m.ControUserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.DebateId,
                    m.Phase,
                    m.CreatedAt
                })
                .ToListAsync();

            return Ok(matches);
        }
    }
}
