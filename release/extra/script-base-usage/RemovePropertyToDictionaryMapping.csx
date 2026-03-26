using System;
using System.Threading.Tasks;
using BBT.Workflow.Definitions;
using BBT.Workflow.Scripting;
using BBT.Workflow.Scripting.Functions;

/// <summary>
/// Tests: RemoveProperty(), ToDictionary(), HasProperty()
/// Creates a dynamic object, removes a property, converts to dictionary.
/// </summary>
public class RemovePropertyToDictionaryMapping : ScriptBase, IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            LogInformation("RemoveProperty and ToDictionary Test - Starting");

            // Create a test object with multiple properties
            dynamic testObj = CreateObject();
            SetProperty(testObj, "id", "test-001");
            SetProperty(testObj, "name", "Test Object");
            SetProperty(testObj, "tempField", "this will be removed");
            SetProperty(testObj, "keepField", "this stays");

            // Verify property exists before removal (uses HasProperty from ScriptBase)
            var hadTempField = HasProperty(testObj, "tempField");
            var hadKeepField = HasProperty(testObj, "keepField");

            // Test RemoveProperty()
            var removeResult = RemoveProperty(testObj, "tempField");

            // Verify property is gone
            var hasTempFieldAfter = HasProperty(testObj, "tempField");
            var hasKeepFieldAfter = HasProperty(testObj, "keepField");

            // Test RemoveProperty() on non-existent property → false
            var removeNonExistent = RemoveProperty(testObj, "doesNotExist");

            // Test ToDictionary()
            var dict = ToDictionary(testObj);
            var dictHasId = dict.ContainsKey("id");
            var dictHasName = dict.ContainsKey("name");
            var dictHasTemp = dict.ContainsKey("tempField");
            var dictHasKeep = dict.ContainsKey("keepField");

            // Test ToDictionary() with null → empty dictionary
            var emptyDict = ToDictionary(null);

            LogInformation("RemoveProperty: removed={0}, dictCount={1}", args: new object[] { removeResult, dict.Count });

            return Task.FromResult(new ScriptResponse
            {
                Key = "remove-to-dict-success",
                Data = new
                {
                    removeToDictResult = new
                    {
                        success = true,
                        hadTempFieldBefore = hadTempField,
                        hadKeepFieldBefore = hadKeepField,
                        removePropertyResult = removeResult,
                        hasTempFieldAfterRemove = hasTempFieldAfter,
                        hasKeepFieldAfterRemove = hasKeepFieldAfter,
                        removeNonExistentReturnsFalse = !removeNonExistent,
                        dictCount = dict.Count,
                        dictHasId = dictHasId,
                        dictHasName = dictHasName,
                        dictRemovedTempField = !dictHasTemp,
                        dictKeptKeepField = dictHasKeep,
                        nullToDictReturnsEmpty = emptyDict.Count == 0,
                        removePropertyWorked = removeResult && !hasTempFieldAfter && hasKeepFieldAfter,
                        toDictionaryWorked = dictHasId && dictHasName && !dictHasTemp && dictHasKeep
                    }
                },
                Tags = new[] { "collection-object-test", "remove-property", "to-dictionary", "has-property", "success" }
            });
        }
        catch (Exception ex)
        {
            LogError("RemoveProperty/ToDictionary Test - Failed: {0}", args: new object[] { ex.Message });
            return Task.FromResult(new ScriptResponse
            {
                Key = "remove-to-dict-error",
                Data = new { error = ex.Message }
            });
        }
    }
}
