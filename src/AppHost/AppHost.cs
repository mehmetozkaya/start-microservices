var builder = DistributedApplication.CreateBuilder(args);


// Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithPgAdmin(pgAdmin => pgAdmin.WithUrlForEndpoint("http", url => url.DisplayText = "PostgreDB Browser"))
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalogdb = postgres.AddDatabase("catalogdb");

var cache = builder
        .AddRedis("cache")
        .WithRedisInsight()
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);


// Projects
var catalog = builder
        .AddProject<Projects.Catalog>("catalog")
        .WithReference(catalogdb)
        .WaitFor(catalogdb);

var basket = builder
        .AddProject<Projects.Basket>("basket")
        .WithReference(cache)
        .WaitFor(cache);






builder.Build().Run();
