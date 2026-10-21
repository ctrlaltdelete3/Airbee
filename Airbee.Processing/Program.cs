using Airbee.Data.Context;
using Airbee.Shared.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

            // Scoped, because it uses the scoped DbContext
            builder.Services.AddScoped<MeasurementSaver>();

            builder.Services.AddOptions<RabbitMqOptions>()
                .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
                .ValidateOnStart();
            builder.Services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
