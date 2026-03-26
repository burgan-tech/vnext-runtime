using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Initial transition mapping - Initializes the collection/object test workflow
/// </summary>
public class InitCollectionTestMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        LogInformation("CollectionObjectTest - Workflow initialized");

        return Task.FromResult(new ScriptResponse
        {
            Key = "init-success",
            Data = new
            {
                testId = Guid.NewGuid().ToString(),
                startedAt = DateTime.UtcNow
            },
            Tags = new[] { "collection-object-test", "initialized" }
        });
    }
}
