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
    public class OrdersController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;


        public OrdersController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/orders/all
        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                bool isAdmin = await _context.Users
                    .AnyAsync(u => u.Id == loggedUserId && u.isAdmin);

                if (!isAdmin)
                    return Unauthorized("Somente administradores podem listar pedidos.");

                var orders = await _context.Orders
                    .Where(o => o.TenantId == _tenantProvider.TenantId)
                    .OrderByDescending(o => o.DeliveryInfo.DeliveryDate)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        CartId = o.CartId,

                        // mapeando o DeliveryInfo como objeto aninhado
                        DeliveryInfo = new DeliveryInfoDto
                        {
                            Id = o.DeliveryInfo.Id,
                            AddressId = o.DeliveryInfo.AddressId,
                            UserId = o.DeliveryInfo.UserId,
                            CanLeaveAtDoor = o.DeliveryInfo.CanLeaveAtDoor,
                            DeliveryDate = o.DeliveryInfo.DeliveryDate,
                            DeliveryPeriod = o.DeliveryInfo.DeliveryPeriod,
                            DeliveryType = o.DeliveryInfo.DeliveryType
                            
                        },

                        FullName = o.FullName,
                        Phone = o.Phone,
                        PaymentMethod = o.PaymentMethod.Name,
                        Subtotal = o.Subtotal,
                        DeliveryFee = o.DeliveryFee,
                        Total = o.Total,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/Orders
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderDto>> PostOrder(OrderCreateDto dto)
        {
            try
            {
                // 1. Pega tenantId do JWT
                var tenantIdString = User.FindFirstValue("tenantId");
                if (tenantIdString == null)
                    return Unauthorized("TenantId não encontrado no token.");

                int tenantId = int.Parse(tenantIdString);

                // 2. Valida que Cart pertence ao tenant
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.Id == dto.CartId && c.TenantId == tenantId);

                if (cart == null)
                    return Forbid("Cart não pertence ao tenant atual.");

                // 3. Valida DeliveryInfo pertence ao tenant
                var deliveryInfo = await _context.DeliveryInfo
                    .FirstOrDefaultAsync(d => d.Id == dto.DeliveryInfoId && d.TenantId == tenantId);

                if (deliveryInfo == null)
                    return Forbid("DeliveryInfo não pertence ao tenant atual.");

                // 4. Valida PaymentMethod pertence ao tenant
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(p => p.Id == dto.PaymentMethodId && p.TenantId == tenantId);

                if (paymentMethod == null)
                    return Forbid("PaymentMethod não pertence ao tenant atual.");

                // 5. Cria Order
                var order = new Order
                {
                    TenantId = tenantId,
                    CartId = dto.CartId,
                    DeliveryInfoId = dto.DeliveryInfoId,
                    PaymentMethodId = dto.PaymentMethodId,

                    FullName = dto.FullName,
                    Phone = dto.Phone,

                    Subtotal = dto.Subtotal,
                    DeliveryFee = dto.DeliveryFee,
                    Total = dto.Total,

                    CreatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 6. Retorna OrderDto
                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    CartId = order.CartId,
                    DeliveryInfo = new DeliveryInfoDto
                    {
                        DeliveryDate = deliveryInfo.DeliveryDate,
                        DeliveryPeriod = deliveryInfo.DeliveryPeriod,
                        CanLeaveAtDoor = deliveryInfo.CanLeaveAtDoor,  
                    },
                    FullName = order.FullName,
                    Phone = order.Phone,
                    PaymentMethod = paymentMethod.Name,
                    Subtotal = order.Subtotal,
                    DeliveryFee = order.DeliveryFee,
                    Total = order.Total,
                    CreatedAt = order.CreatedAt
                };

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Erro ao criar pedido.");
            }
        }


        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
//verificar se usuario esta logado e autenticado
//receber dados de Order na requisicao. exemplo:

/*        {
"cartId": 3,
"deliveryInfoId": 5,
"fullName": "João da Silva",
"phone": "11999999999",
"paymentMethodId": 1,
"subtotal": 70,
"deliveryFee": 5,
"total": 75
} */

//como aqui vou saber qual deliveryInfo associar no Order ? suponha que eu tenha muitos
//validar se DeliveryInfoId existe
//validar se DeliveryInfo pertence a um endereco do usuario autenticado
//validar se o DeliveryInfo de fato ainda nao esta associado a um order