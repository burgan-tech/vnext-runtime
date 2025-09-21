using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PushPendingContinueRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.mfa.success == null && 
               context.Instance.Data.mfa.status == "pending";
    }
}