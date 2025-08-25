using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class AuthFailedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.login.success == false;
    }
}