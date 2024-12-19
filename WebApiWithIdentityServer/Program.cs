
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApiWithIdentityServer.Configurations;

namespace WebApiWithIdentityServer
{
    public class Program
    {
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

            builder.Services.AddIdentityServer()
                .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
                .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
                .AddInMemoryClients(IdentityServerConfig.Clients)
                .AddDeveloperSigningCredential();

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:7191"; // IdentityServer URL
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = false, // Validate
                    };
                });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();


            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });

                //// Add Security Definition for Bearer token
                //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    In = ParameterLocation.Header,
                //    Description = "Please enter token",
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.Http,
                //    BearerFormat = "JWT",
                //    Scheme = "bearer"
                //});

                // Add Security Definition for oauth2
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:7191/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:7191/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "api1", "Access to My API" }
                            }
                        }
                    }
                });

                //// Add Security Requirement for Bearer token
                //options.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer"
                //            }
                //        },
                //        Array.Empty<string>()
                //    }
                //});

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
                        new[] { "api1" }
                    }
                });
            });

            var app = builder.Build();

            app.UseRouting();
            app.UseIdentityServer();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");

                    // Configure OAuth2 login in Swagger UI
                    options.OAuthClientId("client"); // Client ID defined in IdentityServer
                    options.OAuthClientSecret("secret"); // Client Secret for the client
                    options.OAuthUsePkce(); // Enable Proof Key for Code Exchange (PKCE)
                    options.OAuthScopes("api1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
