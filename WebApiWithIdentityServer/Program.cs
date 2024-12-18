
using WebApiWithIdentityServer.Configurations;

namespace WebApiWithIdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddIdentityServer()
                .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
                .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
                .AddInMemoryClients(IdentityServerConfig.Clients)
                .AddDeveloperSigningCredential();

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:7191"; // IdentityServer URL
                    options.RequireHttpsMetadata = false;
                    options.Audience = "api1";
                });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseIdentityServer();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
