using Humanizer;
using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LunchboxesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        public LunchboxesController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/Lunchboxes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lunchbox>>> GetLunchboxes()
        {
            return await _context.Lunchboxes.Where(x => x.TenantId == _tenantProvider.TenantId).ToListAsync();
        }

        // GET: api/Lunchboxes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lunchbox>> GetLunchbox(int id)
        {
            var lunchbox = await _context.Lunchboxes.Where(l => l.Id == id && l.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();


            if (lunchbox == null)
            {
                return NotFound();
            }

            return lunchbox;
        }

        // POST: api/LunchboxesWithImage
        [HttpPost]
        [Route("/api/LunchboxesWithImage")]
        [Authorize]
        public async Task<ActionResult<Lunchbox>> PostLunchboxWithImage([FromForm] LunchboxCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Lunchbox cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            //Saving image in server
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/lunchboxes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }


            //creating uniqueName
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            Console.WriteLine($"UploadsFolder: {uploadsFolder} /n UniqueFileName: {uniqueFileName}");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(fileStream);
            }

            // 🔹 Criar objeto Lunchbox para salvar no banco
            var lunchbox = new Lunchbox
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                PortionGram = dto.PortionGram,
                CategoryId = dto.CategoryId,
                ImageUrl = "/images/lunchboxes/" + uniqueFileName,
                TenantId = _tenantProvider.TenantId
            };

            _context.Lunchboxes.Add(lunchbox);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLunchbox", new { id = lunchbox.Id }, lunchbox);

        }

        // PUT: api/LunchboxesWithImage
        [HttpPut]
        [Route("/api/LunchboxesWithImage/{id}")]
        public async Task<IActionResult> PutLunchboxWithImage(int id, [FromForm] LunchboxUpdateDto dto)
        {
            // dto pode ser null se o content-type/form vier quebrado
            if (dto is null)
                return BadRequest("No form data received.");

            var existingLunchbox = await _context.Lunchboxes.Where(l => l.Id == id && l.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
            if (existingLunchbox == null)
                return NotFound("Lunchbox not found.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // valida categoria
            if (dto.CategoryId != null)
            {
                bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.TenantId == _tenantProvider.TenantId);

                if (!categoryExists)
                    return BadRequest("Category does not exist.");

            }


            // Atualiza campos básicos
            if (dto.Name != null)
                existingLunchbox.Name = dto.Name;

            if (dto.Description != null)
                existingLunchbox.Description = dto.Description;

            if (dto.Price.HasValue)
                existingLunchbox.Price = dto.Price.Value;

            if (dto.PortionGram.HasValue)
                existingLunchbox.PortionGram = dto.PortionGram.Value;

            if (dto.CategoryId.HasValue)
                existingLunchbox.CategoryId = dto.CategoryId.Value;

            // --- Processamento de imagem (opcional) ---
            // Fallback: se o binder não popular dto.Image, tenta pegar do Request.
            IFormFile? image = dto.Image;
            if (image == null && Request.HasFormContentType && Request.Form.Files.Count > 0)
            {
                // tenta pelo nome "image"; se não achar, pega a primeira
                image = Request.Form.Files["image"] ?? Request.Form.Files.FirstOrDefault();
            }

            // só processa se realmente veio um arquivo válido
            var hasNewImage = image != null
                              && image.Length > 0
                              && !string.IsNullOrWhiteSpace(image.FileName);

            if (hasNewImage)
            {
                // Remove a imagem antiga se existir
                if (!string.IsNullOrEmpty(existingLunchbox.ImageUrl))
                {
                    string oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existingLunchbox.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Garante pasta de upload
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/lunchboxes");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Salva nova imagem
                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image!.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(fileStream);

                // Atualiza caminho no banco
                existingLunchbox.ImageUrl = "/images/lunchboxes/" + uniqueFileName;
            }
            // --- fim processamento de imagem ---

            await _context.SaveChangesAsync();
            return NoContent(); // padrão REST para updates
        }




        // DELETE: api/Lunchboxes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLunchbox(int id)
        {
            var lunchbox = await _context.Lunchboxes.Where(l => l.Id == id && l.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();

            if (lunchbox == null)
            {
                return NotFound();
            }

            //se existir algum cart com essa lunchboxId, nao vai excluir.
            if (_context.CartItems.Any(c => c.TenantId == _tenantProvider.TenantId && c.LunchboxId == id))
            {
                return BadRequest(new { error = "Não é possível deletar, existem carrinhos usando essa lunchbox." });
            }

            //getting image in server
            Console.WriteLine($"CURRENT DIRECTORY: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"IMAGEURL LUNCHBOX: {lunchbox.ImageUrl}");

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", lunchbox.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.Lunchboxes.Remove(lunchbox);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Lunchbox com ID {id} foi deletada com sucesso." });
        }

    }
}
