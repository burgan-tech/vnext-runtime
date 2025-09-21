using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class DeactivatePaymentScheduleMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        if (httpTask == null)
            throw new InvalidOperationException("Task must be an HttpTask");

        var deactivationData = new
        {
            scheduleId = context.Instance?.Data?.paymentSchedule?.scheduleId,
            status = "deactive",
            deactivatedAt = DateTime.UtcNow,
            reason = context.Body?.reason ?? "Manual deactivation"
        };

        httpTask.SetBody(deactivationData);
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
        var statusCode = context.Body?.statusCode ?? 500;

        if (statusCode >= 200 && statusCode < 300)
        {
            response.Data = new
            {
                paymentSchedule = new
                {
                    status = "deactive",
                    deactivatedAt = DateTime.UtcNow,
                    isActive = false
                }
            };
        }
        else
        {
            response.Data = new
            {
                paymentSchedule = new
                {
                    isActive = true, // Still active if failed
                    error = "Failed to deactivate payment schedule"
                }
            };
        }

        return response;
    }
}