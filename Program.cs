using back.Models;
using back.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WorkflowStore>();

var app = builder.Build();

app.MapPost("/workflows", (WorkflowDefinition definition, WorkflowStore store) =>
{
    if (definition.States.Count(s => s.IsInitial) != 1)
    {
        return Results.BadRequest("A workflow definition must have exactly one initial state.");
    }
    if (store.TryAddDefinition(definition))
    {
        return Results.Created($"/workflows/{definition.Id}", definition);
    }
    return Results.Conflict($"A workflow with ID '{definition.Id}' already exists.");
});

app.MapGet("/workflows/{id}", (string id, WorkflowStore store) =>
{
    var definition= store.GetDefinition(id);
    return definition is not null? Results.Ok(definition) : Results.NotFound();
});

app.MapPost("/workflows/{definitionId}/instances", (string definitionId, WorkflowStore store) =>
{
    var definition = store.GetDefinition(definitionId);
    if (definition is null)
    {
        return Results.NotFound($"Workflow definition with ID '{definitionId}' not found.");
    }
    var initialState = definition.States.First(s => s.IsInitial);
    var instance = new WorkflowInstance(
        Id: Guid.NewGuid(),
        DefinitionId: definitionId,
        CurrentStateId: initialState.Id,
        History: []
    );
    store.SaveInstance(instance);
    return Results.Created($"/instances/{instance.Id}", instance);
});

app.MapGet("/instances/{instanceId}", (Guid instanceId, WorkflowStore store) =>
{
    var instance = store.GetInstance(instanceId);
    return instance is not null ? Results.Ok(instance) : Results.NotFound();
});

app.MapPost("/instances/{instanceId}/execute", (Guid instanceId, ExecuteActionRequest request, WorkflowStore store) =>
{
    var instance = store.GetInstance(instanceId);
    if (instance is null)
    {
        return Results.NotFound($"Instance with ID '{instanceId}' not found.");
    }
    var definition = store.GetDefinition(instance.DefinitionId);
    var currentState = definition!.States.First(s => s.Id == instance.CurrentStateId);
    if (currentState.IsFinal)
    {
        return Results.BadRequest("Cannot execute action on an instance in a final state.");
    }

    var actionToExecute = definition.Actions.FirstOrDefault(a => a.Id == request.ActionId);
    if (actionToExecute is null)
    {
        return Results.BadRequest($"Action '{request.ActionId}' not found in workflow definition.");
    }

    if (!actionToExecute.Enabled)
    {
        return Results.BadRequest($"Action '{request.ActionId}' is disabled.");
    }

    if (!actionToExecute.FromStates.Contains(currentState.Id))
    {
        return Results.BadRequest($"Action '{request.ActionId}' cannot be executed from the current state '{currentState.Id}'.");
    }

    var updatedInstance = instance with
    {
        CurrentStateId = actionToExecute.ToState,
        History = instance.History.Append(new HistoryEntry(request.ActionId, DateTime.UtcNow)).ToList()
    };
    store.SaveInstance(updatedInstance);
    return Results.Ok(updatedInstance);
});

app.Run();




