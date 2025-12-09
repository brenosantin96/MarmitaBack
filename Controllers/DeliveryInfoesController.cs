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
                    .ToListAsync();

                return Ok(deliveryInfos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult<DeliveryInfo>> PostDeliveryInfo(DeliveryInfoCreateDto dto)
        {

            try
            {
                var userId = UserHelper.GetUserId(User);

                if (userId == null)
                {
                    return Unauthorized("Usuario não autenticado");
                }

                var deliveryInfo = new DeliveryInfo
                {
                    TenantId = _tenantProvider.TenantId,
                    UserId = userId.Value,
                    AddressId = dto.AddressId,
                    DeliveryType = dto.DeliveryType,
                    DeliveryDate = dto.DeliveryDate,
                    DeliveryPeriod = dto.DeliveryPeriod,
                    CanLeaveAtDoor = dto.CanLeaveAtDoor

                };

                _context.DeliveryInfo.Add(deliveryInfo);
                await _context.SaveChangesAsync();

                // Mapeia para DTO de resposta
                var deliveryInfoDto = new DeliveryInfoDto
                {
                    Id = deliveryInfo.Id,
                    UserId = deliveryInfo.UserId,
                    AddressId = deliveryInfo.AddressId,
                    DeliveryType = deliveryInfo.DeliveryType,
                    DeliveryDate = deliveryInfo.DeliveryDate,
                    DeliveryPeriod = deliveryInfo.DeliveryPeriod,
                    CanLeaveAtDoor = deliveryInfo.CanLeaveAtDoor
                };

                return CreatedAtAction(nameof(GetDeliveryInfo), new { id = deliveryInfo.Id }, deliveryInfoDto);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}

