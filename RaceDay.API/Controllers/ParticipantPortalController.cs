using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    public class EnrolDto
    {
        [Range(1, int.MaxValue)] public int EventID { get; set; }
        [Range(1, int.MaxValue)] public int CategoryID { get; set; }
    }

    public class ProfileDto
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
        [MaxLength(20)] public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(500)] public string? ProfilePictureURL { get; set; }
    }

    /// <summary>Participant-only endpoints: enrol, my enrolments, my results, my profile.</summary>
    [ApiController]
    [Route("api/participant")]
    [Authorize(Roles = "Participant")]
    public class ParticipantPortalController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantPortalController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Enrols the logged-in participant in an event under a chosen category.</summary>
        /// <response code="409">Already enrolled in this event</response>
        [HttpPost("enrol")]
        public async Task<IActionResult> Enrol(EnrolDto dto)
        {
            var ev = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == dto.EventID);
            if (ev == null) return NotFound(new { message = "Event not found." });

            if (ev.EventDate < DateTime.Now)
                return BadRequest(new { message = "This event has already taken place." });

            var categoryOk = await _context.Categories
                .AnyAsync(c => c.CategoryID == dto.CategoryID && c.EventID == dto.EventID);
            if (!categoryOk)
                return BadRequest(new { message = "Please choose a category for this event." });

            var uid = UserId;
            if (await _context.Enrolments.AnyAsync(e => e.ParticipantID == uid && e.EventID == dto.EventID))
                return Conflict(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantID = uid,
                EventID = dto.EventID,
                CategoryID = dto.CategoryID,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { enrolment.EnrolmentID, message = "You are enrolled!" });
        }

        /// <summary>The participant's own enrolments with status.</summary>
        [HttpGet("enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var uid = UserId;
            var rows = await _context.Enrolments.AsNoTracking()
                .Where(e => e.ParticipantID == uid)
                .OrderByDescending(e => e.Event.EventDate)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.EventID,
                    EventName = e.Event.Name,
                    EventDate = e.Event.EventDate,
                    CategoryName = e.Category.CategoryName,
                    e.Status,
                    e.EnrolmentDate
                })
                .ToListAsync();
            return Ok(rows);
        }

        /// <summary>The participant's race history: time, position and total finishers per event.</summary>
        [HttpGet("results")]
        public async Task<IActionResult> GetMyResults()
        {
            var uid = UserId;
            var rows = await _context.Results.AsNoTracking()
                .Where(r => r.Enrolment.ParticipantID == uid)
                .OrderByDescending(r => r.Enrolment.Event.EventDate)
                .Select(r => new
                {
                    EventName = r.Enrolment.Event.Name,
                    EventDate = r.Enrolment.Event.EventDate,
                    CategoryName = r.Enrolment.Category.CategoryName,
                    r.FinishingTime,
                    r.Position,
                    TotalFinishers = _context.Results.Count(x => x.Enrolment.EventID == r.Enrolment.EventID)
                })
                .ToListAsync();
            return Ok(rows);
        }

        /// <summary>The participant's own profile.</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var uid = UserId;
            var user = await _context.Users.AsNoTracking()
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserID == uid);
            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                DateOfBirth = user.ParticipantProfile?.DateOfBirth,
                ProfilePictureURL = user.ParticipantProfile?.ProfilePictureURL
            });
        }

        /// <summary>Updates the participant's name, phone, date of birth and profile picture URL.</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(ProfileDto dto)
        {
            var uid = UserId;
            var user = await _context.Users
                .Include(u => u.ParticipantProfile)
                .FirstOrDefaultAsync(u => u.UserID == uid);
            if (user == null) return NotFound(new { message = "User not found." });

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber;

            if (user.ParticipantProfile == null)
            {
                if (!dto.DateOfBirth.HasValue)
                    return BadRequest(new { message = "Date of birth is required to create your profile." });

                user.ParticipantProfile = new ParticipantProfile
                {
                    UserID = uid,
                    DateOfBirth = dto.DateOfBirth.Value,
                    ProfilePictureURL = dto.ProfilePictureURL
                };
            }
            else
            {
                if (dto.DateOfBirth.HasValue)
                    user.ParticipantProfile.DateOfBirth = dto.DateOfBirth.Value;
                if (!string.IsNullOrWhiteSpace(dto.ProfilePictureURL))
                    user.ParticipantProfile.ProfilePictureURL = dto.ProfilePictureURL;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated." });
        }
    }
}
