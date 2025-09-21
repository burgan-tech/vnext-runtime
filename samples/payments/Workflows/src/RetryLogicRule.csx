using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class RetryLogicRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        // Always return true to proceed with retry
        return true;
    }
}