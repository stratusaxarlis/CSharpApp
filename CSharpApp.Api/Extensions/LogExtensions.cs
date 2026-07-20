using System.Runtime.InteropServices;
using CSharpApp.Core.Settings;
namespace CSharpApp.Api.Extensions;

public static class LogExtensions
{
    /// <summary>
    ///     Logs detailed system and application-level information, including runtime details, architecture,
    ///     environment settings, process information, and configuration values, for diagnostic purposes.
    /// </summary>
    public static void LogSystemInformation(this WebApplication app)
    {
        // The first argument is the name of the executable, not needed.
        string[] cmdLineArgs = Environment.GetCommandLineArgs();
        string commandLine = string.Join(' ', cmdLineArgs.Length > 1 ? cmdLineArgs.Skip(1) : cmdLineArgs);
     
      
        Log.Information("Operating System: {OsDescription}", RuntimeInformation.OSDescription);
        Log.Information("Operating System Architecture: {OsArchitecture}", RuntimeInformation.OSArchitecture);
        Log.Information("ASP.NET Core Environment: {Environment}", WebApplicationExtensions.AspNetCoreEnvironment);
        //Log.Information("Runtime Identifier: {RuntimeIdentifier}", RuntimeInformation.RuntimeIdentifier);
        Log.Information("Framework Description: {FrameworkDescription}", RuntimeInformation.FrameworkDescription);
        Log.Information("Process Id: {ProcessId}", Environment.ProcessId);
        Log.Information("Process Launch CommandLine: {CommandLine}", Environment.CommandLine);
        Log.Information("Process Architecture: {ProcessArchitecture}", RuntimeInformation.ProcessArchitecture);
        Log.Information("CommandLine arguments: {Arguments}", string.IsNullOrWhiteSpace(commandLine) ? "N/A" : commandLine);
      
       
    }
}
