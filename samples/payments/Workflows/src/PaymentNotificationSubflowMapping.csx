using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentNotificationSubflowMapping : ISubProcessMapping
{
    public Task<ScriptResponse> InputHandler(ScriptContext context)
    {
        // Pass payment data and user info to notification subprocess
        dynamic notificationData = new ExpandoObject();
        notificationData.paymentResult   = context.Instance?.Data?.paymentResult;
        notificationData.paymentSchedule = context.Instance?.Data?.paymentSchedule;
        notificationData.userId          = context.Instance?.Data?.userId;
        notificationData.notificationType= "payment-result";
        notificationData.timestamp       = DateTime.UtcNow.ToString("o");

        return Task.FromResult(new ScriptResponse
        {
            Data = notificationData,
            Headers = null
        });
    }
}