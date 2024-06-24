using Conduit;

var builder = DistributedApplication.CreateBuilder(args);

var identityServerAuthorityEnvironmentVariableName = IdentityServerSettingsConfigurationKeys.IdentityServerSettings_Authority.Replace(':', '_');

var postgres = builder.AddPostgres(Postgres.Name);
var conduitDatabase = postgres.AddDatabase(ConduitDatabase.Name);
var identityDatabase = postgres.AddDatabase(IdentityDatabase.Name);

var identityServer = builder
    .AddProject<Projects.Conduit_IdentityServer>(name: IdentityServer.Name)
    .WithReference(identityDatabase);
var identityEndpoint = identityServer.GetEndpoint("https");
var api = builder.AddProject<Projects.Conduit_API>(Backend.Name)
    .WithEnvironment(identityServerAuthorityEnvironmentVariableName, identityEndpoint)
    .WithReference(conduitDatabase);
builder.AddProject<Projects.Conduit_Frontend>(Frontend.Name)
    .WithReference(api)
    .WithEnvironment(identityServerAuthorityEnvironmentVariableName, identityEndpoint);

builder.Build().Run();
