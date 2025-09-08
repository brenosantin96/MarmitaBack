﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarmitaBackend.Models;
using Microsoft.VisualBasic;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Addresses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            return await _context.Addresses.ToListAsync();
        }

        // GET: api/Addresses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(int id)
        {
            var address = await _context.Addresses.FindAsync(id);

            if (address == null)
            {
                return NotFound();
            }

            return address;
        }

        // PUT: api/Addresses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(int id, Address address)
        {
         

            var existingAddress = await _context.Addresses.FindAsync(id);
            if (existingAddress == null)
            {
                return NotFound("Address not found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the existing address with the new values
            existingAddress.UserId = address.UserId;
            existingAddress.ZipCode = address.ZipCode;
            existingAddress.Street = address.Street;
            existingAddress.City = address.City;
            existingAddress.State = address.State;
            existingAddress.Neighborhood = address.Neighborhood;
            existingAddress.Number = address.Number;
            existingAddress.Complement = address.Complement;
            existingAddress.UpdatedAt = DateTime.UtcNow; // Update the timestamp
            


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Aqui você pode logar o erro, retornar uma mensagem personalizada, etc.
                return StatusCode(500, new { message = "Erro ao salvar no banco de dados.", details = ex.Message });
            }

            return NoContent();
        }

        // POST: api/Addresses
        [HttpPost]
        public async Task<ActionResult<Address>> PostAddress(Address address)
        {
            address.CreatedAt = DateTime.UtcNow; // Set the creation timestamp
            address.UpdatedAt = DateTime.UtcNow; // Set the update timestamp
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            

            return CreatedAtAction("GetAddress", new { id = address.Id }, address);
        }

        // DELETE: api/Addresses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }


    }
}
