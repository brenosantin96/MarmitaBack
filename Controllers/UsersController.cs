using Google.Apis.Auth;
using MarmitaBackend.Configurations;
using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Common;
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
        private readonly JwtConfig _jwtConfig;


        public UsersController(ApplicationDbContext context, IConfiguration configuration, JwtConfig jwtConfig)
        {
            _context = context;
            _configuration = configuration;
            _jwtConfig = jwtConfig;
            
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

            //check if the user exists in the database
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

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {

            try
            {

                using var client = new HttpClient();

                var values = new Dictionary<string, string>
                {
                    {"code", dto.Code},
                    {"client_id", Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")},
                    {"client_secret", Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") },
                    { "redirect_uri", "http://localhost:3000" }, // precisa ser o mesmo usado no front
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Unauthorized(new { message = "Erro ao trocar code por token", error = responseString });
                }

                var tokenData = JsonConvert.DeserializeObject<dynamic>(responseString);
                Console.WriteLine($"responseString: {responseString}");
                Console.WriteLine($"TOKEN DATA: {tokenData}");

                string idToken = tokenData.id_token;

                // Validar ID Token
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { "486411282466-hki87kvtqucbgvgk964h91c2tpnvi6j0.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                //payload vai ter email, name, picture etc, buscar esse usuario no banco e se nao existir, criarlo.
                var user = _context.Users.FirstOrDefault(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Name = payload.Name,
                        Password = null,
                        isAdmin = false
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var jwt = GenerateJwtToken(user);

                return Ok(new
                {
                    Token = jwt,
                    User = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        user.isAdmin
                    }
                });

            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Token inválido", error = ex.Message });

            }
        }


        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define os claims do token, pegados do usuário autenticado
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.isAdmin ? "Admin" : "User")
    };

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.TokenValidityMins),
                signingCredentials: creds
            );

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
