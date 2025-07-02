using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Utils;
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
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }


        // POST: api/Carts
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            if (request.KitId == null && request.LunchboxId == null)
            {
                return BadRequest("Você deve fornecer um KitId ou LunchboxId.");
            }

            // Carrega o carrinho e os itens com Kit e Lunchbox incluídos
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = DateTime.UtcNow,
                    IsCheckedOut = false,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            // Verifica se o item já está no carrinho
            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                (request.KitId != null && ci.KitId == request.KitId) ||
                (request.LunchboxId != null && ci.LunchboxId == request.LunchboxId));

            // Se o item já existe, atualiza a quantidade
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            // Se o item não existe, adiciona um novo
            else
            {
                var newItem = new CartItem
                {
                    KitId = request.KitId,
                    LunchboxId = request.LunchboxId,
                    Quantity = request.Quantity
                };

                cart.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();

            // Criar o DTO de resposta
            var response = new CartDto
            {
                Id = cart.Id,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList()
            };

            return Ok(cart);
        }


        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(int id, Cart cart)
        {
            if (id != cart.Id)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
