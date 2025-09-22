using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class IncrementRetryCounterMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        if (httpTask == null)
            throw new InvalidOperationException("Task must be an HttpTask");

        var currentRetryCount = context.Instance?.Data?.retryCount ?? 0;
        var incrementData = new
        {
            scheduleId = context.Instance?.Data?.paymentSchedule?.scheduleId,
            retryCount = currentRetryCount + 1,
            retryAt = DateTime.UtcNow
        };

        httpTask.SetBody(incrementData);
        var headers = new Dictionary<string, string?>
        {
            ["Content-Type"] = "application/json",
            ["X-Schedule-Id"] = context.Instance?.Data?.paymentSchedule?.scheduleId?.ToString()
        };
        httpTask.SetHeaders(headers);

        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        var currentRetryCount = context.Instance?.Data?.retryCount ?? 0;
        
        response.Data = new
        {
            retryCount = currentRetryCount + 1,
            retryScheduledAt = DateTime.UtcNow
        };

        return response;
    }
}