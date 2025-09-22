using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class SendPaymentPushNotificationMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
                throw new InvalidOperationException("Task must be an HttpTask");

            // Check if user has registered devices
            var hasRegisteredDevices = context.Instance?.Data?.hasRegisteredDevices == true;
            if (!hasRegisteredDevices)
            {
                // Skip push notification if no devices
                return Task.FromResult(new ScriptResponse
                {
                    Key = "push-skipped",
                    Data = new { pushSent = false, reason = "No registered devices" }
                });
            }

            // Get data from context
            var paymentResult = context.Instance?.Data?.paymentResult;
            var user = context.Instance?.Data?.user;
            var registeredDevices = context.Instance?.Data?.registeredDevices;
            var language = user?.language ?? "tr-TR";

            // Generate push message
            var title = "";
            var body = "";
            if (paymentResult?.success == true)
            {
                if (language == "en-US")
                {
                    title = "Payment Successful";
                    body = $"Your payment of {paymentResult?.amount} {paymentResult?.currency} has been processed successfully.";
                }
                else
                {
                    title = "Ödeme Başarılı";
                    body = $"{paymentResult?.amount} {paymentResult?.currency} ödemeniz başarıyla işleme alındı.";
                }
            }
            else
            {
                if (language == "en-US")
                {
                    title = "Payment Failed";
                    body = "Your payment could not be processed. Please try again.";
                }
                else
                {
                    title = "Ödeme Başarısız";
                    body = "Ödemeniz gerçekleştirilemedi. Lütfen tekrar deneyin.";
                }
            }

            var pushData = new
            {
                title = title,
                body = body,
                userId = context.Instance?.Data?.userId,
                devices = registeredDevices,
                pushType = "payment-notification",
                data = new
                {
                    paymentId = paymentResult?.transactionId,
                    success = paymentResult?.success
                }
            };

            httpTask.SetBody(pushData);
            httpTask.SetHeaders(new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json"
            });

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse { Key = "push-error", Data = new { error = ex.Message } });
        }
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var statusCode = context.Body?.statusCode ?? 500;
        return new ScriptResponse
        {
            Key = statusCode >= 200 && statusCode < 300 ? "push-success" : "push-failure",
            Data = new { pushSent = statusCode >= 200 && statusCode < 300 }
        };
    }
}
