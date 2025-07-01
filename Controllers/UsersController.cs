using MarmitaBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MarmitaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [Route("register")]
        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna os erros de validação
            }

            var emailExists = _context.Users.Any((u) => u.Email == user.Email);

            if (emailExists)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            _context.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = _context.Users
       .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (existingUser == null)
            {
                return Unauthorized("Invalid email or password.");
            }
           
                //usando o método GenerateJwtToken para criar o token JWT
                var token = GenerateJwtToken(existingUser);
                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        existingUser.Id,
                        existingUser.Name,
                        existingUser.Email,
                        existingUser.isAdmin
                    }
                }); // Retorna o usuário autenticado + token JWT
            


        }

        private string GenerateJwtToken(User user)
        {
            // Lê a chave do appsettings.json via IConfiguration
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.isAdmin ? "Admin" : "User")
        };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            // Busca o usuário no banco
            var existingUser = await _context.Users.FindAsync(id);

            // Se não existir, retorna 404
            if (existingUser == null)
                return NotFound("Usuário não encontrado.");

            // Atualiza os campos necessários
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.Password = updatedUser.Password;
            existingUser.isAdmin = updatedUser.isAdmin;

            try
            {
                await _context.SaveChangesAsync(); // salva a alteração no banco
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Erro ao atualizar usuário: {ex.Message}");
            }

            return NoContent(); // 204: atualizado com sucesso, sem corpo de resposta
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
