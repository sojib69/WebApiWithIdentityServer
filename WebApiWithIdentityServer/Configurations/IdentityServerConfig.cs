using Duende.IdentityServer.Models;

namespace WebApiWithIdentityServer.Configurations
{
    public class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

        public static IEnumerable<ApiScope> ApiScopes =>
            [
                new ApiScope("api1", "My API")
            ];

        public static IEnumerable<ApiResource> ApiResources =>
            [
                new ApiResource("api1", "My API")
                {
                    Scopes = { "api1" }
                }
            ];

        public static IEnumerable<Client> Clients =>
            [
                new Client() {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:5001/swagger/oauth2-redirect.html" }, // Redirect URI for Swagger
                    AllowedScopes = { "api1" },
                    RequirePkce = true,
                }
            ];
    }
}
