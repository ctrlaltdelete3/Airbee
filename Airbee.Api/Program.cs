using Airbee.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Airbee.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("Airbee")
                ?? throw new InvalidOperationException("Connection string 'Airbee' is missing.");

            builder.Services.AddDbContext<AirbeeDbContext>(options => options.UseNpgsql(connectionString));

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
