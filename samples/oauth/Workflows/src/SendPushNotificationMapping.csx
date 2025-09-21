using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class SendPushNotificationMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var pushData = new
        {
            userId = context.Instance.Data?.authentication?.userId,
            deviceId = context.Instance.Data?.deviceRegistration?.deviceId,
            title = "Authentication Request",
            message = "Approve this login attempt from your device",
            actionType = "mfa_authentication",
            expiresIn = 120, // 2 minutes
            metadata = new
            {
                requestId = context.Instance?.Data?.requestId,
                clientId = context.Instance.Data?.clientValidation?.clientId,
                ipAddress = context.Instance?.Data?.ipAddress,
                userAgent = context.Instance?.Data?.userAgent
            }
        };
        httpTask.SetBody(pushData);
        var headers = new Dictionary<string, string?>
        {
            ["Content-Type"] = "application/json",
            ["X-Request-Id"] = context.Instance?.Data?.requestId,
            ["X-User-Id"] = context.Instance.Data?.authentication?.userId,
            ["X-Device-Id"] = context.Instance.Data?.deviceRegistration?.deviceId
        };
        httpTask.SetHeaders(headers);
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        var statusCode = context.Body.statusCode ?? 500;

        if (statusCode == 200)
        {
            response.Data = new
            {
                pushNotification = new
                {
                    sent = true,
                    notificationId = context.Body.data?.notificationId,
                    expiresAt = context.Body.data?.expiresAt,
                    deliveryStatus = context.Body.data?.status ?? "pending"
                }
            };
        }
        else
        {
            response.Data = new
            {
                pushNotification = new
                {
                    sent = false,
                    error = "Failed to send push notification",
                    errorCode = "push_send_failed"
                }
            };
        }

        return response;
    }
}