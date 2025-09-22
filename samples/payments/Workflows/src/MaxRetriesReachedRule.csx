using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class MaxRetriesReachedRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return false;

            var currentRetryCount = context.Instance?.Data?.retryCount ?? 0;
            var maxRetries = paymentSchedule.maxRetries ?? 3;

            return currentRetryCount >= maxRetries;
        }
        catch (Exception)
        {
            return true; // Default to true if exception occurs
        }
    }
}