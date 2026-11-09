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
    public class ParticipantProfilesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantProfilesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/ParticipantProfiles
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<ParticipantProfile>>> GetParticipantProfiles()
        {
            return await _context.ParticipantProfiles
                .Include(p => p.User)
                .ToListAsync();
        }

        // GET: api/ParticipantProfiles/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<ActionResult<ParticipantProfile>> GetParticipantProfile(int id)
        {
            var profile = await _context.ParticipantProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProfileID == id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        // POST: api/ParticipantProfiles
        [HttpPost]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<ActionResult<ParticipantProfile>> PostParticipantProfile(ParticipantProfile profile)
        {
            _context.ParticipantProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipantProfile), new { id = profile.ProfileID }, profile);
        }

        // PUT: api/ParticipantProfiles/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<IActionResult> PutParticipantProfile(int id, ParticipantProfile profile)
        {
            if (id != profile.ProfileID)
            {
                return BadRequest();
            }

            _context.Entry(profile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantProfileExists(id))
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

        // DELETE: api/ParticipantProfiles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")] // Usually safer to restrict profile deletion strictly to admins
        public async Task<IActionResult> DeleteParticipantProfile(int id)
        {
            var profile = await _context.ParticipantProfiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            _context.ParticipantProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParticipantProfileExists(int id)
        {
            return _context.ParticipantProfiles.Any(e => e.ProfileID == id);
        }
    }
}