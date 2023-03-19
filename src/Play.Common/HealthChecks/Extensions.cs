using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Play.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Play.Common.HealthChecks
{
    public static class Extensions
    {
        private const string MongoCheckName = "MongoDB";
        private const string ReadyTagName = "ready";
        private const string LiveTagName = "live";
        private const string HealthEndpoint = "health";
        private const int DefaultSeconds = 3;

        public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, TimeSpan? timeout = default)
        {

            return builder.Add(new HealthCheckRegistration(
                name: MongoCheckName,
                factory: serviceProvider =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var mongoDbSettings = configuration!.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                    var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                    return new MongoDbHealthCheck(mongoClient);
                },
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { ReadyTagName },
                timeout: TimeSpan.FromSeconds(DefaultSeconds)
            ));
        }

        public static void MapPlayEconomyHealtChecks(this WebApplication app)
        {
            app.MapHealthChecks($"/{HealthEndpoint}/{ReadyTagName}", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains(ReadyTagName)
            });
            app.MapHealthChecks($"/{HealthEndpoint}/{LiveTagName}", new HealthCheckOptions
            {
                Predicate = (check) => false
            });
        }

    }
}
