using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentErrorRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            var paymentResult = context.Instance.Data.paymentResult;
            if (paymentResult == null)
                return false;

            return paymentResult.success == false && paymentResult.status == "failed";
        }
        catch (Exception)
        {
            return true; // Default to error if exception occurs
        }
    }
}
