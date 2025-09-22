using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class UserInfoRetrievedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        // Always continue to send notifications, even if user info failed
        return context.Instance?.Data?.user != null;
    }
}
