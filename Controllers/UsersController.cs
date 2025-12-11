using Google.Apis.Auth;
using MarmitaBackend.Configurations;
using MarmitaBackend.DTOs;
using MarmitaBackend.Models;
using MarmitaBackend.Provider;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly ITenantProvider _tenantProvider;


        public UsersController(ApplicationDbContext context, IConfiguration configuration, JwtConfig jwtConfig, ITenantProvider tenantProvider)
        {
            _context = context;
            _configuration = configuration;
            _jwtConfig = jwtConfig;
            _tenantProvider = tenantProvider;

        }


        [Route("register")]
        [HttpPost]
        public IActionResult Register([FromBody] RegisterDto dto)
        {

            Console.WriteLine($"TenantProvider.TenantId = {_tenantProvider.TenantId}");



            if (dto == null)
            {
                return BadRequest("Invalid user.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna os erros de validação
            }

            var emailExists = _context.Users.Any((u) => u.Email == dto.Email);

            if (emailExists)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                TenantId = _tenantProvider.TenantId
            };

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
                // TENANT MULTITENANT
                int tenantId = _tenantProvider.TenantId;

                Console.WriteLine($"[GoogleLogin] Tenant atual = {tenantId}");

                using var client = new HttpClient();

                var values = new Dictionary<string, string>
        {
            { "code", dto.Code },
            { "client_id", dto.ClientId },
            { "client_secret", dto.ClientSecret },
            { "redirect_uri", dto.RedirectUri },
            { "grant_type", "authorization_code" }
        };


                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Unauthorized(new
                    {
                        message = "Erro ao trocar code por token",
                        error = responseString
                    });
                }

                var tokenData = JsonConvert.DeserializeObject<dynamic>(responseString);
                string idToken = tokenData.id_token;

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { dto.ClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                // MULTITENANT:
                // Agora o filtro global garante que só busque NO TENANT ATUAL
                var user = await _context
                    .Users
                    .FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Name = payload.Name,
                        Password = null,
                        isAdmin = false,
                        TenantId = tenantId  // ⬅ MULTITENANT OK
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
                return Unauthorized(new
                {
                    message = "Token inválido",
                    error = ex.Message
                });
            }
        }


        [HttpGet("me")]
        public IActionResult ValidateToken()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                    return Unauthorized(new { msg = "Token não fornecido ou inválido" });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtConfig.Key);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    ValidAudience = _jwtConfig.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // garante que é um JWT
                if (validatedToken is not JwtSecurityToken)
                    return Unauthorized(new { msg = "Token inválido" });

                // ============ MULTITENANCY CHECK ============
                var tokenTenantId = principal.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value;

                if (tokenTenantId == null)
                    return Unauthorized(new { msg = "Token sem TenantId" });

                if (tokenTenantId != _tenantProvider.TenantId.ToString())
                    return Unauthorized(new { msg = "Token pertence a outro tenant" });
                // ============================================

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var name = principal.FindFirstValue(ClaimTypes.Name);
                var email = principal.FindFirstValue(ClaimTypes.Email);
                var role = principal.FindFirstValue(ClaimTypes.Role);
                var tenantId = principal.FindFirstValue("tenantId");

                return Ok(new
                {
                    msg = "Token válido",
                    user = new
                    {
                        id = userId,
                        name = name,
                        email = email,
                        isAdmin = role == "Admin",
                        tenantId = tenantId
                    }
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { msg = "Token inválido", error = ex.Message });
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
        new Claim(ClaimTypes.Role, user.isAdmin ? "Admin" : "User"),

        //multitenant
        new Claim("tenantId", user.TenantId.ToString())
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
            var user = await _context.Users
                .Where(u => u.TenantId == _tenantProvider.TenantId)
                .FirstOrDefaultAsync(u => u.Id == id);

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
