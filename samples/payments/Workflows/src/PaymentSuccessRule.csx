using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentSuccessRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            var paymentResult = context.Instance.Data.paymentResult;
            if (paymentResult == null)
                return false;

            return paymentResult.success == true && paymentResult.status == "success";
        }
        catch (Exception)
        {
            return false;
        }
    }
}
