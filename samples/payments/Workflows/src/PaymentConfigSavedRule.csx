using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentConfigSavedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            if (context?.Instance?.Data == null)
                return false;

            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return false;

            return paymentSchedule.success == true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
