using Conduit;

var builder = DistributedApplication.CreateBuilder(args);
var identityServer = builder
    .AddProject<Projects.Conduit_IdentityServer>(name: IdentityServer.Name);
var identityEndpoint = identityServer.GetEndpoint("https");
var api = builder.AddProject<Projects.Conduit_API>(Backend.Name)
    .WithEnvironment("Identity__Url", identityEndpoint);
builder.AddProject<Projects.Conduit_Frontend>(Frontend.Name)
    .WithReference(api)
    .WithEnvironment("Identity__Url", identityEndpoint);

builder.Build().Run();
