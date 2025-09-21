using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class SendOtpNotificationMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var otpData = new
        {
            userId = context.Instance.Data?.authentication?.userId,
            email = context.Instance.Data?.authentication?.email,
            phone = context.Instance.Data?.authentication?.phone,
            method = "sms", // Default to sms, can be "sms" or "email"
            deviceId = context.Instance?.Data?.deviceId,
            language = context.Headers["accept-language"] ?? "en-US"
        };
        httpTask.SetBody(otpData);
        var headers = new Dictionary<string, string?>
        {
            ["Content-Type"] = "application/json",
            ["X-Request-Id"] = context.Instance?.Data?.requestId,
            ["X-User-Id"] = context.Instance.Data?.authentication?.userId
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
                otp = new
                {
                    sent = true,
                    otpId = context.Body.data?.otpId,
                    expiresAt = context.Body.data?.expiresAt,
                    method = context.Body.data?.method,
                    attemptsRemaining = context.Body.data?.attemptsRemaining ?? 3
                }
            };
        }
        else
        {
            response.Data = new
            {
                otp = new
                {
                    sent = false,
                    error = "Failed to send OTP",
                    errorCode = "otp_send_failed"
                }
            };
        }

        return response;
    }
}