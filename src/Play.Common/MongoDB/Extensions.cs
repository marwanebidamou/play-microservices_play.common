using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Play.Common;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace Play.Common.MongoDB
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            //To Store guid type in mongo db as string
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

            //To Store DateTimeOffset type in mongo db as string
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));


            //register mongo db service
            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                //service settings
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

                //mongo db settings
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                //prepare mongo client
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                //get mongo database with the same name as the current service
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;

        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
        {
            //we use service provider because we need to specify a parameter as an input to the repository
            //(in MongoRepository we're receiving our collection name, so we cannot just expect to all parameters to be injected automatically by the service container for us)
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                //before we can create an instance of the MongoRepository first, we need an instance of the IMongoDatabase
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database, collectionName);
            });

            return services;
        }
    }
}
