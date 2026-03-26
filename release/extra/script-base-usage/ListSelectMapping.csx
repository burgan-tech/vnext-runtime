using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: ListSelect&lt;TResult&gt;()
/// Projects list items into different types: string, int, object
/// </summary>
public class ListSelectMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("ListSelect Test - Starting");

            object instanceData = context.Instance?.Data;
            var items = GetList(instanceData, "items");

            // Test ListSelect<string>() - extract names
            var names = ListSelect<string>(items, x => (string)x.name);

            // Test ListSelect<int>() - extract ages
            var ages = ListSelect<int>(items, x => (int)x.age);

            // Test ListSelect<string>() with transformation
            var labels = ListSelect<string>(items, x => $"{x.name} ({x.status})");

            // Test ListSelect on empty list - should return empty list
            var emptyResult = ListSelect<string>(CreateList(), x => (string)x.name);

            LogInformation("ListSelect: {0} names, {1} ages", args: new object[] { names.Count, ages.Count });

            return Task.FromResult(new ScriptResponse
            {
                Key = "list-select-success",
                Data = new
                {
                    listSelectResult = new
                    {
                        success = true,
                        names = names,
                        ages = ages,
                        labels = labels,
                        namesCount = names.Count,
                        agesCount = ages.Count,
                        emptySelectCount = emptyResult.Count,
                        selectStringWorked = names.Count == 3 && names[0] == "Alice",
                        selectIntWorked = ages.Count == 3 && ages[0] == 30,
                        selectTransformWorked = labels.Count == 3,
                        emptySelectWorked = emptyResult.Count == 0
                    }
                },
                Tags = new[] { "collection-object-test", "list-select", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("ListSelect Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "list-select-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
