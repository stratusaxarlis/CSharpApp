using Projects;
using Scalar.Aspire;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        // Create the distributed application builder from the Aspire hosting SDK
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
        // Optionally add projects/resources here, for example:
        IResourceBuilder<ProjectResource> apiService = builder.AddProject<CSharpApp_Api>("apiService");
        IResourceBuilder<ScalarResource> scalar = builder.AddScalarApiReference();
        scalar
            .WithApiReference(apiService);
        await builder.Build().RunAsync();
    }
}
