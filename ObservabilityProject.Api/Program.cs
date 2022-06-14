using ObservabilityProject.Api;
using ObservabilityProject.Api.Domains;
using ObservabilityProject.Api.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register the datastore
builder.Services.AddSingleton<ITodoListStore, ToDoDataStore>();
// Register usecases
builder.Services.AddTransient<MarkInProgress>();
builder.Services.AddTransient<MarkAsDone>();
builder.Services.AddTransient<CreateTodoList>();
builder.Services.AddTransient<CreateTodo>();
builder.Services.AddTransient(typeof(TelemetryEvents<>));

builder.AddTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//======================
// ENDPOINTS
//======================

// Index
app.MapGet("/", () =>
{
    return Results.Ok("Hello TODO");
})
.WithName("Index");

// Create a new todo list
app.MapPost("/todos/{todoListId}", (Guid todoListId, string name, CreateTodoList createTodoList) =>
{
    var result = createTodoList.Execute(todoListId, name);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("Create TODO Lists");

// Create a new todo item in a list
app.MapPost("/todos/{todoListId}/new/{todoId}", (Guid todoListId, Guid todoId, string name, CreateTodo createTodo) =>
{
    var result = createTodo.Execute(todoListId, todoId, name);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("Create TODO item");

// Mark a todo item as in progress
app.MapPost("/todos/{todoListId}/in-progress", (Guid todoListId, Guid todoId, MarkInProgress markInProgress) =>
{
    var result = markInProgress.Execute(todoListId, todoId);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("Mark In Progress");

// Mark a todo item as done
app.MapPost("/todos/{todoListId}/done", (Guid todoListId, Guid todoId, MarkAsDone markAsDone) =>
{
    var result = markAsDone.Execute(todoListId, todoId);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("Mark Done");

// List all todo lists
app.MapGet("/todos", (ITodoListStore todos) =>
{
        var result = todos.GetLists();
        return Results.Ok(result.Select(x => new { Id = x.Id, Name = x.Name }));          
})
.WithName("GET TODO Lists");

// Return a todo list
app.MapGet("/todos/{todoListId}", (Guid todoListId, ITodoListStore todos) =>
{
    var (result, todoList) = todos.Get(todoListId);
    return Results.Ok(todoList);
})
.WithName("GET TODO List");

// RUN APP
app.Run();


internal static class ObservabilityProjectActivitySource
{
    private static readonly System.Reflection.AssemblyName AssemblyName = typeof(ObservabilityProjectActivitySource).Assembly.GetName();
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    internal static readonly System.Diagnostics.ActivitySource ActivitySource = new(AssemblyName.Name, AssemblyName.Version.ToString());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
}