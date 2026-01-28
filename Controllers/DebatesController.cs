using DebatePlatform.Api.Controllers.Request;
using DebatePlatform.Api.Controllers.Response;
using DebatePlatform.Api.Domain.Entities;
using DebatePlatform.Api.Domain.Enums;
using DebatePlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DebatePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DebatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/debates
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var debates = await _context.Debates
                .AsNoTracking()
                .Where(d => d.Status == DebateStatus.Approved)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DebateListItemResponse
                {
                    Id = d.Id,
                    Title = d.Title,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    CreatedByUserId = d.CreatedByUserId
                })
                .ToListAsync();

            return Ok(debates);
        }

        // GET: api/debates/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            //Recupero solo lo status (query leggerissima)
            var status = await _context.Debates
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => (DebateStatus?)d.Status)
                .FirstOrDefaultAsync();

            if (status is null) return NotFound();

            //  Regola visibilità:
            // - Approved: visibile a tutti
            // - Non Approved: visibile solo a Moderator
            var isModerator = User.IsInRole("Moderator");
            if (status != DebateStatus.Approved && !isModerator)
                return NotFound(); 

            // prendere il dettaglio completo
            var debate = await _context.Debates
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DebateDetailResponse
                {
                    Id = d.Id,
                    Title = d.Title,
                    Body = d.Body,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    CreatedByUserId = d.CreatedByUserId,

                    ProCount = d.Votes.Count(v => v.Value == VoteValue.Pro),
                    ControCount = d.Votes.Count(v => v.Value == VoteValue.Contro),
                    TotalVotes = d.Votes.Count(),

                    StatusHistory = d.StatusHistory
                        .OrderByDescending(h => h.ChangedAt)
                        .Select(h => new DebateStatusHistoryItemResponse
                        {
                            Id = h.Id,
                            FromStatus = h.FromStatus,
                            ToStatus = h.ToStatus,
                            ChangedAt = h.ChangedAt,
                            ChangedByUserId = h.ChangedByUserId,
                            ChangedByUsernameSnapshot = h.ChangedByUsernameSnapshot,
                            ChangedByEmailSnapshot = h.ChangedByEmailSnapshot,
                            Reason = h.Reason
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (debate is null) return NotFound();
            return Ok(debate);
        }




        [HttpGet("moderation")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> GetForModeration([FromQuery] DebateStatus? status = null)
    {
        var query = _context.Debates.AsNoTracking();

      
        if (status is null)
        {
            query = query.Where(d => d.Status == DebateStatus.Open || d.Status == DebateStatus.InReview);
        }
        else
        {
            query = query.Where(d => d.Status == status);
        }

        var debates = await query
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DebateListItemResponse
            {
                Id = d.Id,
                Title = d.Title,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                CreatedByUserId = d.CreatedByUserId
            })
            .ToListAsync();

        return Ok(debates);
    }





    // POST: api/debates
    [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateDebateRequest request)
        {
            // 1) Leggo l'ID dell'utente dal token JWT validato da ASP.NET Core
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Token non valido: NameIdentifier mancante.");

            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Token non valido: userId non è un Guid.");

            // 2) Creo il dibattito usando l'utente loggato
            var debate = new Debate
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                CreatedAt = DateTime.UtcNow,
                Status = Domain.Enums.DebateStatus.Open,
                CreatedByUserId = userId
            };

            _context.Debates.Add(debate);
            await _context.SaveChangesAsync();

            var response = new CreateDebateResponse
            {
                Id = debate.Id,
                Status = debate.Status,
                CreatedAt = debate.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = debate.Id }, response);
        }

        // POST: api/debates/{id}/status
        [HttpPost("{id:guid}/status")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeDebateStatusRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var moderatorId))
                return Unauthorized("Token non valido.");

            var debate = await _context.Debates.FirstOrDefaultAsync(d => d.Id == id);
            if (debate is null)
                return NotFound("Dibattito inesistente.");

            var from = debate.Status;
            var to = request.NewStatus;

            if (!IsAllowedTransition(from, to))
                return BadRequest($"Transizione non consentita: {from} -> {to}");

            debate.Status = to;

            var history = new DebateStatusHistory
            {
                Id = Guid.NewGuid(),
                DebateId = debate.Id,
                FromStatus = from,
                ToStatus = to,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = moderatorId,
                ChangedByUsernameSnapshot = User.Identity?.Name,
                ChangedByEmailSnapshot = User.FindFirstValue(ClaimTypes.Email),
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim()
            };

            _context.DebateStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                debate.Id,
                FromStatus = from,
                ToStatus = to,
                history.ChangedAt,
                history.ChangedByUserId,
                history.Reason
            });
        }

        private static bool IsAllowedTransition(DebateStatus from, DebateStatus to)
        {
            return (from, to) switch
            {
                (DebateStatus.Open, DebateStatus.InReview) => true,
                (DebateStatus.InReview, DebateStatus.Approved) => true,
                (DebateStatus.InReview, DebateStatus.Rejected) => true,
                (DebateStatus.Approved, DebateStatus.Closed) => true,

                _ => false
            };
        }



    }
}
