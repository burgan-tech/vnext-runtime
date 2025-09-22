using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class ArchivePaymentRecordMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        if (httpTask == null)
            throw new InvalidOperationException("Task must be an HttpTask");

        var archiveData = new
        {
            scheduleId = context.Instance?.Data?.paymentSchedule?.scheduleId,
            status = "archived",
            archivedAt = DateTime.UtcNow,
            completionReason = "All payments completed",
            finalPaymentCount = context.Instance?.Data?.paymentSchedule?.completedPayments
        };

        httpTask.SetBody(archiveData);
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

        response.Data = new
        {
            paymentSchedule = new
            {
                status = "finished",
                archived = statusCode >= 200 && statusCode < 300,
                finishedAt = DateTime.UtcNow
            }
        };

        return response;
    }
}
