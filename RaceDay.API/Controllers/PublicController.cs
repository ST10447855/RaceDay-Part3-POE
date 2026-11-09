using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    /// <summary>Public (no login) read-only endpoints used by the home page and event detail page.</summary>
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public PublicController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>Lists events (upcoming by default) with optional search and event-type filter.</summary>
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] string? search,
            [FromQuery] int? eventTypeId,
            [FromQuery] bool upcomingOnly = true)
        {
            var query = _context.Events.AsNoTracking().AsQueryable();

            if (upcomingOnly)
                query = query.Where(e => e.EventDate >= DateTime.Today);

            if (eventTypeId.HasValue)
                query = query.Where(e => e.EventTypeID == eventTypeId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e => e.Name.Contains(term) || e.Location.Contains(term));
            }

            var events = await query
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventID,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventTypeID,
                    EventTypeName = e.EventType.TypeName,
                    e.BannerImageURL,
                    CategoryCount = _context.Categories.Count(c => c.EventID == e.EventID)
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>Full details of one event including its categories.</summary>
        /// <response code="404">Event does not exist</response>
        [HttpGet("events/{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events.AsNoTracking()
                .Where(e => e.EventID == id)
                .Select(e => new
                {
                    e.EventID,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.EventTypeID,
                    EventTypeName = e.EventType.TypeName,
                    e.BannerImageURL,
                    Categories = _context.Categories
                        .Where(c => c.EventID == e.EventID)
                        .OrderBy(c => c.CategoryName)
                        .Select(c => new { c.CategoryID, c.CategoryName, c.MinimumAge, c.MaximumAge, c.Distance })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }
    }
}
