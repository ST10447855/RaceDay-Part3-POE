using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganisersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Organisers
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Organiser>>> GetOrganisers()
        {
            return await _context.Organisers.ToListAsync();
        }

        // GET: api/Organisers/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Organiser>> GetOrganiser(int id)
        {
            var organiser = await _context.Organisers.FindAsync(id);

            if (organiser == null)
            {
                return NotFound();
            }

            return organiser;
        }

        // POST: api/Organisers
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Organiser>> PostOrganiser(Organiser organiser)
        {
            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganiser), new { id = organiser.OrganiserID }, organiser);
        }

        // PUT: api/Organisers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PutOrganiser(int id, Organiser organiser)
        {
            if (id != organiser.OrganiserID)
            {
                return BadRequest();
            }

            _context.Entry(organiser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganiserExists(id))
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

        // DELETE: api/Organisers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteOrganiser(int id)
        {
            var organiser = await _context.Organisers.FindAsync(id);
            if (organiser == null)
            {
                return NotFound();
            }

            _context.Organisers.Remove(organiser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrganiserExists(int id)
        {
            return _context.Organisers.Any(e => e.OrganiserID == id);
        }
    }
}