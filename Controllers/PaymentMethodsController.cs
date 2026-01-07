using MarmitaBackend.DTOs;
using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;


        public PaymentMethodsController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/PaymentMethods
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethod>>> GetAllPaymentMethods()
        {
            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                bool isAdmin = await _context.Users
                    .AnyAsync(u => u.Id == loggedUserId && u.isAdmin);

                if (!isAdmin)
                    return Forbid();


                var paymentMethods = await _context.PaymentMethods
                    .Where(pm => pm.TenantId == _tenantProvider.TenantId)
                    .ToListAsync();

                return Ok(paymentMethods);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Teste erro: {ex.Message} cu");
                return BadRequest(ex.Message);
            }
        }

        // POST: api/PaymentMethods
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PaymentMethod>> CreatePaymentMethod([FromBody] PaymentMethodCreateUpdateDto dto)
        {
            var loggedUserId = UserHelper.GetUserId(User);

            bool isAdmin = await _context.Users
                .AnyAsync(u => u.Id == loggedUserId && u.isAdmin);


            if (dto == null)
            {
                return BadRequest("PaymentMethod cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Criar objeto Lunchbox para salvar no banco
            var paymentMethod = new PaymentMethod
            {
                Name = dto.Name,
                Description = dto.Description,
                TenantId = _tenantProvider.TenantId
            };

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            var responseDto = new PaymentMethodDto
            {
                Id = paymentMethod.Id,
                Name = paymentMethod.Name,
                Description = paymentMethod.Description
            };


            return CreatedAtAction("GetAllPaymentMethods", new { id = paymentMethod.Id }, responseDto);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> UpdatePaymentMethod(int id,[FromBody] PaymentMethodCreateUpdateDto dto)
        {
            var userLogged = UserHelper.GetUserId(User);

            bool isAdmin = await _context.Users
                .AnyAsync(u =>
                    u.Id == userLogged &&
                    u.isAdmin &&
                    u.TenantId == _tenantProvider.TenantId);

            if (!isAdmin)
                return Forbid();

            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm =>
                    pm.Id == id &&
                    pm.TenantId == _tenantProvider.TenantId);

            if (paymentMethod == null)
                return NotFound();

            if (dto.Name != null)
                paymentMethod.Name = dto.Name;

            if (dto.Description != null)
                paymentMethod.Description = dto.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE: api/PaymentMethods/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeletePaymentMethod(int id)
        {
            var userLogged = UserHelper.GetUserId(User);

            bool isAdmin = await _context.Users
                .AnyAsync(u =>
                    u.Id == userLogged &&
                    u.isAdmin &&
                    u.TenantId == _tenantProvider.TenantId);

            if (!isAdmin)
                return Forbid();

            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm =>
                    pm.Id == id &&
                    pm.TenantId == _tenantProvider.TenantId);

            if (paymentMethod == null)
                return NotFound();

            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 - padrão para delete
        }

    }
}
