using Conduit;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres(ConduitDatabase.Name).WithPgAdmin();

var identityServer = builder
    .AddProject<Projects.Conduit_IdentityServer>(name: IdentityServer.Name)
    .WithReference(postgres);
var identityEndpoint = identityServer.GetEndpoint("https");

var backend = builder.AddProject<Projects.Conduit_API>(Backend.Name)
    .WithEnvironment(IdentityServerSettingsConfigurationKeys.IdentityServerSettings_Authority, identityEndpoint)
    .WithReference(postgres);

var frontend = builder.AddProject<Projects.Conduit_Frontend>(Frontend.Name)
    .WithReference(backend)
    .WithEnvironment(IdentityServerSettingsConfigurationKeys.IdentityServerSettings_Authority, identityEndpoint);

builder.Build().Run();
