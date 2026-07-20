namespace CSharpApp.Core.Settings;

public sealed record HttpClientSettings
{
    public int LifeTime { get; set; }
    public int RetryCount { get; set; }
    public int SleepDuration { get; set; }
}