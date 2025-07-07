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
    public class KitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Kits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kit>>> GetKits()
        {
            return await _context.Kits.ToListAsync();
        }

        // GET: api/Kits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Kit>> GetKit(int id)
        {
            var kit = await _context.Kits.FindAsync(id);

            if (kit == null)
            {
                return NotFound();
            }

            return kit;
        }

        // PUT: api/Kits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKit(int id, Kit kit)
        {

            var existingKit = await _context.Kits.FindAsync(id);
            if (existingKit == null)
            {
                return NotFound("Kit not found.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // validating [required], [range].
            }

            // Update the existing kit with the new values
            existingKit.Name = kit.Name;
            existingKit.Description = kit.Description;
            existingKit.Price = kit.Price;
            existingKit.ImageUrl = kit.ImageUrl;
            existingKit.CategoryId = kit.CategoryId;
            existingKit.KitLunchboxes = kit.KitLunchboxes;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao salvar no banco de dados.", details = ex.Message });
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Kit>> PostKit(Kit kit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // validating [required], [range].
            }

            _context.Kits.Add(kit);
            await _context.SaveChangesAsync();


            var createdKit = await _context.Kits
                .Include(k => k.Category) // Incluindo a categoria relacionada
                .FirstOrDefaultAsync(k => k.Id == kit.Id); //buscando no array de kits o kit recém criado


            return CreatedAtAction("GetKit", new { id = kit.Id }, createdKit);
        }

        // DELETE: api/Kits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKit(int id)
        {
            var kit = await _context.Kits.FindAsync(id);
            if (kit == null)
            {
                return NotFound();
            }

            _context.Kits.Remove(kit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //POST: api/Kits/1/lunchboxes
        [HttpPost("{kitId}/lunchboxes")]
        public async Task<IActionResult> AddLunchboxesToKit(int kitId, [FromBody] List<int> lunchboxIds)
        {

            //Primeiro devemos achar o kit que quero adicionar as marmitas
            var kitToAddLunchboxes = await _context.Kits.FirstOrDefaultAsync(k => k.Id == kitId);

            if (kitToAddLunchboxes == null)
            {
                return NotFound($"Kit with ID {kitId} not found.");
            }

            //Agora, vamos de fato adicionar as marmitas ao kit
            foreach (var lunchboxId in lunchboxIds)
            {
                var lunchbox = await _context.Lunchboxes.FindAsync(lunchboxId); // Verifica se a marmita existe
                if (lunchbox == null)
                {
                    return NotFound($"Lunchbox with ID {lunchboxId} does not exists.");
                }

                var existing = await _context.KitLunchboxes
                     .FirstOrDefaultAsync(k => k.KitId == kitId && k.LunchBoxId == lunchboxId); // Verifica se já existe a associação

                if (existing == null)
                {
                    _context.KitLunchboxes.Add(new KitLunchbox
                    {
                        KitId = kitId,
                        LunchBoxId = lunchboxId,
                        Quantity = 1 // Definindo uma quantidade padrão de 1
                    });
                }

            }

            await _context.SaveChangesAsync();
            return Ok($"Marmitas associadas ao kit {kitId}");
        }


    }

}

