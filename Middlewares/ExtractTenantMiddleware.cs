using MarmitaBackend.Provider;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class ExtractTenantMiddleware
{
    private readonly RequestDelegate _next;

    public ExtractTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantAccessor accessor)
    {
        // 1 - Tenta pegar do header
        var hasTenant = context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader);

        Console.WriteLine($"TENANT HEADER: {tenantHeader}");

        if (!hasTenant || string.IsNullOrWhiteSpace(tenantHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("TenantId header missing");
            return;
        }

        // 2 - Validar inteiro
        if (!int.TryParse(tenantHeader, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid TenantId format");
            return;
        }

        // 3 - Armazena no HttpContext
        context.Items["TenantId"] = tenantId;

        // 4 - Armazena no TenantAccessor (SCOPED)
        accessor.TenantId = tenantId;

        // 5 - Continua pipeline
        await _next(context);
    }
}


/*
Middleware lê o header
Middleware valida
 Middleware salva no HttpContext.Items
 Middleware salva no TenantAccessor (o que será usado pelo DbContext)
 TenantProvider (se você usar) pode ler do context
 ApplicationDbContext usa TenantAccessor para QueryFilters
 Controllers usam TenantProvider para inserir
 EF Core filtra automaticamente dados por Tenant
 Tudo alinhado em cada requisição
*/