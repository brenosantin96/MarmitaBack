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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLunchbox(int id, Lunchbox lunchbox)
        {
            var existingLunchbox = await _context.Lunchboxes.FindAsync(id);

            if (existingLunchbox == null)
            {
                return NotFound("Lunchbox not found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // validating [required], [range].
            }

            //verificando se categoria existe
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == lunchbox.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            // Atualiza os campos


            existingLunchbox.Name = lunchbox.Name;
            existingLunchbox.Description = lunchbox.Description;
            existingLunchbox.Price = lunchbox.Price;
            existingLunchbox.ImageUrl = lunchbox.ImageUrl;
            existingLunchbox.CategoryId = lunchbox.CategoryId;



            await _context.SaveChangesAsync();

            return NoContent(); // padrão REST para updates


        }

        // POST: api/Lunchboxes
        [HttpPost]
        public async Task<ActionResult<Lunchbox>> PostLunchbox(Lunchbox lunchbox)
        {
            if (lunchbox == null)
            {
                return BadRequest("Lunchbox cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // validating [required], [range].
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == lunchbox.CategoryId);
            if (!categoryExists)
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

            return Ok(new { message = $"Lunchbox com ID {id} foi deletada com sucesso." });
        }

    }
}
