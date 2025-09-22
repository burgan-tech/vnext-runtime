using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class OtpSentRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.otp.sent == true;
    }
}