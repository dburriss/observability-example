using ObservabilityProject.Api.DataAccess;
using ObservabilityProject.Api.Domains;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Datadog.Logs;
using StatsdClient;
using System.Net;

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

//// DogStatsD

//var dogstatsdConfig = new StatsdConfig
//{
//    //StatsdServerName = "127.0.0.1",
//    //StatsdPort = 8125,
//};


//builder.Services.AddTransient<Func<DogStatsdService>>((_) => { return () => new DogStatsdService(); });

// Log to Datadog
builder.Host.UseSerilog((ctx, cfg) => {
    cfg
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.DatadogLogs(
        Environment.GetEnvironmentVariable("DD_API_KEY"),
        service: Environment.GetEnvironmentVariable("DD_SERVICE"),
        host: Environment.UserDomainName ?? "unknown",
        configuration: new DatadogConfiguration(url: "tcp-intake.logs.datadoghq.eu", port: 443, useSSL: true, useTCP: true)
    );
});

var app = builder.Build();

try
{
    // Configure your DogStatsd client and configure any tags
    DogStatsd.Configure(new StatsdConfig());
}
catch (Exception ex)
{
    // An exception is thrown by the Configure call if the necessary environment variables are not present.
    // These environment variables are present in Azure App Service, but
    // need to be set in order to test your custom metrics: DD_API_KEY:{api_key}, DD_AGENT_HOST:localhost
    // Ignore or log the exception as it suits you
    Console.WriteLine(ex);
    Log.Logger.Error(ex, "DD metrics failed");
}

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