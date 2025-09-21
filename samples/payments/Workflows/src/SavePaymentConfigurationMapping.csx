using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Save Payment Configuration Mapping - Payment schedule configuration saving
/// </summary>
public class SavePaymentConfigurationMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for payment configuration");
            }

            // Prepare payment configuration data
            var paymentConfig = new
            {
                userId = context.Body?.userId,
                amount = context.Body?.amount,
                currency = context.Body?.currency ?? "USD",
                frequency = context.Body?.frequency ?? "monthly",
                startDate = context.Body?.startDate,
                endDate = context.Body?.endDate,
                paymentMethodId = context.Body?.paymentMethodId,
                description = context.Body?.description,
                recipientId = context.Body?.recipientId,
                isAutoRetry = context.Body?.isAutoRetry ?? true,
                maxRetries = context.Body?.maxRetries ?? 3
            };

            httpTask.SetBody(paymentConfig);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["X-Workflow-Instance"] = context.Instance?.Id.ToString(),
                ["X-User-Id"] = context.Body?.userId?.ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse
            {
                Key = "payment-config-error",
                Data = new { error = ex.Message }
            });
        }
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = context.Body?.statusCode ?? 500;
            var responseData = context.Body?.data;

            if (statusCode >= 200 && statusCode < 300)
            {
                var paymentSchedule = new
                {
                    success = true,
                    scheduleId = responseData?.scheduleId,
                    status = "inactive", // initial state
                    nextPaymentDate = responseData?.nextPaymentDate,
                    maxPayments = responseData?.maxPayments,
                    createdAt = DateTime.UtcNow,
                    completedPayments = 0,
                    lastPaymentAt = DateTime.UtcNow,
                    isActive = false,
                    activatedAt = DateTime.UtcNow,
                    archived = false,
                    finishedAt = DateTime.UtcNow
                };

                return new ScriptResponse
                {
                    Key = "payment-config-success",
                    Data = new
                    {
                        paymentSchedule = paymentSchedule,
                        paymentResults = new List<dynamic>()
                    },
                    Tags = new[] { "payments", "schedule", "configuration", "success" }
                };
            }
            else
            {
                var paymentSchedule = new
                {
                    success = false,
                    error = "Failed to save payment configuration",
                    errorCode = "config_save_failed",
                    statusCode = statusCode
                };

                return new ScriptResponse
                {
                    Key = "payment-config-failure",
                    Data = new
                    {
                        paymentSchedule = paymentSchedule,
                        paymentResults = new List<dynamic>()
                    },
                    Tags = new[] { "payments", "schedule", "configuration", "failure" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "payment-config-exception",
                Data = new
                {
                    paymentSchedule = new
                    {
                        success = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message
                    },
                    paymentResults = new List<dynamic>()
                },
                Tags = new[] { "payments", "schedule", "configuration", "error" }
            };
        }
    }
}