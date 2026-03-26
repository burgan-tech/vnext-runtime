using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: ListFilter(), ListCount(), ListAny()
/// Uses the items list (Alice=active, Bob=inactive, Charlie=active) from Instance.Data.
/// </summary>
public class ListFilterCountAnyMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("ListFilter, ListCount, ListAny Test - Starting");

            object instanceData = context.Instance?.Data;
            var items = GetList(instanceData, "items");

            // Test ListFilter() - filter active items (Alice + Charlie)
            var activeItems = ListFilter(items, x => x.status == "active");

            // Test ListCount() - no predicate (total)
            var totalCount = ListCount(items);

            // Test ListCount() with predicate
            var activeCount = ListCount(items, x => x.status == "active");
            var inactiveCount = ListCount(items, x => x.status == "inactive");

            // Test ListAny() - no predicate (has any element)
            var hasItems = ListAny(items);

            // Test ListAny() with predicate
            var hasActive = ListAny(items, x => x.status == "active");
            var hasAdminRole = ListAny(items, x => x.status == "admin");

            // Empty list checks
            var emptyList = CreateList();
            var emptyHasItems = ListAny(emptyList);
            var emptyCount = ListCount(emptyList);

            LogInformation("ListFilter: active={0}, inactive={1}, total={2}",
                args: new object[] { activeCount, inactiveCount, totalCount });

            return Task.FromResult(new ScriptResponse
            {
                Key = "filter-count-any-success",
                Data = new
                {
                    filterCountAnyResult = new
                    {
                        success = true,
                        totalCount = totalCount,
                        activeCount = activeCount,
                        inactiveCount = inactiveCount,
                        activeItemsFiltered = activeItems.Count,
                        hasItems = hasItems,
                        hasActive = hasActive,
                        hasAdminRole = hasAdminRole,
                        emptyListHasItems = emptyHasItems,
                        emptyListCount = emptyCount,
                        filterWorked = activeItems.Count == 2,
                        countWorked = totalCount == 3 && activeCount == 2 && inactiveCount == 1,
                        anyWorked = hasItems && hasActive && !hasAdminRole && !emptyHasItems
                    }
                },
                Tags = new[] { "collection-object-test", "list-filter", "list-count", "list-any", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("ListFilter/Count/Any Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "filter-count-any-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
