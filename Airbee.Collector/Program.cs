using Airbee.Collector.RabbitMq;
using Airbee.Collector.SensorCommunity;
using Airbee.Shared.RabbitMq;
using Microsoft.Extensions.Options;

namespace Airbee.Collector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddOptions<SensorCommunityOptions>()
                .Bind(builder.Configuration.GetSection(SensorCommunityOptions.SectionName))
                .PostConfigure(options =>
                {
                    // Without a trailing slash HttpClient drops the last path segment ("v1").
                    if (options.BaseUrl is not null && options.BaseUrl.AbsoluteUri.EndsWith('/') == false)
                    {
                        options.BaseUrl = new Uri(options.BaseUrl.AbsoluteUri + "/");
                    }
                })
                .ValidateOnStart();
            builder.Services.AddSingleton<IValidateOptions<SensorCommunityOptions>, SensorCommunityOptionsValidator>();

            builder.Services.AddSingleton<SensorCommunityMapper>();

            builder.Services.AddOptions<RabbitMqOptions>()
                .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
                .ValidateOnStart();
            builder.Services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();

            builder.Services.AddSingleton<MeasurementPublisher>();

            builder.Services.AddHttpClient<SensorCommunityClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<SensorCommunityOptions>>().Value;
                httpClient.BaseAddress = options.BaseUrl;
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            })
            // The singleton Worker keeps this client forever, so the factory never rotates its handler.
            // Recycle connections here instead, so DNS changes are still picked up.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
