using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class VerifyOtpCodeMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var verifyData = new
        {
            otpId = context.Instance.Data?.otp?.otpId,
            otpCode = context.Body.otp_code,
            userId = context.Instance.Data?.authentication?.userId,
            deviceId = context.Instance?.Data?.deviceId
        };
        httpTask.SetBody(verifyData);
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
                mfa = new
                {
                    success = true,
                    otpVerified = true,
                    method = "otp",
                    verifiedAt = DateTime.UtcNow.ToString("o")
                }
            };
        }
        else if (statusCode == 400)
        {
            response.Data = new
            {
                mfa = new
                {
                    success = false,
                    error = "Invalid OTP code",
                    errorCode = "invalid_otp",
                    attemptsRemaining = context.Body.data?.attemptsRemaining ?? 0
                }
            };
        }
        else
        {
            response.Data = new
            {
                mfa = new
                {
                    success = false,
                    error = "Server error occurred",
                    errorCode = "server_error"
                }
            };
        }

        return response;
    }
}