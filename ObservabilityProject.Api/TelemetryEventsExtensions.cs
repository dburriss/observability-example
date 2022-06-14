using ObservabilityProject.Api.DataAccess;
using StatsdClient;
using static ObservabilityProject.Api.MetricName;
using static ObservabilityProject.Api.Tag;

namespace ObservabilityProject.Api
{
    public static class TelemetryEventsExtensions
    {
        public static void NoListsReturned(this TelemetryEvents<ToDoDataStore> telemetryEvents)
        {
            telemetryEvents.Logger.LogInformation("TODO lists requested but there are none.");
            DogStatsd.Set(TodoListCount, 0, tags: Tags(Failure));
        }

        public static void ListsAvailable(this TelemetryEvents<ToDoDataStore> telemetryEvents, int count)
        {
            DogStatsd.Set(TodoListCount, count, tags: Tags(Success));
        }
    }
}
