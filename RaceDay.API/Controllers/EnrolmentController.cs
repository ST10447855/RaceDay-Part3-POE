using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for the controller endpoints by default
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [HttpGet]
        [Authorize(Roles = "Organiser")] // Generally restricted to Admins to view all system enrolments
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            return await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .ToListAsync();
        }

        // GET: api/Enrolments/my-enrolments
        [HttpGet("my-enrolments")]
        [Authorize(Roles = "Participant,Organiser")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                return Unauthorized();
            }

            var mine = await _context.Enrolments
                .Where(e => e.ParticipantID == userId)
                .Select(e => new
                {
                    e.EnrolmentID,
                    EventName = e.Event.Name,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(mine);
        }

        // GET: api/Enrolments/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            return enrolment;
        }

        // POST: api/Enrolments
        [HttpPost]
        [Authorize(Roles = "Organiser,Participant")] // Allow participants to register for races
        public async Task<ActionResult<Enrolment>> PostEnrolment(Enrolment enrolment)
        {
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrolmentID }, enrolment);
        }

        // PUT: api/Enrolments/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<IActionResult> PutEnrolment(int id, Enrolment enrolment)
        {
            if (id != enrolment.EnrolmentID)
            {
                return BadRequest();
            }

            _context.Entry(enrolment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrolmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Enrolments/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null)
            {
                return NotFound();
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EnrolmentExists(int id)
        {
            return _context.Enrolments.Any(e => e.EnrolmentID == id);
        }
    }
}