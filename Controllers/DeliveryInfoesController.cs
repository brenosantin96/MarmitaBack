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
    public class DeliveryInfoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public DeliveryInfoesController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }


        // GET: api/DeliveryInfoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryInfo>> GetDeliveryInfo(int id)
        {
            var deliveryInfo = await _context.DeliveryInfo
                .Where(di => di.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync(di => di.Id == id);

            if (deliveryInfo == null)
            {
                return NotFound();
            }

            return deliveryInfo;
        }

        // GET: api/DeliveryInfoes/GetDeliveryInfoByUserId/5
        [Authorize]
        [HttpGet("GetDeliveryInfoByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<DeliveryInfo>>> GetDeliveryInfoByUserId(int userId)
        {
            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                if (loggedUserId == null)
                    return Unauthorized("Usuário não autenticado.");

                if (loggedUserId != userId)
                    return StatusCode(403, "Você não tem permissão para acessar dados de outro usuário.");

                var deliveryInfos = await _context.DeliveryInfo
                    .Where(di =>
                        di.TenantId == _tenantProvider.TenantId &&
                        di.UserId == userId
                    )
                    .OrderByDescending(di => di.DeliveryDate)
                    .ToListAsync(); //retorna uma lista de deliveryInfos desse usuario



                return Ok(deliveryInfos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/DeliveryInfoes/by-cart/5
        [Authorize]
        [HttpGet("by-cart/{cartId}")]
        public async Task<ActionResult<DeliveryInfoDto>> GetDeliveryInfoByCartId(int cartId)
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                // 1 - Validar se o carrinho pertence ao usuário logado
                var cart = await _context.Carts
                    .Where(c =>
                        c.Id == cartId &&
                        c.UserId == userId &&
                        c.TenantId == _tenantProvider.TenantId
                    )
                    .FirstOrDefaultAsync();

                if (cart == null)
                    return NotFound("Carrinho não encontrado ou não pertence ao usuário.");

                // 2 - Buscar DeliveryInfo do carrinho
                var deliveryInfo = await _context.DeliveryInfo
                    .Where(di =>
                        di.CartId == cart.Id &&
                        di.TenantId == _tenantProvider.TenantId
                    )
                    .FirstOrDefaultAsync();

                if (deliveryInfo == null)
                    return NotFound("DeliveryInfo não encontrado para este carrinho.");

                // 3 - Mapear DTO
                var response = new DeliveryInfoDto
                {
                    Id = deliveryInfo.Id,
                    UserId = deliveryInfo.UserId,
                    CartId = deliveryInfo.CartId,
                    AddressId = deliveryInfo.AddressId,
                    DeliveryType = deliveryInfo.DeliveryType,
                    DeliveryDate = deliveryInfo.DeliveryDate,
                    DeliveryPeriod = deliveryInfo.DeliveryPeriod,
                    CanLeaveAtDoor = deliveryInfo.CanLeaveAtDoor
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        //Cart > DeliveryInfo > Payment > Order
        // GET: api/DeliveryInfoes/current
        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<DeliveryInfoDto>> GetDeliveryInfoByCurrentCart()
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                // 1- Buscar o carrinho ativo do usuário
                var cart = await _context.Carts
                    .Where(c =>
                        c.TenantId == _tenantProvider.TenantId &&
                        c.UserId == userId &&
                        !c.IsCheckedOut
                    )
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                if (cart == null)
                    return NotFound("Carrinho ativo não encontrado.");

                // 2- Buscar o DeliveryInfo desse carrinho
                var deliveryInfo = await _context.DeliveryInfo
                    .Where(di =>
                        di.TenantId == _tenantProvider.TenantId &&
                        di.CartId == cart.Id
                    )
                    .FirstOrDefaultAsync();

                if (deliveryInfo == null)
                    return NotFound("DeliveryInfo não encontrado para este carrinho.");

                // 3- Mapear DTO
                var response = new DeliveryInfoDto
                {
                    Id = deliveryInfo.Id,
                    UserId = deliveryInfo.UserId,
                    CartId = deliveryInfo.CartId,
                    AddressId = deliveryInfo.AddressId,
                    DeliveryType = deliveryInfo.DeliveryType,
                    DeliveryDate = deliveryInfo.DeliveryDate,
                    DeliveryPeriod = deliveryInfo.DeliveryPeriod,
                    CanLeaveAtDoor = deliveryInfo.CanLeaveAtDoor
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [Authorize]
        [HttpPost]
        public async Task<ActionResult<DeliveryInfoDto>> PostDeliveryInfo([FromBody] DeliveryInfoCreateUpdateDto dto)
        {
            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                    return Unauthorized("Usuário não autenticado.");

                // 1- Validar carrinho
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c =>
                        c.Id == dto.CartId &&
                        c.UserId == userId &&
                        c.TenantId == _tenantProvider.TenantId &&
                        !c.IsCheckedOut
                    );

                if (cart == null)
                    return NotFound("Carrinho inválido ou já finalizado.");

                // 2- Verificar se já existe DeliveryInfo para esse carrinho
                var deliveryInfo = await _context.DeliveryInfo
                    .FirstOrDefaultAsync(di =>
                        di.CartId == cart.Id &&
                        di.TenantId == _tenantProvider.TenantId
                    );

                // 3 - Create ou Update
                if (deliveryInfo == null)
                {
                    deliveryInfo = new DeliveryInfo
                    {
                        TenantId = _tenantProvider.TenantId,
                        UserId = userId.Value,
                        CartId = cart.Id
                    };

                    _context.DeliveryInfo.Add(deliveryInfo);
                }

                // 4 -  Atualizar dados
                if (dto.AddressId.HasValue)
                    deliveryInfo.AddressId = dto.AddressId;

                if (dto.DeliveryDate.HasValue)
                    deliveryInfo.DeliveryDate = dto.DeliveryDate.Value;

                if (dto.DeliveryPeriod != null)
                    deliveryInfo.DeliveryPeriod = dto.DeliveryPeriod;

                deliveryInfo.DeliveryType = dto.DeliveryType;
                deliveryInfo.CanLeaveAtDoor = dto.CanLeaveAtDoor;

                await _context.SaveChangesAsync();

                // 5- Response DTO
                var response = new DeliveryInfoDto
                {
                    Id = deliveryInfo.Id,
                    UserId = deliveryInfo.UserId,
                    CartId = deliveryInfo.CartId,
                    AddressId = deliveryInfo.AddressId,
                    DeliveryType = deliveryInfo.DeliveryType,
                    DeliveryDate = deliveryInfo.DeliveryDate,
                    DeliveryPeriod = deliveryInfo.DeliveryPeriod,
                    CanLeaveAtDoor = deliveryInfo.CanLeaveAtDoor
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}

