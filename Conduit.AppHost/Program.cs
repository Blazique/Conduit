using Conduit;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(ConduitDatabase.Name);

var identityServer = builder
    .AddProject<Projects.Conduit_IdentityServer>(name: IdentityServer.Name)
    .WithReference(postgres);
var identityEndpoint = identityServer.GetEndpoint("https");
var api = builder.AddProject<Projects.Conduit_API>(Backend.Name)
    .WithEnvironment(IdentityServerSettingsConfigurationKeys.IdentityServerSettings_Authority, identityEndpoint)
    .WithReference(postgres);
builder.AddProject<Projects.Conduit_Frontend>(Frontend.Name)
    .WithReference(api)
    .WithEnvironment(IdentityServerSettingsConfigurationKeys.IdentityServerSettings_Authority, identityEndpoint);

builder.Build().Run();
