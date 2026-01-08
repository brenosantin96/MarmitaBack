using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public KitsController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/Kits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kit>>> GetKits()
        {
            try
            {
                var kits =  await _context.Kits
                    .Where(k => k.TenantId == _tenantProvider.TenantId)
                    .Include(k => k.KitLunchboxes)
                    .ToListAsync();


                // Mapeia para DTO
                var response = kits.Select(k => new KitResponseDto
                {
                    Id = k.Id,
                    Name = k.Name,
                    Description = k.Description,
                    Price = k.Price,
                    ImageUrl = k.ImageUrl,
                    CategoryId = k.CategoryId,
                    LunchboxIds = k.KitLunchboxes
                        .Select(kl => kl.LunchBoxId)
                        .ToList()
                })
                .ToList();

                return Ok(response);


            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        // GET: api/Kits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KitResponseDto>> GetKit(int id)
        {
            var kit = await _context.Kits
                .Where(k => k.Id == id && k.TenantId == _tenantProvider.TenantId)
                .Include(k => k.KitLunchboxes)
                .FirstOrDefaultAsync();

            if (kit == null)
                return NotFound();

            // montar DTO
            var dto = new KitResponseDto
            {
                Id = kit.Id,
                Name = kit.Name,
                Description = kit.Description,
                Price = kit.Price,
                ImageUrl = kit.ImageUrl,
                CategoryId = kit.CategoryId,
                LunchboxIds = kit.KitLunchboxes
                    .Select(kl => kl.LunchBoxId)
                    .ToList()
            };

            return Ok(dto);
        }


        // POST: api/KitsWithImage
        [HttpPost]
        [Route("/api/KitsWithImage")]
        [Authorize]
        public async Task<ActionResult<KitResponseDto>> PostKitWithImage([FromForm] KitCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Kit cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = _tenantProvider.TenantId;

            // Valida categoria
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.TenantId == tenantId);

            if (!categoryExists)
                return BadRequest("Category does not exist.");

            // Pasta por tenant
            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                tenantId.ToString(),
                "kits"
            );

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Nome único
            string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.Image.CopyToAsync(stream);

            var kit = new Kit
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                ImageUrl = $"/images/{tenantId}/kits/{uniqueFileName}",
                TenantId = tenantId
            };

            _context.Kits.Add(kit);
            await _context.SaveChangesAsync();

            // Associa marmitas
            if (dto.LunchboxIds != null)
            {
                foreach (var lunchboxId in dto.LunchboxIds)
                {
                    bool exists = await _context.Lunchboxes
                        .AnyAsync(l => l.Id == lunchboxId && l.TenantId == tenantId);

                    if (exists)
                    {
                        _context.KitLunchboxes.Add(new KitLunchbox
                        {
                            KitId = kit.Id,
                            LunchBoxId = lunchboxId,
                            Quantity = 1,
                            TenantId = tenantId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            var response = new KitResponseDto
            {
                Id = kit.Id,
                Name = kit.Name,
                Description = kit.Description,
                Price = kit.Price,
                ImageUrl = kit.ImageUrl,
                CategoryId = kit.CategoryId,
                LunchboxIds = dto.LunchboxIds?.ToList() ?? new List<int>()
            };

            return CreatedAtAction("GetKit", new { id = kit.Id }, response);
        }


        // PUT: api/KitsWithImage/5
        [HttpPut]
        [Route("/api/KitsWithImage/{id}")]
        [Authorize]
        public async Task<IActionResult> PutKitWithImage(int id, [FromForm] KitUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("No data received.");

            var existingKit = await _context.Kits
                .Where(k => k.Id == id && k.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync();

            if (existingKit == null)
                return NotFound("Kit not found.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Valida Categoria
            if (dto.CategoryId.HasValue)
            {
                bool categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == dto.CategoryId && c.TenantId == _tenantProvider.TenantId);

                if (!categoryExists)
                    return BadRequest("Category does not exist.");

                existingKit.CategoryId = dto.CategoryId.Value;
            }

            // Atualiza campos básicos
            if (dto.Name != null) existingKit.Name = dto.Name;
            if (dto.Description != null) existingKit.Description = dto.Description;
            if (dto.Price.HasValue) existingKit.Price = dto.Price.Value;


            var tenantId = _tenantProvider.TenantId;

            // Processamento de nova imagem
            IFormFile? image = dto.Image;

            if (image == null && Request.HasFormContentType && Request.Form.Files.Count > 0)
            {
                image = Request.Form.Files["image"] ?? Request.Form.Files.FirstOrDefault();
            }

            bool hasNewImage = image != null && image.Length > 0;

            if (hasNewImage)
            {
                // Apaga imagem antiga
                if (!string.IsNullOrEmpty(existingKit.ImageUrl))
                {
                    string oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existingKit.ImageUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Pasta por tenant
                string uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    tenantId.ToString(),
                    "kits"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                existingKit.ImageUrl = $"/images/{tenantId}/kits/{uniqueFileName}";
            }


            // Atualização dos LunchboxIds
            if (dto.LunchboxIds != null)
            {
                // Remove todas as associações antigas desse Kit e do tenant atual
                var existingLinks = await _context.KitLunchboxes
                    .Where(kl => kl.KitId == existingKit.Id && kl.TenantId == _tenantProvider.TenantId)
                    .ToListAsync();

                if (existingLinks.Any())
                    _context.KitLunchboxes.RemoveRange(existingLinks);

                // Adiciona apenas as novas IDs enviadas
                foreach (var lunchboxId in dto.LunchboxIds)
                {
                    var lunchboxExists = await _context.Lunchboxes
                        .AnyAsync(l => l.Id == lunchboxId && l.TenantId == _tenantProvider.TenantId);

                    if (lunchboxExists)
                    {
                        _context.KitLunchboxes.Add(new KitLunchbox
                        {
                            KitId = existingKit.Id,
                            LunchBoxId = lunchboxId,
                            Quantity = 1,
                            TenantId = _tenantProvider.TenantId
                        });
                    }
                }
            }





            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Kits/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteKit(int id)
        {
            var kit = await _context.Kits
                .Where(k => k.Id == id && k.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync();

            if (kit == null)
                return NotFound();

            // Verifica se existe carrinho usando este KIT
            if (_context.CartItems.Any(c => c.TenantId == _tenantProvider.TenantId && c.KitId == id))
            {
                return BadRequest(new { error = "Não é possível deletar, existem carrinhos usando este kit." });
            }

            // Apagar imagem
            if (!string.IsNullOrEmpty(kit.ImageUrl))
            {
                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", kit.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Kits.Remove(kit);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Kit com ID {id} foi deletado com sucesso." });
        }

        // POST: api/Kits/1/lunchboxes
        [HttpPost("{kitId}/lunchboxes")]
        public async Task<IActionResult> AddLunchboxesToKit(int kitId, [FromBody] List<int> lunchboxIds)
        {
            var kit = await _context.Kits
                .Where(k => k.Id == kitId && k.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync();

            if (kit == null)
                return NotFound($"Kit with ID {kitId} not found.");

            foreach (var lunchboxId in lunchboxIds)
            {
                var lunchbox = await _context.Lunchboxes
                    .Where(l => l.Id == lunchboxId && l.TenantId == _tenantProvider.TenantId)
                    .FirstOrDefaultAsync();

                if (lunchbox == null)
                    return NotFound($"Lunchbox with ID {lunchboxId} does not exist.");

                var existing = await _context.KitLunchboxes
                     .FirstOrDefaultAsync(k => k.KitId == kitId && k.LunchBoxId == lunchboxId);

                if (existing == null)
                {
                    _context.KitLunchboxes.Add(new KitLunchbox
                    {
                        KitId = kitId,
                        LunchBoxId = lunchboxId,
                        Quantity = 1,
                        TenantId = _tenantProvider.TenantId,
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok($"Marmitas associadas ao kit {kitId}");
        }

        // POST: api/Kits/1/lunchboxes/remove
        [HttpPost("{kitId}/lunchboxes/remove")]
        [Authorize]
        public async Task<IActionResult> RemoveLunchboxesFromKit(int kitId, [FromBody] List<int> lunchboxIds)
        {
            var kit = await _context.Kits
                .Where(k => k.Id == kitId && k.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync();

            if (kit == null)
                return NotFound($"Kit with ID {kitId} not found.");

            foreach (var lunchboxId in lunchboxIds)
            {
                var link = await _context.KitLunchboxes
                    .Where(kl => kl.KitId == kitId && kl.LunchBoxId == lunchboxId && kl.TenantId == _tenantProvider.TenantId)
                    .FirstOrDefaultAsync();

                if (link != null)
                    _context.KitLunchboxes.Remove(link);
            }

            await _context.SaveChangesAsync();
            return Ok($"Marmitas removidas do kit {kitId}");
        }




    }
}
