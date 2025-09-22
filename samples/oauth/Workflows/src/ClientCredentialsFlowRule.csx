using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class ClientCredentialsFlowRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.grant_type == "client_credentials";
    }
}