using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions.Timer;

public class PaymentDueTimerRule : ITimerMapping
{
    public async Task<TimerSchedule> Handler(ScriptContext context)
    {
        try
        {
            return TimerSchedule.FromDateTime(DateTime.UtcNow.AddSeconds(60)); // For testing
            // var paymentSchedule = context.Instance.Data.paymentSchedule;
            // if (paymentSchedule == null)
            //     return TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(1)); // Default to 1 day
    
            // if (paymentSchedule.nextPaymentDate != null)
            // {
            //     if (DateTime.TryParse(paymentSchedule.nextPaymentDate.ToString(), out DateTime nextPayment))
            //         return TimerSchedule.FromDateTime(nextPayment);
            // }

            // // Calculate next payment based on frequency
            // var frequency = context.Instance.Data.frequency?.ToString().ToLower() ?? "monthly";
            // return frequency switch
            // {
            //     "daily" => TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(1)),
            //     "weekly" => TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(7)),
            //     "monthly" => TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            //     "quarterly" => TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            //     "yearly" => TimerSchedule.FromDateTime(DateTime.UtcNow.AddYears(1)),
            //     _ => TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(1))
            // };
        }
        catch (Exception)
        {
            return TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(1)); // Fallback
        }
    }
}
