using back.Models;
using System.Collections.Concurrent;

namespace back.Storage
{
    public class WorkflowStore
    {
        //The two dictionaries which store all our data
        private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
        private readonly ConcurrentDictionary<Guid, WorkflowInstance> _instances= new();

        //TryAddDefinition adds a new workflow(template) to the database
        public bool TryAddDefinition(WorkflowDefinition definition)
        {
            return _definitions.TryAdd(definition.Id, definition);
        }

        //Checks if a particular Definition exists in the database and retrieves it
        public WorkflowDefinition? GetDefinition(string id)
        {
            _definitions.TryGetValue(id, out var definition);
            return definition;
        }

        //saves a particular instance of a workflow into the database
        public void SaveInstance(WorkflowInstance instance)
        {
            _instances[instance.Id] = instance;
        }

        //Checks if a particular instance exists in the database and retrieves it
        public WorkflowInstance? GetInstance(Guid id)
        {
            _instances.TryGetValue(id, out var instance);
            return instance;
        }
    }
}