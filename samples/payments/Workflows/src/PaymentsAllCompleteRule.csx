using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentsAllCompleteRule : IConditionMapping
{
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            var paymentSchedule = context.Instance.Data.paymentSchedule;
            if (paymentSchedule == null)
                return false;

            // Check if end date is reached
            if (context.Instance.Data.endDate != null)
            {
                if (DateTime.TryParse(context.Instance.Data.endDate.ToString(), out DateTime endDate))
                {
                    if (DateTime.UtcNow >= endDate)
                        return true;
                }
            }

            // Check if maximum payment count is reached
            var maxPayments = paymentSchedule.maxPayments;
            var completedPayments = paymentSchedule.completedPayments ?? 0;

            if (maxPayments != null && completedPayments >= maxPayments)
                return true;

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}