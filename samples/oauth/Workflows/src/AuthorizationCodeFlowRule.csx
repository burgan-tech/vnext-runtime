using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class AuthorizationCodeFlowRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.grant_type == "authorization_code";
    }
}