namespace PlusUltra.WebApi.JWT
{
    public class JwtSettings
    {
        public JwtOIDCSettings oidc { get; set; }
    }

    public class JwtOIDCSettings
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string  Issuer { get; set; }
    }
}