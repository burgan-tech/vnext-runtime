using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class TokenGenerationFailedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.tokens.success == false;
    }
}