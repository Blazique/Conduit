var builder = DistributedApplication.CreateBuilder(args);
var identityServer = builder.AddProject<Projects.Conduit_IdentityServer>(name: Conduit.Services.IdentityServer.Name);
var api = builder.AddProject<Projects.Conduit_API>(Conduit.Services.API.Name)
    .WithReference(identityServer);
builder.AddProject<Projects.Conduit_Frontend>(Conduit.Services.Frontend.Name)
    .WithReference(identityServer)
    .WithReference(api);

builder.Build().Run();
