using back.Models;
using System.Collections.Concurrent;

namespace back.Storage
{
    public class WorkflowStore
    {
        private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions= new();
        private readonly ConcurrentDictionary<Guid, WorkflowInstance> _instances= new();
        public bool TryAddDefinition(WorkflowDefinition definition)
        {
            return _definitions.TryAdd(definition.Id, definition);
        }
        public WorkflowDefinition? GetDefinition(string id)
        {
            _definitions.TryGetValue(id, out var definition);
            return definition;
        }
        public void SaveInstance(WorkflowInstance instance)
        {
            _instances[instance.Id] = instance;
        }
        public WorkflowInstance? GetInstance(Guid id)
        {
            _instances.TryGetValue(id, out var instance);
            return instance;
        }
    }
}