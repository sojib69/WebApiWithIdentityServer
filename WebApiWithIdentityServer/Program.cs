using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using WebApiWithIdentityServer.Configurations;

namespace WebApiWithIdentityServer
{
    public class Program
    {
        private static readonly string[] scopes = ["scope2"];

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddCors(policies =>
            {
                policies.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });

            // Add services to the DI container
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // For API Bearer token
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme; // For interactive login
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = "https://localhost:5001"; // IdentityServer URL
                options.Audience = "https://localhost:5001/resources"; // API Resource name
                options.RequireHttpsMetadata = false; // Set to true in production
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme); // Use cookies for browser-based logins

            // Add IdentityServer
            builder.Services.AddIdentityServer()
                .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
                .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
                .AddInMemoryClients(IdentityServerConfig.Clients)
                .AddDeveloperSigningCredential();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });

                // Add Security Definition for oauth2
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:5001/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "scope2", "Access to My API" }
                            }
                        }
                    }
                });

                // Add Security Requirement for oauth2
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        scopes }
                });
            });

            var app = builder.Build();

            app.UseRouting();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");

                    // Configure OAuth2 login in Swagger UI
                    options.OAuthClientId("interactive"); // Client ID defined in IdentityServer
                    options.OAuthClientSecret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0"); // Client Secret for the client
                    options.OAuthUsePkce(); // Enable Proof Key for Code Exchange (PKCE)
                    options.OAuthScopes("scope2");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseIdentityServer();

            app.MapControllers();

            app.Run();
        }
    }
}
