using Datadog.Trace;
using ObservabilityProject.Api.Domains;
using StatsdClient;
using System.Diagnostics;
using static ObservabilityProject.Api.MetricName;

namespace ObservabilityProject.Api.DataAccess
{
    public class ToDoDataStore : ITodoListStore
    {
        static Dictionary<Guid, TodoList> dataStore = new Dictionary<Guid, TodoList>();
        private readonly TelemetryEvents<ToDoDataStore> telemetryEvents;

        public ToDoDataStore(TelemetryEvents<ToDoDataStore> telemetryEvents)
        {
            this.telemetryEvents = telemetryEvents;
        }

        public (bool, TodoList) Get(Guid toDoListId)
        {
            var _ = dataStore.TryGetValue(toDoListId, out var todoList);
            return todoList switch
            {
                null => (false, new TodoList(toDoListId, "Default", new List<Todo>())),
                _ => (true, todoList)
            };
        }

        public void Save(TodoList todoList)
        {
            dataStore[todoList.Id] = todoList;
            DogStatsd.Increment(TodoListCreated);
        }

        public IReadOnlyList<TodoList> GetLists()
        {
            //logger.LogInformation("Datadog traceid {traceid} and spanid {spanid}.", Tracer.Instance.ActiveScope.Span.TraceId, Tracer.Instance.ActiveScope.Span.SpanId);
            //logger.LogInformation("Dotnet traceid {traceid} and spanid {spanid}.", Activity.Current?.TraceId, Activity.Current?.SpanId);

            using (var scope = Tracer.Instance.StartActive("query.todo_lists"))
            {
                scope.Span.ResourceName = $"{nameof(ToDoDataStore)}.{nameof(GetLists)}";
                //logger.LogInformation("Datadog child traceid {traceid} and spanid {spanid}.", Tracer.Instance.ActiveScope.Span.TraceId, Tracer.Instance.ActiveScope.Span.SpanId);
                //logger.LogInformation("Dotnet child traceid {traceid} and spanid {spanid}.", Activity.Current?.TraceId, Activity.Current?.SpanId);

                if (dataStore.Count == 0)
                {
                    telemetryEvents.NoListsReturned();
                    return (new List<TodoList>());
                }
                telemetryEvents.ListsAvailable(dataStore.Count);
                return dataStore.Values.ToList();
            }
        }
    }
}
