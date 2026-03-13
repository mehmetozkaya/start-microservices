var builder = DistributedApplication.CreateBuilder(args);


// Backing Services
var postgres = builder
        .AddPostgres("postgres")
        .WithPgAdmin(pgAdmin => pgAdmin.WithUrlForEndpoint("http", url => url.DisplayText = "PostgreDB Browser"))
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);

var catalogdb = postgres.AddDatabase("catalogdb");


// Projects
var catalog = builder
        .AddProject<Projects.Catalog>("catalog")
        .WithReference(catalogdb)
        .WaitFor(catalogdb);




builder.Build().Run();
