using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentConfigFailedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            if (context?.Instance?.Data == null)
                return true;

            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return true;

            return paymentSchedule.success == false;
        }
        catch (Exception)
        {
            return true;
        }
    }
}
