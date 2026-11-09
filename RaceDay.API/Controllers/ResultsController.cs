using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            return await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .ToListAsync();
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        // POST: api/Results
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Result>> PostResult(Result result)
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResult), new { id = result.ResultID }, result);
        }

        // PUT: api/Results/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PutResult(int id, Result result)
        {
            if (id != result.ResultID)
            {
                return BadRequest();
            }

            _context.Entry(result).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultExists(id))
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

        // DELETE: api/Results/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultExists(int id)
        {
            return _context.Results.Any(e => e.ResultID == id);
        }
    }
}