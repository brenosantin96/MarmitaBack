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
using System.Threading.Tasks;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public CartsController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CartDto>> GetCart(int id)
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                var cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                    return StatusCode(404, "Carrinho nao encontrado.");


                var response = new CartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    CreatedAt = cart.CreatedAt,
                    isCheckedOut = cart.IsCheckedOut,
                    CartItems = cart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        LunchboxId = ci.LunchboxId,
                        KitId = ci.KitId,
                        Quantity = ci.Quantity,
                        Name = ci.Kit?.Name ?? ci.Lunchbox?.Name,
                        ImageUrl = ci.Kit?.ImageUrl ?? ci.Lunchbox?.ImageUrl,
                        PortionGram = ci.Lunchbox?.PortionGram,
                        Price = ci.Kit?.Price ?? ci.Lunchbox?.Price
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }



        // GET: api/Carts/GetCartByUserId/5
        [HttpGet("GetCartByUserId/{userId}")]
        public async Task<ActionResult<CartDto>> GetCartByUserId(int userId)
        {
            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                if (loggedUserId == null)
                    return Unauthorized("Usuário não autenticado.");

                if (loggedUserId != userId)
                    return StatusCode(403, "Você não tem permissão para acessar o carrinho de outro usuário.");


                var cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

                if (cart == null)
                    return NotFound("Carrinho não encontrado.");

                var response = new CartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    CreatedAt = cart.CreatedAt,
                    isCheckedOut = cart.IsCheckedOut,
                    CartItems = cart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        LunchboxId = ci.LunchboxId,
                        KitId = ci.KitId,
                        Name = ci.Kit?.Name ?? ci.Lunchbox?.Name,
                        ImageUrl = ci.Kit?.ImageUrl ?? ci.Lunchbox?.ImageUrl,
                        PortionGram = ci.Lunchbox?.PortionGram,
                        Price = ci.Kit?.Price ?? ci.Lunchbox?.Price
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }




        // POST: api/Carts/create2
        [Authorize]
        [HttpPost("create2")]
        public async Task<IActionResult> CreateCartWithItems([FromBody] CreateCartDto createCartDto)
        {
            var userId = UserHelper.GetUserId(User); // valida usuário

            if (userId == null)
                return Unauthorized("Usuário não autenticado.");

            var cart = await _context.Carts
                .Where(c => c.TenantId == _tenantProvider.TenantId)
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
                    CartItems = createCartDto.CartItems.Select(i => new CartItem
                    {
                        Quantity = i.Quantity,
                        LunchboxId = i.LunchboxId,
                        KitId = i.KitId,
                        TenantId = _tenantProvider.TenantId
                    }).ToList(),
                    TenantId = _tenantProvider.TenantId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                // recarrega o carrinho com relacionamentos
                cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id);

                created = true;
            }
            else
            {
                cart.CartItems.Clear();
                foreach (var item in createCartDto.CartItems)
                {
                    cart.CartItems.Add(new CartItem
                    {
                        Quantity = item.Quantity,
                        LunchboxId = item.LunchboxId,
                        KitId = item.KitId,
                        TenantId = _tenantProvider.TenantId
                    });
                }
                await _context.SaveChangesAsync();

                // recarrega o carrinho atualizado
                cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id);
            }

            var response = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                isCheckedOut = cart.IsCheckedOut,
                CartItems = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    Name = ci.Kit?.Name ?? ci.Lunchbox?.Name,
                    ImageUrl = ci.Kit?.ImageUrl ?? ci.Lunchbox?.ImageUrl,
                    PortionGram = ci.Lunchbox?.PortionGram,
                    Price = ci.Kit?.Price ?? ci.Lunchbox?.Price

                }).ToList()
            };

            if (created)
                return CreatedAtAction(nameof(CreateCartWithItems), new { id = cart.Id }, response);

            return Ok(response);
        }

        //adicionar item no carrinho, de momento nao estou usando esse controller no front mas pode ser que uso depois.
        // POST: api/Carts/add
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                if (request.LunchboxId == null && request.KitId == null)
                    return BadRequest("Você deve fornecer um KitId ou LunchboxId.");

                if (request.Quantity <= 0)
                    return BadRequest("A quantidade deve ser maior que zero.");

                //  MULTITENANCY – sempre filtrar pelo TenantId
                var cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

                //  Se o carrinho não existe → criar novo
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId.Value,
                        CreatedAt = DateTime.UtcNow,
                        IsCheckedOut = false,
                        TenantId = _tenantProvider.TenantId,
                        CartItems = new List<CartItem>()
                    };

                    _context.Carts.Add(cart);
                }

                //  Verificar se item já existe no carrinho
                var existingItem = cart.CartItems.FirstOrDefault(ci =>
                    (request.KitId != null && ci.KitId == request.KitId) ||
                    (request.LunchboxId != null && ci.LunchboxId == request.LunchboxId));

                if (existingItem != null)
                {
                    // Atualiza quantidade
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    // Adiciona novo item com TenantId
                    cart.CartItems.Add(new CartItem
                    {
                        KitId = request.KitId,
                        LunchboxId = request.LunchboxId,
                        Quantity = request.Quantity,
                        TenantId = _tenantProvider.TenantId
                    });
                }

                await _context.SaveChangesAsync();

                //  Recarregar carrinho com relacionamentos (kit e lunchbox)
                var updatedCart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems).ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems).ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id);

                //  Montar DTO de resposta
                var response = new CartDto
                {
                    Id = updatedCart.Id,
                    UserId = updatedCart.UserId,
                    CreatedAt = updatedCart.CreatedAt,
                    isCheckedOut = updatedCart.IsCheckedOut,
                    CartItems = updatedCart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        KitId = ci.KitId,
                        LunchboxId = ci.LunchboxId,
                        Name = ci.Kit?.Name ?? ci.Lunchbox?.Name,
                        ImageUrl = ci.Kit?.ImageUrl ?? ci.Lunchbox?.ImageUrl,
                        PortionGram = ci.Lunchbox?.PortionGram,
                        Price = ci.Kit?.Price ?? ci.Lunchbox?.Price
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }


        [Authorize]
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                if (request.CartItemId <= 0)
                    return BadRequest("CartItemId inválido.");

                if (request.Quantity <= 0)
                    return BadRequest("A quantidade deve ser maior que zero.");

                // MULTITENANCY - filtro obrigatório
                var cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

                if (cart == null)
                    return NotFound("Carrinho ativo não encontrado.");

                // Localiza o item dentro do carrinho e respeitando Tenant
                var item = cart.CartItems.FirstOrDefault(ci =>
                    ci.Id == request.CartItemId &&
                    ci.TenantId == _tenantProvider.TenantId);

                if (item == null)
                    return NotFound($"Item com ID {request.CartItemId} não encontrado no carrinho.");

                // Atualiza quantidade
                item.Quantity -= request.Quantity;

                // Se zerou ou ficou negativo → remove
                if (item.Quantity <= 0)
                    _context.CartItems.Remove(item);

                await _context.SaveChangesAsync();

                // RECARREGA carrinho com inclusões para DTO correto
                var updatedCart = await _context.Carts
                    .Where(c => c.Id == cart.Id && c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems).ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems).ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync();

                // Monta DTO
                var response = new CartDto
                {
                    Id = updatedCart.Id,
                    UserId = updatedCart.UserId,
                    CreatedAt = updatedCart.CreatedAt,
                    isCheckedOut = updatedCart.IsCheckedOut,
                    CartItems = updatedCart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        KitId = ci.KitId,
                        LunchboxId = ci.LunchboxId,
                        Name = ci.Kit?.Name ?? ci.Lunchbox?.Name,
                        ImageUrl = ci.Kit?.ImageUrl ?? ci.Lunchbox?.ImageUrl,
                        PortionGram = ci.Lunchbox?.PortionGram,
                        Price = ci.Kit?.Price ?? ci.Lunchbox?.Price
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }


        [Authorize]
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromBody] UpdateCartItemQuantityRequest request)
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                if (request.NewQuantity < 1)
                    return BadRequest("A quantidade deve ser maior que zero.");

                // MULTITENANCY — filtro obrigatório pelo TenantId
                var cart = await _context.Carts
                    .Where(c => c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

                if (cart == null)
                    return NotFound("Carrinho não encontrado.");

                // Localiza o item respeitando Tenant
                var cartItem = cart.CartItems
                    .FirstOrDefault(ci => ci.Id == request.CartItemId &&
                                          ci.TenantId == _tenantProvider.TenantId);

                if (cartItem == null)
                    return NotFound($"Item com ID {request.CartItemId} não encontrado no carrinho.");

                // Atualiza quantidade
                cartItem.Quantity = request.NewQuantity;

                await _context.SaveChangesAsync();

                // RECARREGA carrinho com relacionamentos
                var updatedCart = await _context.Carts
                    .Where(c => c.Id == cart.Id && c.TenantId == _tenantProvider.TenantId)
                    .Include(c => c.CartItems).ThenInclude(ci => ci.Lunchbox)
                    .Include(c => c.CartItems).ThenInclude(ci => ci.Kit)
                    .FirstOrDefaultAsync();

                var response = new CartDto
                {
                    Id = updatedCart.Id,
                    UserId = updatedCart.UserId,
                    CreatedAt = updatedCart.CreatedAt,
                    isCheckedOut = updatedCart.IsCheckedOut,
                    CartItems = updatedCart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        KitId = ci.KitId,
                        LunchboxId = ci.LunchboxId,
                        Name = ci.Kit?.Name ?? ci.Lunchbox?.Name,
                        ImageUrl = ci.Kit?.ImageUrl ?? ci.Lunchbox?.ImageUrl,
                        PortionGram = ci.Lunchbox?.PortionGram,
                        Price = ci.Kit?.Price ?? ci.Lunchbox?.Price
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }


        [Authorize]
        [HttpPost("remove-full")]
        public async Task<IActionResult> RemoveItemCompletelyFromCart([FromBody] RemoveFromCartRequest request)
        {
            var userId = UserHelper.GetUserId(User);

            if (userId == null)
                return BadRequest("Usuário não autenticado.");

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);

            if (userCart == null)
                return BadRequest("Carrinho ativo não encontrado.");

            var itemToRemove = userCart.CartItems
                .FirstOrDefault(i => i.Id == request.CartItemId);

            if (itemToRemove == null)
                return NotFound($"Item com ID {request.CartItemId} não encontrado no carrinho.");

            // Remove completamente usando o DbSet (mais seguro e consistente)
            _context.CartItems.Remove(itemToRemove);
            await _context.SaveChangesAsync();

            // Recarrega o carrinho atualizado
            var updatedCart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Kit)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Lunchbox)
                .FirstOrDefaultAsync(c => c.Id == userCart.Id);

            // Monta o DTO atualizado
            var response = new CartDto
            {
                Id = updatedCart.Id,
                UserId = updatedCart.UserId,
                CreatedAt = updatedCart.CreatedAt,
                isCheckedOut = updatedCart.IsCheckedOut,

                CartItems = updatedCart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    Quantity = ci.Quantity,
                    KitId = ci.KitId,
                    LunchboxId = ci.LunchboxId,
                    Name = ci.Kit?.Name ?? ci.Lunchbox?.Name
                }).ToList()
            };

            return Ok(response);
        }


        // DELETE: api/Carts/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var tenantId = _tenantProvider.TenantId;

            var cart = await _context.Carts
                .Where(c => c.Id == id
                         && c.TenantId == tenantId
                         && !c.IsCheckedOut)
                .FirstOrDefaultAsync();

            if (cart == null)
                return NotFound($"Carrinho {id} não encontrado ou já finalizado.");

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }



    /* CREATE ja nao estou utilizando.

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
               CartItems = cart.CartItems.Select(ci => new CartItemDto
               {
                   Id = ci.Id,
                   Quantity = ci.Quantity,
                   KitId = ci.KitId,
                   LunchboxId = ci.LunchboxId,
                   Name = ci.Kit?.Name ?? ci.Lunchbox?.Name
               }).ToList(),
               isCheckedOut = cart.IsCheckedOut
           };

           if (created)
               return CreatedAtAction(nameof(CreateCart), new { id = cart.Id }, response);

           return Ok(response);
       }

       */


}
