using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class NotificationsSentRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        // Always move to completion, regardless of notification results
        return true;
    }
}
