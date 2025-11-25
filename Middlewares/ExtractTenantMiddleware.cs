using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ExtractTenantMiddleware
{
    private readonly RequestDelegate _next;


    public ExtractTenantMiddleware(RequestDelegate next)
    {
        _next = next; //para pular pro proximo middleware
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1 - Tenta pegar o TenantId do Header: X-Tenant-Id
        var hasTenant = context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader);

        if (!hasTenant || string.IsNullOrWhiteSpace(tenantHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("TenantId header missing");
            return;
        }

        // 2️ - Valida se é um número inteiro
        if (!int.TryParse(tenantHeader, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid TenantId format");
            return;
        }

        // 3️ - Armazena no contexto da requisição
        context.Items["TenantId"] = tenantId;

        // 4️ - Continua para o próximo middleware
        await _next(context);
    }
}
