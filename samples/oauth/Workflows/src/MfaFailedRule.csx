using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class MfaFailedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.mfa.success == false;
    }
}