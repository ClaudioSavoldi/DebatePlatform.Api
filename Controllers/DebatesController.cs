using DebatePlatform.Api.Controllers.Request;
using DebatePlatform.Api.Controllers.Response;
using DebatePlatform.Api.Domain.Entities;
using DebatePlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    CreatedByUserId = d.CreatedByUserId
                })
                .FirstOrDefaultAsync();

            if (debate is null) return NotFound();
            return Ok(debate);
        }

        // POST: api/debates
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDebateRequest request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.CreatedByUserId);
            if (!userExists)
                return BadRequest("CreatedByUserId non valido: utente inesistente.");

            var debate = new Debate
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                CreatedAt = DateTime.UtcNow,
                Status = Domain.Enums.DebateStatus.Open,
                CreatedByUserId = request.CreatedByUserId
            };

            _context.Debates.Add(debate);
            await _context.SaveChangesAsync();

            var response = new DebateDetailResponse
            {
                Id = debate.Id,
                Title = debate.Title,
                Body = debate.Body,
                Status = debate.Status,
                CreatedAt = debate.CreatedAt,
                CreatedByUserId = debate.CreatedByUserId
            };

            return CreatedAtAction(nameof(GetById), new { id = debate.Id }, response);
        }
    }
}
