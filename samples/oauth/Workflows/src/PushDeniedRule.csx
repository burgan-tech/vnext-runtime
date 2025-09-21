using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PushDeniedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.mfa.success == false && 
               context.Instance.Data.mfa.errorCode == "access_denied";
    }
}