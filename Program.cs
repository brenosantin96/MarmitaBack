using MarmitaBackend.Configurations;
using MarmitaBackend.Provider;
using MarmitaBackend.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using System.Text.Json;


namespace MarmitaBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(7192); // Escuta todas as interfaces na porta 7192
            });


            //Configurar cultura padrao para en-US (ponto como separador decimal)
            var defaultCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            // Add services to the container
            builder.Services.AddControllers();

            //aplicando scoped DI requisicao web.
            builder.Services.AddHttpContextAccessor();            // Necessário para acessar HttpContext
            builder.Services.AddScoped<TenantAccessor>();

            builder.Services.AddScoped<ITenantProvider, TenantProvider>(); // Um TenantProvider por request

            //Adding connection string to the database
            // ja estou usando DI do TenantProvider dentro do ApplicationDbContext.
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
            var key = Encoding.ASCII.GetBytes(jwtConfig.Key);  // Convert the JWT key from string to byte array

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

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse(); // impede resposta padrão

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            await context.Response.WriteAsync(
                                JsonSerializer.Serialize(new
                                {
                                    error = "Não autenticado",
                                    message = "Token JWT ausente ou inválido"
                                })
                            );
                        }

                    };

                });


            //swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //cors
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",      // Frontend rodando localmente no mesmo PC
                            "http://192.168.1.130:3000"   // Frontend acessando de outro dispositivo na rede local
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // necessário se você usar withCredentials: true
                });
            });

            var app = builder.Build();

            //  INSERE O MIDDLEWARE MULTITENANT AQUI, vai rodar antes de tudo
            app.UseMiddleware<ExtractTenantMiddleware>();

            // Middleware de cultura
            var supportedCultures = new[] { new CultureInfo("en-US") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseStaticFiles();
            app.UseCors("AllowFrontend");
            app.UseAuthentication(); // Enable authentication middleware
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection(); descomentar para funcionar o https.

            app.MapControllers();

            app.Run();
        }
    }
}
