using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class ProcessOrderMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var headers = new Dictionary<string, string?>
        {
            ["Authorization"] = $"Berear {context.Instance.Data?.login?.currentLogin?.accessToken}"
        };
        httpTask.SetHeaders(headers);
        context.Body.userId = context.Instance.Data?.login?.currentLogin?.id;
        httpTask.SetBody(context.Body);
        return Task.FromResult(new ScriptResponse
        {
            Data = new { },
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
                order = new
                {
                    success = true,
                    context.Body.data
                }
            };
        }
        else if (statusCode == 404)
        {
            // Not found - handle specifically
            response.Data = new
            {
                order = new
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
                order = new
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