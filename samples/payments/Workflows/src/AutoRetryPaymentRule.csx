using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class AutoRetryPaymentRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return false;

            // Check if auto-retry is enabled
            if (paymentSchedule.isAutoRetry != true)
                return false;

            // Check max retries
            var currentRetryCount = context.Instance?.Data?.retryCount ?? 0;
            var maxRetries = paymentSchedule.maxRetries ?? 3;

            return currentRetryCount < maxRetries;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
