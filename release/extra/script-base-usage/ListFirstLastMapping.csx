using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: ListFirst(), ListLast()
/// Items order: Alice(active), Bob(inactive), Charlie(active)
/// </summary>
public class ListFirstLastMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("ListFirst and ListLast Test - Starting");

            object instanceData = context.Instance?.Data;
            var items = GetList(instanceData, "items");

            // Test ListFirst() without predicate → Alice
            var first = ListFirst(items);

            // Test ListFirst() with predicate → first active = Alice, first inactive = Bob
            var firstActive = ListFirst(items, x => x.status == "active");
            var firstInactive = ListFirst(items, x => x.status == "inactive");

            // Test ListFirst() with no match → null
            var firstNotFound = ListFirst(items, x => x.status == "admin");

            // Test ListLast() without predicate → Charlie
            var last = ListLast(items);

            // Test ListLast() with predicate → last active = Charlie
            var lastActive = ListLast(items, x => x.status == "active");

            // Test on empty list → null
            var emptyFirst = ListFirst(CreateList());
            var emptyLast = ListLast(CreateList());

            LogInformation("First: {0}, Last: {1}", args: new object[] { first?.name, last?.name });

            return Task.FromResult(new ScriptResponse
            {
                Key = "first-last-success",
                Data = new
                {
                    firstLastResult = new
                    {
                        success = true,
                        firstName = (string)(first?.name ?? "null"),
                        lastName = (string)(last?.name ?? "null"),
                        firstActiveName = (string)(firstActive?.name ?? "null"),
                        firstInactiveName = (string)(firstInactive?.name ?? "null"),
                        lastActiveName = (string)(lastActive?.name ?? "null"),
                        firstNotFoundIsNull = firstNotFound == null,
                        emptyFirstIsNull = emptyFirst == null,
                        emptyLastIsNull = emptyLast == null,
                        firstWorked = first?.id == "item-001",
                        lastWorked = last?.id == "item-003",
                        firstWithPredicateWorked = firstActive?.id == "item-001",
                        lastWithPredicateWorked = lastActive?.id == "item-003"
                    }
                },
                Tags = new[] { "collection-object-test", "list-first", "list-last", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("ListFirst/Last Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "first-last-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
