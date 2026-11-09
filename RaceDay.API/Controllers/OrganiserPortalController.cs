using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    public class EventDto
    {
        [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string? Description { get; set; }
        [Required] public DateTime EventDate { get; set; }
        [Required, MaxLength(200)] public string Location { get; set; } = string.Empty;
        [Range(0.1, 1000)] public decimal Distance { get; set; }
        [Range(1, int.MaxValue)] public int EventTypeID { get; set; }
        [MaxLength(500)] public string? BannerImageURL { get; set; }
    }

    public class CategoryDto
    {
        [Required, MaxLength(100)] public string CategoryName { get; set; } = string.Empty;
        [Range(0, 120)] public int MinimumAge { get; set; }
        [Range(0, 120)] public int MaximumAge { get; set; }
        [Range(0.1, 1000)] public decimal Distance { get; set; }
    }

    public class ResultDto
    {
        [Required, MaxLength(30)] public string FinishingTime { get; set; } = string.Empty;
        [Range(1, 100000)] public int Position { get; set; }
    }

    /// <summary>Organiser-only endpoints: own events, categories, enrolments and results.</summary>
    [ApiController]
    [Route("api/organiser")]
    [Authorize(Roles = "Organiser")]
    public class OrganiserPortalController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganiserPortalController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ---------- Events ----------

        /// <summary>Lists the logged-in organiser's events with enrolment totals.</summary>
        [HttpGet("events")]
        public async Task<IActionResult> GetMyEvents()
        {
            var uid = UserId;
            var events = await _context.Events.AsNoTracking()
                .Where(e => e.OrganiserID == uid)
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
                    CategoryCount = _context.Categories.Count(c => c.EventID == e.EventID),
                    EnrolmentCount = _context.Enrolments.Count(n => n.EventID == e.EventID)
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>Creates an event owned by the logged-in organiser.</summary>
        [HttpPost("events")]
        public async Task<IActionResult> CreateEvent(EventDto dto)
        {
            if (!await _context.EventTypes.AnyAsync(t => t.EventTypeID == dto.EventTypeID))
                return BadRequest(new { message = "Please choose a valid event type." });

            var ev = new Event
            {
                OrganiserID = UserId,
                EventTypeID = dto.EventTypeID,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                EventDate = dto.EventDate,
                Location = dto.Location.Trim(),
                Distance = dto.Distance,
                BannerImageURL = string.IsNullOrWhiteSpace(dto.BannerImageURL) ? null : dto.BannerImageURL
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return Ok(new { ev.EventID, message = "Event created." });
        }

        /// <summary>Updates one of the organiser's own events.</summary>
        [HttpPut("events/{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, EventDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != UserId) return Forbid();

            if (!await _context.EventTypes.AnyAsync(t => t.EventTypeID == dto.EventTypeID))
                return BadRequest(new { message = "Please choose a valid event type." });

            ev.EventTypeID = dto.EventTypeID;
            ev.Name = dto.Name.Trim();
            ev.Description = dto.Description?.Trim();
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location.Trim();
            ev.Distance = dto.Distance;
            if (!string.IsNullOrWhiteSpace(dto.BannerImageURL))
                ev.BannerImageURL = dto.BannerImageURL;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated." });
        }

        /// <summary>Deletes an event (and its categories) if nobody has enrolled yet.</summary>
        /// <response code="409">Event already has enrolments</response>
        [HttpDelete("events/{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != UserId) return Forbid();

            if (await _context.Enrolments.AnyAsync(e => e.EventID == id))
                return Conflict(new { message = "This event already has enrolments and cannot be deleted." });

            _context.Categories.RemoveRange(_context.Categories.Where(c => c.EventID == id));
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted." });
        }

        // ---------- Categories ----------

        /// <summary>Lists the categories of one of the organiser's events.</summary>
        [HttpGet("events/{id:int}/categories")]
        public async Task<IActionResult> GetCategories(int id)
        {
            if (!await OwnsEvent(id)) return NotFound(new { message = "Event not found." });

            var cats = await _context.Categories.AsNoTracking()
                .Where(c => c.EventID == id)
                .OrderBy(c => c.CategoryName)
                .Select(c => new { c.CategoryID, c.CategoryName, c.MinimumAge, c.MaximumAge, c.Distance })
                .ToListAsync();
            return Ok(cats);
        }

        /// <summary>Adds an age or distance category to an event.</summary>
        [HttpPost("events/{id:int}/categories")]
        public async Task<IActionResult> AddCategory(int id, CategoryDto dto)
        {
            if (!await OwnsEvent(id)) return NotFound(new { message = "Event not found." });
            if (dto.MaximumAge != 0 && dto.MaximumAge < dto.MinimumAge)
                return BadRequest(new { message = "Maximum age cannot be less than minimum age." });

            _context.Categories.Add(new Category
            {
                EventID = id,
                CategoryName = dto.CategoryName.Trim(),
                MinimumAge = dto.MinimumAge,
                MaximumAge = dto.MaximumAge,
                Distance = dto.Distance
            });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category added." });
        }

        /// <summary>Removes a category that nobody has enrolled in.</summary>
        [HttpDelete("categories/{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
            if (cat == null) return NotFound(new { message = "Category not found." });
            if (cat.Event.OrganiserID != UserId) return Forbid();

            if (await _context.Enrolments.AnyAsync(e => e.CategoryID == id))
                return Conflict(new { message = "Participants are enrolled in this category, so it cannot be removed." });

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category removed." });
        }

        // ---------- Enrolments & results ----------

        /// <summary>Lists every participant enrolled in one of the organiser's events.</summary>
        [HttpGet("events/{id:int}/enrolments")]
        public async Task<IActionResult> GetEnrolments(int id)
        {
            if (!await OwnsEvent(id)) return NotFound(new { message = "Event not found." });

            var rows = await _context.Enrolments.AsNoTracking()
                .Where(e => e.EventID == id)
                .OrderBy(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.EnrolmentID,
                    ParticipantName = e.Participant.FirstName + " " + e.Participant.LastName,
                    ParticipantEmail = e.Participant.Email,
                    CategoryName = e.Category.CategoryName,
                    e.Status,
                    e.EnrolmentDate,
                    HasResult = e.Result != null,
                    FinishingTime = e.Result != null ? e.Result.FinishingTime : null,
                    Position = e.Result != null ? (int?)e.Result.Position : null
                })
                .ToListAsync();
            return Ok(rows);
        }

        /// <summary>Captures (or updates) a participant's finish time and position.</summary>
        [HttpPost("enrolments/{id:int}/result")]
        public async Task<IActionResult> SaveResult(int id, ResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event.OrganiserID != UserId) return Forbid();

            if (enrolment.Result == null)
            {
                _context.Results.Add(new Result
                {
                    EnrolmentID = id,
                    FinishingTime = dto.FinishingTime.Trim(),
                    Position = dto.Position
                });
            }
            else
            {
                enrolment.Result.FinishingTime = dto.FinishingTime.Trim();
                enrolment.Result.Position = dto.Position;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Result saved." });
        }

        private async Task<bool> OwnsEvent(int eventId)
        {
            var uid = UserId;
            return await _context.Events.AnyAsync(e => e.EventID == eventId && e.OrganiserID == uid);
        }
    }
}
