using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class AuthLoginMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        httpTask.SetBody(context.Body);
        var headers = new Dictionary<string, string?>
        {
            ["X-Request-Id"] = context.Headers["x-request-id"],
            ["X-Device-Id"] = context.Headers["x-device-id"]
        };
        httpTask.SetHeaders(headers);
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data,
            Headers = null
        });
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        // Store authentication token for future requests
        var response = new ScriptResponse();
        var statusCode = context.Body.statusCode ?? 500;

        if (statusCode == 200)
        {
            response.Data = new
            {
                login = new
                {
                    success = true,
                    currentLogin = context.Body.data
                }
            };
        }
        else if (statusCode == 404)
        {
            // Not found - handle specifically
            response.Data = new
            {
                login = new
                {
                    success = false,
                    error = "Resource not found",
                    shouldRetry = false
                }
            };
        }
        else
        {
            // Server error - might want to retry
            response.Data = new
            {
                login = new
                {
                    success = false,
                    error = "Server error occurred",
                    shouldRetry = true,
                    retryAfter = 30 // seconds
                }
            };
        }

        return response;
    }
}