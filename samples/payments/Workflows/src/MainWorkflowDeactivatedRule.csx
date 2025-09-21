using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class MainWorkflowDeactivatedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return false;

            // Check if payment schedule is deactivated or not active
            return paymentSchedule.status == "deactive" || paymentSchedule.isActive == false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}