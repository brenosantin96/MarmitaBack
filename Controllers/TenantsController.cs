using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tenants/resolve?hostname=cliente1.marmita.com
        [HttpGet("resolve")]
        public async Task<ActionResult<object>> ResolveTenant([FromQuery] string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return BadRequest("Hostname é obrigatório");

            var tenant = await _context.Tenants
                .Where(t => t.Domain == hostname)
                .FirstOrDefaultAsync();

            if (tenant == null)
                return NotFound("Tenant não encontrado");

            return Ok(new { tenantId = tenant.Id, tenantName = tenant.Name });
        }
    }
}
