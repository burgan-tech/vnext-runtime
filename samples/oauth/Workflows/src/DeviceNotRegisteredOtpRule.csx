using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class DeviceNotRegisteredOtpRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        return context.Instance.Data.deviceRegistration.isRegistered == false;
    }
}