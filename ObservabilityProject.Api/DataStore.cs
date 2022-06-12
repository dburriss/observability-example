using Datadog.Trace;
using ObservabilityProject.Api.Domains;
using StatsdClient;

namespace ObservabilityProject.Api.DataAccess
{
    public class ToDoDataStore : ITodoListStore
    {
        static Dictionary<Guid, TodoList> dataStore = new Dictionary<Guid, TodoList>();
        private readonly ILogger<ToDoDataStore> logger;
        private readonly Func<DogStatsdService> statsDFactory;

        public ToDoDataStore(ILogger<ToDoDataStore> logger, Func<DogStatsdService> statsDFactory)
        {
            this.logger = logger;
            this.statsDFactory = statsDFactory;
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
        }

        public IReadOnlyList<TodoList> GetLists()
        {
            using (var scope = Tracer.Instance.StartActive("query.todo_lists"))
            {
                scope.Span.ResourceName = "ToDoDataStore";

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
