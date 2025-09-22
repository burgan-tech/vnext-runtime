using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class TokensGeneratedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.tokens.success == true;
    }
}