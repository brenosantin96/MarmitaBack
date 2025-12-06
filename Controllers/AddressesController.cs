using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public AddressesController(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;

        }

        // GET: api/Addresses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            return await _context.Addresses.Where(a => a.TenantId == _tenantProvider.TenantId).ToListAsync();

        }

        // GET: api/Addresses/admin/all
        [HttpGet("admin/all")]
        public async Task<ActionResult<IEnumerable<Address>>> GetAdminAddresses()
        {
            var adminAddresses = await _context.Addresses
                .Include(a => a.User)
                .Where(a => a.User.isAdmin == true && a.TenantId == _tenantProvider.TenantId)
                .ToListAsync();

            return Ok(adminAddresses);
        }

        // GET: api/Addresses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(int id)
        {
            var address = await _context.Addresses.Where(a => a.Id == id && a.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();


            if (address == null)
            {
                return NotFound();
            }

            return address;
        }


        // GET: api/Addresses/GetAddressByUserId/16
        [HttpGet("GetAddressByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddressesByUserId(int userId)
        {

            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                if (loggedUserId == null)
                {
                    return Unauthorized("Usuário não autenticado.");
                }

                // Garante que o usuário autenticado só pode ver o próprio carrinho
                if (loggedUserId != userId)
                {
                    return Forbid("Você não tem permissão para ver endereços de outro usuario.");
                }

                var addresses = await _context.Addresses.Where(a => a.TenantId == _tenantProvider.TenantId && a.UserId == loggedUserId).ToListAsync();
                return Ok(addresses);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                return BadRequest(ex.Message);
            }



        }

        // POST: api/Addresses
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Address>> PostAddress(Address address)
        {
            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                // Carrega o usuário logado garantindo que pertence ao tenant atual
                var loggedUser = await _context.Users
                    .Where(u => u.Id == loggedUserId && u.TenantId == _tenantProvider.TenantId)
                    .FirstOrDefaultAsync();

                if (loggedUser == null)
                {
                    return Unauthorized("Usuário logado não encontrado.");
                }

                var isUserAdmin = loggedUser.isAdmin;

                // Usuário comum NUNCA pode criar endereço para outro UserId
                if (!isUserAdmin && address.UserId != loggedUserId)
                {
                    return BadRequest("Você não pode criar um endereço para outro usuário.");
                }

                // Seta valores obrigatórios
                address.TenantId = _tenantProvider.TenantId;
                address.CreatedAt = DateTime.UtcNow;
                address.UpdatedAt = DateTime.UtcNow;

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetAddress", new { id = address.Id }, address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao criar endereço.",
                    details = ex.Message
                });
            }
        }



        // PUT: api/Addresses/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(int id, AddressUpdateDto dto)
        {
            //NO POSTMAN AO ENVIAR USERID INCORRETO, DA ERRO MAS CAI NO CATCH... ESPECIFICAR MENSAGEM CORRETA

            var loggedUserId = UserHelper.GetUserId(User);

            // Carrega usuário logado garantindo tenant
            var loggedUser = await _context.Users
                .Where(u => u.Id == loggedUserId && u.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync();

            if (loggedUser == null)
                return Unauthorized("Usuário logado não encontrado.");

            var isAdmin = loggedUser.isAdmin;

            // Buscar o endereço válido neste tenant
            var existingAddress = await _context.Addresses
                .Where(a => a.TenantId == _tenantProvider.TenantId && a.Id == id)
                .FirstOrDefaultAsync();

            if (existingAddress == null)
                return NotFound("Endereço não encontrado.");

            // Usuário comum só pode editar seus próprios endereços
            if (!isAdmin && existingAddress.UserId != loggedUserId)
                return BadRequest("Você não tem permissão para editar este endereço.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Atualizações parciais
            if (dto.UserId.HasValue)
            {
                // Somente admin pode alterar UserId
                if (!isAdmin && dto.UserId.Value != loggedUserId)
                    return BadRequest("Usuário comum não pode alterar o UserId do endereço.");

                existingAddress.UserId = dto.UserId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.ZipCode))
                existingAddress.ZipCode = dto.ZipCode;

            if (!string.IsNullOrWhiteSpace(dto.Street))
                existingAddress.Street = dto.Street;

            if (!string.IsNullOrWhiteSpace(dto.City))
                existingAddress.City = dto.City;

            if (!string.IsNullOrWhiteSpace(dto.State))
                existingAddress.State = dto.State;

            if (!string.IsNullOrWhiteSpace(dto.Neighborhood))
                existingAddress.Neighborhood = dto.Neighborhood;

            if (!string.IsNullOrWhiteSpace(dto.Number))
                existingAddress.Number = dto.Number;

            if (!string.IsNullOrWhiteSpace(dto.Complement))
                existingAddress.Complement = dto.Complement;

            // Timestamp
            existingAddress.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao salvar no banco de dados.",
                    details = ex.Message
                });
            }

            return NoContent();
        }



        // DELETE: api/Addresses/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var loggedUserId = UserHelper.GetUserId(User);

                var loggedUser = await _context.Users
                    .Where(u => u.Id == loggedUserId && u.TenantId == _tenantProvider.TenantId)
                    .FirstOrDefaultAsync();

                if (loggedUser == null)
                {
                    return Unauthorized("Usuário não encontrado.");
                }

                var isUserAdmin = loggedUser.isAdmin;

                // Buscar o endereço garantindo que pertence ao tenant
                var address = await _context.Addresses
                    .Where(a => a.Id == id && a.TenantId == _tenantProvider.TenantId)
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return NotFound("Endereço não encontrado.");
                }

                // Se não for admin, só pode deletar endereço do próprio usuário
                if (!isUserAdmin && address.UserId != loggedUserId)
                {
                    return BadRequest("Você não tem permissão para deletar endereços de outro usuário.");
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao excluir endereço.",
                    details = ex.Message
                });
            }
        }



    }
}
