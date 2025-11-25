using MarmitaBackend.Provider;

namespace MarmitaBackend.Provider
{
    public class TenantProvider : ITenantProvider
    {

        private readonly IHttpContextAccessor _http; //permite acessar HttpContext (cabecalhos, cookies, reqs, resps, contexto) 

        public TenantProvider(IHttpContextAccessor http)
        {
            _http = http; //DI Para criar um TenantProvider, se deve injetar um objeto que implementa IHttpContextAccessor.
        }

        public int TenantId =>
    int.TryParse(_http.HttpContext?.Items["TenantId"]?.ToString(), out var id) //verifica no contexto se existe o item TenantId e transforma em int
        ? id
        : throw new Exception("TenantId not available");

    }
}


//Essa classe será a responsável por dizer de onde vem o tenant atual.
//IHttpContextAccessor permite acessar o HttpContext de qualquer lugar
//Para encontrar o Tenant atual, preciso acessar o HttpContext da requisição.


