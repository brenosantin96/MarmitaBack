using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmitaBackend.Models;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LunchboxesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LunchboxesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Lunchboxes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lunchbox>>> GetLunchboxes()
        {
            return await _context.Lunchboxes.ToListAsync();
        }

        // GET: api/Lunchboxes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lunchbox>> GetLunchbox(int id)
        {
            var lunchbox = await _context.Lunchboxes.FindAsync(id);

            if (lunchbox == null)
            {
                return NotFound();
            }

            return lunchbox;
        }

        // PUT: api/Lunchboxes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLunchbox(int id, Lunchbox lunchbox)
        {
            if (id != lunchbox.Id)
            {
                return BadRequest();
            }

            _context.Entry(lunchbox).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LunchboxExists(id))
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

        // POST: api/Lunchboxes
        [HttpPost]
        public async Task<ActionResult<Lunchbox>> PostLunchbox(Lunchbox lunchbox)
        {
            if (lunchbox == null)
            {
                return BadRequest("Lunchbox cannot be null.");
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState); // validating [required], [range].
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == lunchbox.CategoryId);
            if(!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            _context.Lunchboxes.Add(lunchbox);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLunchbox", new { id = lunchbox.Id }, lunchbox);
        }




        // DELETE: api/Lunchboxes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLunchbox(int id)
        {
            var lunchbox = await _context.Lunchboxes.FindAsync(id);
            if (lunchbox == null)
            {
                return NotFound();
            }

            _context.Lunchboxes.Remove(lunchbox);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LunchboxExists(int id)
        {
            return _context.Lunchboxes.Any(e => e.Id == id);
        }
    }
}
