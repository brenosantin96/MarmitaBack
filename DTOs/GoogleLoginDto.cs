namespace MarmitaBackend.DTOs
{
    public class GoogleLoginDto
    {
        public string Code { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }

    }
}
