using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: CreateObject(), CreateList(), SetProperty(), ListAdd()
/// Creates a list of 3 dynamic person objects and stores them in Instance.Data.
/// </summary>
public class CreateAndSetMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("CreateAndSet Test - Starting");

            // Test CreateObject() + SetProperty()
            dynamic item1 = CreateObject();
            SetProperty(item1, "id", "item-001");
            SetProperty(item1, "name", "Alice");
            SetProperty(item1, "age", 30);
            SetProperty(item1, "status", "active");

            dynamic item2 = CreateObject();
            SetProperty(item2, "id", "item-002");
            SetProperty(item2, "name", "Bob");
            SetProperty(item2, "age", 25);
            SetProperty(item2, "status", "inactive");

            dynamic item3 = CreateObject();
            SetProperty(item3, "id", "item-003");
            SetProperty(item3, "name", "Charlie");
            SetProperty(item3, "age", 35);
            SetProperty(item3, "status", "active");

            // Test CreateList() + ListAdd()
            var items = CreateList();
            ListAdd(items, item1);
            ListAdd(items, item2);
            ListAdd(items, item3);

            // Nested dynamic object for metadata
            dynamic metadata = CreateObject();
            SetProperty(metadata, "createdAt", DateTime.UtcNow.ToString("O"));
            SetProperty(metadata, "source", "collection-object-test");
            SetProperty(metadata, "itemCount", items.Count);

            LogInformation("CreateAndSet Test - Created {0} items", args: new object[] { items.Count });

            return Task.FromResult(new ScriptResponse
            {
                Key = "create-and-set-success",
                Data = new
                {
                    items = items,
                    metadata = metadata,
                    createAndSetResult = new
                    {
                        success = true,
                        objectsCreated = 3,
                        listItemCount = items.Count,
                        propertiesSet = true
                    }
                },
                Tags = new[] { "collection-object-test", "create-and-set", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("CreateAndSet Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "create-and-set-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
