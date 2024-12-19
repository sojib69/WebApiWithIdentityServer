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
                new ApiScope("scope2", "My API")
            ];

        public static IEnumerable<ApiResource> ApiResources =>
            [
                new ApiResource("scope2", "My API")
                {
                    Scopes = { "scope2" }
                }
            ];

        public static IEnumerable<Client> Clients =>
            [
                new Client() {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:7191/swagger/oauth2-redirect.html" }, // Redirect URI for Swagger
                    AllowedScopes = { "scope2" },
                    RequirePkce = true,
                }
            ];
    }
}
