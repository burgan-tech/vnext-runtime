using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class AuthorizationFailedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.authentication.success == false;
    }
}
