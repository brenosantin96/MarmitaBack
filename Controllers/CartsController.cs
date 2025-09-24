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



            var userId = UserHelper.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
            {
                return NotFound();
            }

            // Criar o DTO de resposta
            var response = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    LunchboxId = ci.LunchboxId,
                    KitId = ci.KitId,
                    Quantity = ci.Quantity,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = cart.IsCheckedOut
            };

            return Ok(response);

        }

        // GET: api/Carts/GetCartByUserId/5
        [HttpGet("GetCartByUserId/{userId}")]
        public async Task<ActionResult<Cart>> GetCartByUserId(int userId)
        {

            var loggedUserId = UserHelper.GetUserId(User);

            if (loggedUserId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            // Garante que o usuário autenticado só pode ver o próprio carrinho
            if (loggedUserId != userId)
            {
                return Forbid("Você não tem permissão para acessar o carrinho de outro usuário.");
            }

            // Busca o carrinho do usuário
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Lunchbox)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Kit)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (cart == null)
            {
                return NotFound("Carrinho não encontrado");
            }

            // Monta o DTO
            var response = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    LunchboxId = ci.LunchboxId,
                    KitId = ci.KitId,
                    Quantity = ci.Quantity,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = cart.IsCheckedOut
            };

            return Ok(response);

        }

        // POST: api/Carts/create
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCart()
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
                return Unauthorized("Usuário não autenticado.");

            // Busca o carrinho do usuário (não finalizado)
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            bool created = false;

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
                await _context.SaveChangesAsync();
                created = true;
            }

            // Criar o DTO de resposta
            var response = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = cart.IsCheckedOut
            };

            if (created)
                return CreatedAtAction(nameof(CreateCart), new { id = cart.Id }, response);

            return Ok(response);
        }

        // POST: api/Carts/create2
        [Authorize]
        [HttpPost("create2")]
        public async Task<IActionResult> CreateCartWithItems([FromBody] CreateCartDto createCartDto)
        {
            var userId = UserHelper.GetUserId(User); // valida usuário

            if (userId == null)
                return Unauthorized("Usuário não autenticado.");

            // pega carrinho não finalizado, já trazendo Lunchbox e Kit
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            bool created = false;

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    CreatedAt = createCartDto.CreatedAt,
                    IsCheckedOut = createCartDto.isCheckedOut,
                    CartItems = createCartDto.Items.Select(i => new CartItem
                    {
                        Quantity = i.Quantity,
                        LunchboxId = i.LunchboxId,
                        KitId = i.KitId
                    }).ToList()
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                created = true;
            }
            else
            {
                cart.CartItems.Clear();
                foreach (var item in createCartDto.Items)
                {
                    cart.CartItems.Add(new CartItem
                    {
                        Quantity = item.Quantity,
                        LunchboxId = item.LunchboxId,
                        KitId = item.KitId
                    });
                }
                await _context.SaveChangesAsync();
            }

            var response = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                isCheckedOut = cart.IsCheckedOut,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList()
            };

            if (created)
                return CreatedAtAction(nameof(CreateCartWithItems), new { id = cart.Id }, response);

            return Ok(response);
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
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = cart.IsCheckedOut
            };

            return Ok(response);
            //Quando se tenta retorna return Ok(cart);, o ASP.NET Core vai tentar converter esse objeto para JSON. Mas isso vai dar erro de OB Cycle Reference, porque o objeto Cart tem uma lista de CartItems, que por sua vez tem referências a Kit e Lunchbox, que podem ter referências circulares.
        }

        [Authorize]
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
                return BadRequest("Usuário não autenticado");

            // Inclui os itens do carrinho + Kits e Lunchboxes para ProductName
            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsCheckedOut == false);

            if (userCart == null)
                return BadRequest("Carrinho ativo não encontrado");

            // Localiza o item a ser removido
            var itemToRemove = userCart.CartItems
                .FirstOrDefault(i => i.Id == request.CartItemId);

            if (itemToRemove == null)
                return NotFound($"Item com ID {request.CartItemId} não encontrado.");

            // Subtrai a quantidade ou remove
            itemToRemove.Quantity -= request.Quantity;

            if (itemToRemove.Quantity <= 0)
                userCart.CartItems.Remove(itemToRemove);

            await _context.SaveChangesAsync();

            // Cria DTO de resposta
            var response = new CartDto
            {
                Id = userCart.Id,
                CreatedAt = userCart.CreatedAt,
                Items = userCart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = userCart.IsCheckedOut
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromBody] UpdateCartItemQuantityRequest request)
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
                return Unauthorized("Usuário não autenticado");

            if (request.NewQuantity < 1)
                return BadRequest("A quantidade deve ser maior que zero.");

            //obtem carrinho associado ao usuario e que nao esteja concluido.
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (cart == null)
                return NotFound("Carrinho não encontrado.");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == request.CartItemId);

            if (cartItem == null)
                return NotFound($"Item com ID {request.CartItemId} não encontrado no carrinho.");

            cartItem.Quantity = request.NewQuantity;

            await _context.SaveChangesAsync();

            var response = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = cart.IsCheckedOut
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost("remove-full")]
        public async Task<IActionResult> RemoveItemCompletelyFromCart([FromBody] RemoveFromCartRequest request)
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
                return BadRequest("Usuário não autenticado");

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart == null)
                return BadRequest("Carrinho ativo não encontrado");

            var itemToRemove = userCart.CartItems
                .FirstOrDefault(i => i.Id == request.CartItemId);

            if (itemToRemove == null)
                return NotFound($"Item com ID {request.CartItemId} não encontrado.");

            // Remove completamente do carrinho
            userCart.CartItems.Remove(itemToRemove);
            await _context.SaveChangesAsync();

            // Monta o DTO de resposta com os itens restantes
            var response = new CartDto
            {
                Id = userCart.Id,
                UserId = userCart.UserId,
                CreatedAt = userCart.CreatedAt,
                Items = userCart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    ProductName = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList(),
                isCheckedOut = userCart.IsCheckedOut
            };

            return Ok(response);
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            //encontra carrinho com o id enviado e que nao tenha sido finalizado
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.IsCheckedOut == false);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
