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


       

    }
}
