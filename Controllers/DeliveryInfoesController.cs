using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Utils;
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

        public DeliveryInfoesController(ApplicationDbContext context)
        {
            _context = context;
        }

     
        // GET: api/DeliveryInfoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryInfo>> GetDeliveryInfo(int id)
        {
            var deliveryInfo = await _context.DeliveryInfo.FindAsync(id);

            if (deliveryInfo == null)
            {
                return NotFound();
            }

            return deliveryInfo;
        }

        [HttpPost]
        public async Task<ActionResult<DeliveryInfo>> PostDeliveryInfo(DeliveryInfo deliveryInfo)
        {

            var userId = UserHelper.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            _context.DeliveryInfo.Add(deliveryInfo);
            await _context.SaveChangesAsync();

            //retorno de dto para evitar erros de cycle object e desserializacao 
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

            return CreatedAtAction("GetDeliveryInfo", new { id = deliveryInfo.Id }, deliveryInfoDto);
        }

    }
}

