﻿using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using MarmitaBackend.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        // Esse método deve ser apenas para ADMINS, mudar depois
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
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
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
                return Unauthorized("Usuário não autenticado.");

            // Verifica se DeliveryInfo existe e pertence ao usuário
            var deliveryInfo = await _context.DeliveryInfo
                .Include(di => di.Order)
                .FirstOrDefaultAsync(di => di.Id == order.DeliveryInfoId && di.UserId == userId);

            if (deliveryInfo == null)
                return BadRequest("DeliveryInfo inválido ou não pertence ao usuário.");

            // Verifica se já está associado a uma Order
            if (deliveryInfo.Order != null)
                return BadRequest("Este DeliveryInfo já está vinculado a um pedido.");

            // Verifica se o Cart pertence ao usuário
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == order.CartId && c.UserId == userId);

            if (cart == null)
                return BadRequest("Carrinho inválido para este usuário.");

            // Adiciona a Order
            order.CreatedAt = DateTime.UtcNow;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Recarrega com Include para popular DeliveryInfo
            var createdOrder = await _context.Orders
                .Include(o => o.DeliveryInfo)
                .Include(o => o.PaymentMethod)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            //DTO de resposta
            var orderDto = new OrderDto
            {
                Id = createdOrder.Id,
                CartId = createdOrder.CartId,
                FullName = createdOrder.FullName,
                Phone = createdOrder.Phone,
                PaymentMethod = createdOrder.PaymentMethod.Name,
                Subtotal = createdOrder.Subtotal,
                DeliveryFee = createdOrder.DeliveryFee,
                Total = createdOrder.Total,
                CreatedAt = createdOrder.CreatedAt,
                DeliveryInfo = new DeliveryInfoDto
                {
                    Id = createdOrder.DeliveryInfo.Id,
                    DeliveryDate = createdOrder.DeliveryInfo.DeliveryDate,
                    DeliveryPeriod = createdOrder.DeliveryInfo.DeliveryPeriod,
                    DeliveryType = createdOrder.DeliveryInfo.DeliveryType,
                    CanLeaveAtDoor = createdOrder.DeliveryInfo.CanLeaveAtDoor,
                    AddressId = createdOrder.DeliveryInfo.AddressId
                }
            };

            return CreatedAtAction("GetOrder", new { id = createdOrder.Id }, orderDto);


            

            
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