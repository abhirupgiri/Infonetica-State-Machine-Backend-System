using back.Models;
using back.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WorkflowStore>();

var app = builder.Build();

//This API Creates a workflow if it does not already exist
app.MapPost("/workflows", (WorkflowDefinition definition, WorkflowStore store) =>
{
    if (definition.States.Count(s => s.IsInitial) != 1)
    {
        return Results.BadRequest("ERROR!!!!!! A workflow definition must have exactly one initial state.");
    }
    if (store.TryAddDefinition(definition))
    {
        return Results.Created($"/workflows/{definition.Id}", definition);
    }
    return Results.Conflict($"ERROR!!!!!! The given workflow with ID '{definition.Id}' already exists and hence cannot me made again.");
});

//This API retrieves a particular workflow given an id
app.MapGet("/workflows/{id}", (string id, WorkflowStore store) =>
{
    var definition = store.GetDefinition(id);
    return definition is not null ? Results.Ok(definition) : Results.NotFound();
});

//This API creates an Instance given a definitionId i.e. its workflow is given
app.MapPost("/workflows/{definitionId}/instances", (string definitionId, WorkflowStore store) =>
{
    var definition = store.GetDefinition(definitionId);
    if (definition is null)
    {
        return Results.NotFound($"ERROR!!!!! Workflow definition with given ID '{definitionId}' not found. Please create the workflow first!");
    }
    var initialState =definition.States.First(s =>s.IsInitial);
    var instance =new WorkflowInstance(
        Id: Guid.NewGuid(),
        DefinitionId: definitionId,
        CurrentStateId: initialState.Id,
        History: []
    );
    store.SaveInstance(instance);
    return Results.Created($"/instances/{instance.Id}", instance);
});

//This API retrieves an instance given its instanceID
app.MapGet("/instances/{instanceId}", (Guid instanceId, WorkflowStore store) =>
{
    var instance = store.GetInstance(instanceId);
    return instance is not null ? Results.Ok(instance) : Results.NotFound();
});

//This API is the main API which executes a particular action if it is avialable
app.MapPost("/instances/{instanceId}/execute", (Guid instanceId, ExecuteActionRequest request, WorkflowStore store) =>
{
    var instance =store.GetInstance(instanceId);
    if(instance is null)
    {
        return Results.NotFound($"ERROR!!!!! Instance with ID '{instanceId}' not found. Please create such an instance first.");
    }
    var definition= store.GetDefinition(instance.DefinitionId);
    var currentState= definition!.States.First(s => s.Id == instance.CurrentStateId);
    if (currentState.IsFinal)
    {
        return Results.BadRequest("ERROR!!!!! Cannot execute action on an instance in a final state. There can be no more transitions!");
    }

    var actionToExecute= definition.Actions.FirstOrDefault(a => a.Id == request.ActionId);
    if (actionToExecute is null)
    {
        return Results.BadRequest($"ERROR!!!!! Action '{request.ActionId}' not found in workflow definition. Create such an action first.");
    }

    if (!actionToExecute.Enabled)
    {
        return Results.BadRequest($"ERROR!!!!! Action '{request.ActionId}' is disabled.");
    }

    if (!actionToExecute.FromStates.Contains(currentState.Id))
    {
        return Results.BadRequest($"ERROR!!!!! Action '{request.ActionId}' cannot be executed from the current state '{currentState.Id}'.");
    }

    var updatedInstance=instance with
    {
        CurrentStateId=actionToExecute.ToState,
        History =instance.History.Append(new HistoryEntry(request.ActionId, DateTime.UtcNow)).ToList()
    };
    store.SaveInstance(updatedInstance);
    return Results.Ok(updatedInstance);
});

app.Run();




