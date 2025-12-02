using MarmitaBackend.Models;
using MarmitaBackend.Provider;
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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public CategoriesController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.Where(c => c.TenantId == _tenantProvider.TenantId).ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.Where(c => c.Id == id && c.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {

            var existingCategory = await _context.Categories
    .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == _tenantProvider.TenantId);

            if (existingCategory == null)
            {
                return NotFound("Category not found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //applying changes
            existingCategory.Name = category.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {

            category.TenantId = _tenantProvider.TenantId;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Where(c => c.Id == id && c.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
