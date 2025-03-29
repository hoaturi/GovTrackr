using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Database password is auto generated
// To check the password go to dashboard -> database container -> environment variables
// or add user secrets to the project.
// dotnet user-secrets set Parameters:govtrackr-password <password>
var postgres = builder.AddPostgres("govtrackr", port: 5432).WithDataVolume();

// It doesn't create the actual database.
// You need to create the database manually.
var database = postgres.AddDatabase("govtrackr-db");

builder.AddProject<GovTrackr_Api>("govtrackr-api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();