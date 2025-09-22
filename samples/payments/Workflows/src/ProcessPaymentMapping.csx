
using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Process Payment Mapping - Handles payment processing logic
/// </summary>
public class ProcessPaymentMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for payment processing");
            }

            // Prepare payment processing data
            var paymentSchedule = context.Instance?.Data?.paymentSchedule;
            var paymentRequest = new
            {
                scheduleId = paymentSchedule?.scheduleId,
                userId = context.Instance?.Data?.userId,
                amount = context.Instance?.Data?.amount,
                currency = context.Instance?.Data?.currency,
                processedAt = DateTime.UtcNow
            };

            httpTask.SetBody(paymentRequest);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["X-Schedule-Id"] = paymentSchedule?.scheduleId?.ToString(),
                ["X-Payment-Request-Id"] = Guid.NewGuid().ToString(),
                ["X-Retry-Count"] = context.Instance?.Data.maxRetries.ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse
            {
                Key = "payment-processing-error",
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
                var paymentResult = new
                {
                    status = "success",
                    success = true,
                    transactionId = responseData?.transactionId,
                    amount = responseData?.amount,
                    currency = responseData?.currency,
                    processedAt = DateTime.UtcNow,
                    paymentMethod = responseData?.paymentMethod,
                    fees = responseData?.fees
                };

                // Update payment counter
                var paymentSchedule = context.Instance?.Data?.paymentSchedule ?? new { };
                paymentSchedule.completedPayments = (paymentSchedule.completedPayments ?? 0) + 1;
                paymentSchedule.lastPaymentAt = DateTime.UtcNow;

                return new ScriptResponse
                {
                    Key = "payment-success",
                    Data = new
                    {
                        paymentResult = paymentResult,
                        paymentSchedule = paymentSchedule
                    },
                    Tags = new[] { "payments", "success", "transaction" }
                };
            }
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                var currentRetryCount = context.Instance?.Data?.maxRetries ?? 0;

                var paymentResult = new
                {
                    status = "failed",
                    success = false,
                    error = errorInfo.message,
                    errorCode = errorInfo.code,
                    errorDescription = errorInfo.description,
                    statusCode = statusCode,
                    failedAt = DateTime.UtcNow,
                    retryCount = currentRetryCount
                };

                return new ScriptResponse
                {
                    Key = "payment-failure",
                    Data = new
                    {
                        paymentResult = paymentResult,
                        retryCount = currentRetryCount
                    },
                    Tags = new[] { "payments", "failure", "error", "retry-potential" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "payment-processing-exception",
                Data = new
                {
                    paymentResult = new
                    {
                        status = "failed",
                        success = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message,
                        failedAt = DateTime.UtcNow
                    }
                },
                Tags = new[] { "payments", "failure", "error", "exception" }
            };
        }
    }

    private static (string message, string code, string description) ExtractErrorInformation(ScriptContext context, int statusCode)
    {
        var errorData = context.Body?.error ?? context.Body?.data?.error;

        string message = "Payment processing failed";
        string code = "payment_processing_error";
        string description = "Unable to process payment";

        if (errorData != null)
        {
            message = errorData.message ?? errorData.error ?? message;
            code = errorData.code ?? errorData.error_code ?? code;
            description = errorData.description ?? errorData.error_description ?? description;
        }
        else if (statusCode == 400)
        {
            code = "invalid_request";
            description = "Invalid payment request";
        }
        else if (statusCode == 401)
        {
            code = "unauthorized";
            description = "Unauthorized payment attempt";
        }
        else if (statusCode == 402)
        {
            code = "insufficient_funds";
            description = "Insufficient funds for payment";
        }
        else if (statusCode == 403)
        {
            code = "forbidden";
            description = "Payment forbidden";
        }
        else if (statusCode == 404)
        {
            code = "payment_method_not_found";
            description = "Payment method not found";
        }
        else if (statusCode == 429)
        {
            code = "too_many_requests";
            description = "Too many payment requests";
        }
        else if (statusCode >= 500)
        {
            code = "server_error";
            description = "Internal server error during payment processing";
        }

        return (message, code, description);
    }
}