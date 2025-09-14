using Google.Apis.Auth;
using MarmitaBackend.Configurations;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;


namespace MarmitaBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //GoogleAuth
            builder.Services.Configure<GoogleAuthSettings>(
            builder.Configuration.GetSection("Authentication:Google"));

            // Add services to the container
            builder.Services.AddControllers();

            //Adding connection string to the database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

            //The jwtConfig variable now contains a JwtConfig object with the values extracted from appsettings.json, ready to be used to configure JWT authentication.
            var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
            if (jwtConfig == null || string.IsNullOrEmpty(jwtConfig.Key))
            {
                throw new InvalidOperationException("JWT configuration is missing or invalid.");
            }
            // Register the JwtConfig as a singleton service
            builder.Services.AddSingleton(jwtConfig); // você já tem esse objeto criado antes

            // Convert the JWT key from string to byte array
            var key = Encoding.ASCII.GetBytes(jwtConfig.Key);


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //cors
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            //adding authentication and configuring JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Set to true in production
                    options.SaveToken = true; // Salvar o token no contexto da requisição. Define se o token deve ser salvo no contexto da requisição após a validação.

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // Valida o emissor do token (Define se o emissor (issuer) do token deve ser validado)
                        ValidIssuer = jwtConfig.Issuer, // Emissor permitido (definido no appsettings.json)

                        ValidateAudience = true, // Valida o público do token
                        ValidAudience = jwtConfig.Audience, // Público permitido (definido no appsettings.json)

                        ValidateIssuerSigningKey = true, // Verifica a chave secreta e define se a chave de assinatura do token deve ser validada, obrigatoria
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),// Chave secreta usada para validar o token, obrigatoria

                        ValidateLifetime = true, // Verifica se o token expirou, Define se o tempo de vida do token (validade) deve ser verificado. Isso garante que tokens expirados não sejam aceitos.
                        ClockSkew = TimeSpan.Zero // Remove tempo extra para expiração do token Por padrão, o ClockSkew é de 5 minutos. Definir como TimeSpan.Zero remove essa tolerância.
                    };
                });



            var app = builder.Build();

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseAuthentication(); // Enable authentication middleware
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
