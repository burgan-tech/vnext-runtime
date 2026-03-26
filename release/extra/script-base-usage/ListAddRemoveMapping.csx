using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: ListAdd(), ListRemove()
/// Starts with Alice(active), Bob(inactive), Charlie(active).
/// Adds Diana(active) → removes all inactive (Bob) → ends with Alice, Charlie, Diana.
/// </summary>
public class ListAddRemoveMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("ListAdd and ListRemove Test - Starting");

            object? instanceData = context.Instance?.Data;
            var items = GetList(instanceData, "items");

            var countBefore = ListCount(items);

            // Test ListAdd() - add a new item
            dynamic newItem = CreateObject();
            SetProperty(newItem, "id", "item-004");
            SetProperty(newItem, "name", "Diana");
            SetProperty(newItem, "age", 28);
            SetProperty(newItem, "status", "active");

            ListAdd(items, newItem);

            var countAfterAdd = ListCount(items);

            // Test ListRemove() - remove all inactive items (Bob)
            var removedCount = ListRemove(items, x => x.status == "inactive");

            var countAfterRemove = ListCount(items);

            // Verify no inactive items remain
            var hasInactiveAfterRemove = ListAny(items, x => x.status == "inactive");

            LogInformation("Add/Remove: before={0}, afterAdd={1}, removed={2}, afterRemove={3}",
                args: new object[] { countBefore, countAfterAdd, removedCount, countAfterRemove });

            return Task.FromResult(new ScriptResponse
            {
                Key = "add-remove-success",
                Data = new
                {
                    items = items,
                    listAddRemoveResult = new
                    {
                        success = true,
                        countBefore = countBefore,
                        countAfterAdd = countAfterAdd,
                        removedCount = removedCount,
                        countAfterRemove = countAfterRemove,
                        hasInactiveAfterRemove = hasInactiveAfterRemove,
                        addWorked = countAfterAdd == countBefore + 1,
                        removeWorked = removedCount == 1 && !hasInactiveAfterRemove
                    }
                },
                Tags = new[] { "collection-object-test", "list-add", "list-remove", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("ListAdd/Remove Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "add-remove-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
