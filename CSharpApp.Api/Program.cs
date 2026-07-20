using CSharpApp.Api.Extensions;
using CSharpApp.Core.Settings;


public sealed class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine(
                """

			╔═╗╔═╗╦  ╔═╗┌─┐┬─┐┬  ┬┬┌─┐┌─┐
			╠═╣╠═╝║  ╚═╗├┤ ├┬┘└┐┌┘││  ├┤ 
			╩ ╩╩  ╩  ╚═╝└─┘┴└─ └┘ ┴└─┘└─┘

			""");

        Console.WriteLine($"Starting up... {WebApplicationExtensions.AspNetCoreEnvironment}");

        WebApplicationOptions webApplicationOptions = new()
        {
            Args = args,
            EnvironmentName = WebApplicationExtensions.AspNetCoreEnvironment
        };
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(webApplicationOptions).Configure();

            WebApplication app = builder.Build();
            app.LogSystemInformation();

            app.ConfigureServices();

            // Start the built WebApplication, effectively starting the server
            await app.RunAsync().ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc.Message);
            Log.Fatal(exc.Message);
            if (exc.InnerException is not null)
                Log.Fatal(exc.InnerException, "{AssemblyName} Startup Failed. WebApplicationOptions: {@WebApplicationOptions}", typeof(Program).Assembly.GetName().Name, webApplicationOptions);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}



