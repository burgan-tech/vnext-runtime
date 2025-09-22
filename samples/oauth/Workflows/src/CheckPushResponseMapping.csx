using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class CheckPushResponseMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var checkData = new
        {
            notificationId = context.Instance.Data?.pushNotification?.notificationId,
            userId = context.Instance.Data?.authentication?.userId,
            deviceId = context.Instance.Data?.deviceRegistration?.deviceId
        };
        httpTask.SetBody(checkData);
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
            var responseStatus = context.Body.data?.status;
            if (responseStatus == "approved")
            {
                response.Data = new
                {
                    mfa = new
                    {
                        success = true,
                        pushApproved = true,
                        method = "push",
                        approvedAt = context.Body.data?.respondedAt
                    }
                };
            }
            else if (responseStatus == "denied")
            {
                response.Data = new
                {
                    mfa = new
                    {
                        success = false,
                        error = "Authentication request denied by user",
                        errorCode = "access_denied"
                    }
                };
            }
            else
            {
                // Still pending, continue waiting
                response.Data = new
                {
                    mfa = new
                    {
                        success = false, // Still pending
                        status = "pending"
                    }
                };
            }
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