using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class ContinuePaymentCycleRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        // Always continue to next payment cycle
        return true;
    }
}