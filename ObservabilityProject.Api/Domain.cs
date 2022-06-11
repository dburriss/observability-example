namespace ObservabilityProject.Api.Domains
{
    public class MarkInProgress
    {
        private readonly ITodoListStore todoLists;

        public MarkInProgress(ITodoListStore toDoLists)
        {
            this.todoLists = toDoLists;
        }

        public bool Execute(Guid todoListId, Guid todoId)
        {
            var (_, toDoList) = todoLists.Get(todoListId);
            var result = toDoList.MarkInProgress(todoId);
            todoLists.Save(toDoList);
            return result;
        }
    }

    public class MarkAsDone
    {
        private readonly ITodoListStore todoLists;

        public MarkAsDone(ITodoListStore toDoLists)
        {
            this.todoLists = toDoLists;
        }

        public bool Execute(Guid todoListId, Guid todoId)
        {
            var (_, toDoList) = todoLists.Get(todoListId);
            var result = toDoList.MarkDone(todoId);
            todoLists.Save(toDoList);
            return result;
        }
    }

    public class CreateTodoList
    {
        private readonly ITodoListStore todoLists;

        public CreateTodoList(ITodoListStore toDoLists)
        {
            this.todoLists = toDoLists;
        }

        public bool Execute(Guid todoListId, string name)
        {
            var toDoList = new TodoList(todoListId, name, new Todo[0]);
            todoLists.Save(toDoList);
            return true;
        }
    }

    public class CreateTodo
    {
        private readonly ITodoListStore todoLists;

        public CreateTodo(ITodoListStore toDoLists)
        {
            this.todoLists = toDoLists;
        }

        public bool Execute(Guid todoListId, Guid todoId, string text)
        {
            var (_, toDoList) = todoLists.Get(todoListId);
            var todo = new Todo(todoId, text);
            toDoList.Add(todo);
            todoLists.Save(toDoList);
            return true;
        }
    }

    public interface ITodoListStore
    {
        (bool, TodoList) Get(Guid toDoListId);
        IReadOnlyList<TodoList> GetLists();
        void Save(TodoList todoList);
    }

    public class TodoList
    {
        public Guid Id { get; }
        public string Name { get; }
        List<Todo> todos { get; }

        public TodoList(Guid id, string name, IEnumerable<Todo> todos)
        {
            Id = id;
            Name = name;
            this.todos = new List<Todo>(todos);
        }

        public IReadOnlyList<Todo> Todos => todos;

        public void Add(Todo todo)
        {
            todos.Add(todo);
        }

        public bool MarkInProgress(Guid toDoId)
        {
            var itemToChange = todos.FirstOrDefault(x => x.Id == toDoId);
            if (itemToChange != null)
            {
                itemToChange.InProgress();
                return true;
            }
            else return false;
        }

        public bool MarkDone(Guid toDoId)
        {
            var itemToChange = todos.FirstOrDefault(x => x.Id == toDoId);
            if (itemToChange != null)
            {
                itemToChange.Done();
                return true;
            }
            else return false;
        }
    }

    public class Todo
    {
        public Guid Id { get; private set; }
        public string Text { get; private set; }
        public Status Status { get; private set; }

        public Todo(Guid id, string text)
        {
            Id = id;
            Text = text;
        }

        public void InProgress()
        {
            Status = Status.InProgress;
        }

        public void Done()
        {
            Status = Status.Done;
        }
    }

    public enum Status
    {
        New = 0,
        InProgress = 1,
        Done = 2
    }
}
