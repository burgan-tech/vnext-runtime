using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: GetList(), AsList()
/// Retrieves the items list from Instance.Data and verifies AsList() edge cases.
/// </summary>
public class GetListAndAsListMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("GetList and AsList Test - Starting");

            var instanceData = context.Instance?.Data;

            // Test GetList() - retrieve items from instance data by property name
            var items = GetList(instanceData, "items");

            // Test AsList() with a valid list - should return same items
            var asListFromValid = AsList(items);

            // Test AsList() with null - should return empty list (graceful degradation)
            var asListFromNull = AsList(null);

            // Test AsList() with a non-list value - should return empty list
            var asListFromString = AsList("not a list");

            LogInformation("GetList Test - Retrieved {0} items", args: new object[] { items.Count });

            return Task.FromResult(new ScriptResponse
            {
                Key = "get-list-success",
                Data = new
                {
                    getListResult = new
                    {
                        success = true,
                        itemCount = items.Count,
                        getListWorked = items.Count == 3,
                        asListFromValidCount = asListFromValid.Count,
                        asListNullReturnsEmpty = asListFromNull.Count == 0,
                        asListInvalidReturnsEmpty = asListFromString.Count == 0
                    }
                },
                Tags = new[] { "collection-object-test", "get-list", "as-list", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("GetList Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "get-list-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
