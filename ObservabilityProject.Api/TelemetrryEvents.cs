namespace ObservabilityProject.Api
{
    public class TelemetryEvents<T>
    {
        public ILogger<T> Logger { get; }
        public TelemetryEvents(ILogger<T> logger)
        {
            Logger = logger;
        }
    }
}
