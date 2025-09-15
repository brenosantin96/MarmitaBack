using System.Security.Claims;

namespace MarmitaBackend.Utils
{
    public static class UserHelper
    {
        public static int? GetUserId(ClaimsPrincipal user)
        {
            if (user?.Identity is { IsAuthenticated: true })
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }

            return null; // Não autenticado ou ID inválido
        }
    }
}
//