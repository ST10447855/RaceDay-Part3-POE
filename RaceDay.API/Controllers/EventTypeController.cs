using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        // Constructor injection for the Database Context
        public EventTypesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/EventTypes
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventType>>> GetEventTypes()
        {
            return await _context.EventTypes.ToListAsync();
        }

        // GET: api/EventTypes/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventType>> GetEventType(int id)
        {
            var eventType = await _context.EventTypes.FindAsync(id);

            if (eventType == null)
            {
                return NotFound();
            }

            return eventType;
        }

        // POST: api/EventTypes
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<EventType>> PostEventType(EventType eventType)
        {
            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventType), new { id = eventType.EventTypeID }, eventType);
        }

        // PUT: api/EventTypes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PutEventType(int id, EventType eventType)
        {
            if (id != eventType.EventTypeID)
            {
                return BadRequest();
            }

            _context.Entry(eventType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventTypeExists(id))
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

        // DELETE: api/EventTypes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null)
            {
                return NotFound();
            }

            _context.EventTypes.Remove(eventType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventTypeExists(int id)
        {
            return _context.EventTypes.Any(e => e.EventTypeID == id);
        }
    }
}