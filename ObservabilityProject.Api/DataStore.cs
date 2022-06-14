using Datadog.Trace;
using ObservabilityProject.Api.Domains;
using StatsdClient;
using System.Diagnostics;

namespace ObservabilityProject.Api.DataAccess
{
    public class ToDoDataStore : ITodoListStore
    {
        static Dictionary<Guid, TodoList> dataStore = new Dictionary<Guid, TodoList>();
        private readonly ILogger<ToDoDataStore> logger;

        public ToDoDataStore(ILogger<ToDoDataStore> logger)
        {
            this.logger = logger;
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
            DogStatsd.Increment("observability_project.todo.list_created");
            dataStore[todoList.Id] = todoList;
        }

        public IReadOnlyList<TodoList> GetLists()
        {
            logger.LogInformation("Datadog traceid {traceid} and spanid {spanid}.", Tracer.Instance.ActiveScope.Span.TraceId, Tracer.Instance.ActiveScope.Span.SpanId);
            logger.LogInformation("Dotnet traceid {traceid} and spanid {spanid}.", Activity.Current?.TraceId, Activity.Current?.SpanId);

            using (var scope = Tracer.Instance.StartActive("query.todo_lists"))
            {
                scope.Span.ResourceName = $"{nameof(ToDoDataStore)}.{nameof(GetLists)}";
                logger.LogInformation("Datadog child traceid {traceid} and spanid {spanid}.", Tracer.Instance.ActiveScope.Span.TraceId, Tracer.Instance.ActiveScope.Span.SpanId);
                logger.LogInformation("Dotnet child traceid {traceid} and spanid {spanid}.", Activity.Current?.TraceId, Activity.Current?.SpanId);
                DogStatsd.Set("observability_project.todo.list_count", dataStore.Count);

                if (dataStore.Count == 0)
                {
                    logger.LogInformation("TODO lists requested but none.");
                    return (new List<TodoList>());
                }
                return dataStore.Values.ToList();
            }
        }
    }
}
