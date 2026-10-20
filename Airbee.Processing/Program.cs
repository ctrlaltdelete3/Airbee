using Airbee.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Airbee.Processing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("Airbee")
                ?? throw new InvalidOperationException("Connection string 'Airbee' is missing.");

            builder.Services.AddDbContext<AirbeeDbContext>(options => options.UseNpgsql(connectionString));

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
