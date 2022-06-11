﻿using ObservabilityProject.Api.Domains;

namespace ObservabilityProject.Api.DataAccess
{
    public class ToDoDataStore : ITodoListStore
    {
        static Dictionary<Guid, TodoList> dataStore = new Dictionary<Guid, TodoList>();

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
            if(dataStore.Count == 0)
            {
                return (new List<TodoList>());
            }
            return dataStore.Values.ToList();
        }
    }
}