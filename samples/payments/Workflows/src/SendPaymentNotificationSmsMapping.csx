using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Send Payment Notification SMS Mapping - Prepares SMS notification data
/// </summary>
public class SendPaymentNotificationSmsMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for SMS notification");
            }

            // Get payment result from context
            var paymentResult = context.Instance?.Data?.paymentResult;
            var paymentSchedule = context.Instance?.Data?.paymentSchedule;
            var userId = context.Instance?.Data?.userId;

            // Determine message based on payment status
            var message = "";
            var language = context.Instance?.Data?.language ?? "tr-TR";

            if (paymentResult?.success == true)
            {
                if (language == "en-US")
                {
                    message = $"Payment successful! Amount: {paymentResult?.amount} {paymentResult?.currency}. Reference: {paymentResult?.transactionId}";
                }
                else
                {
                    message = $"Ödeme başarılı! Miktar: {paymentResult?.amount} {paymentResult?.currency}. Referans: {paymentResult?.transactionId}";
                }
            }
            else
            {
                if (language == "en-US")
                {
                    message = $"Payment failed. Reason: {paymentResult?.errorMessage ?? "Unknown error"}. Please check your payment details.";
                }
                else
                {
                    message = $"Ödeme başarısız. Neden: {paymentResult?.errorMessage ?? "Bilinmeyen sebep"}. Lütfen ödeme bilgilerinizi kontrol edin.";
                }
            }

            // Prepare SMS data
            var smsData = new
            {
                to = context.Instance?.Data?.phoneNumber ?? "[Phone Not Found]", // Should come from user data
                message = message,
                userId = userId,
                smsType = "payment-notification",
                templateId = "payment-notif-template",
                language = language,
                senderId = "PaymentSystem",
                timestamp = DateTime.UtcNow.ToString("iso8")
            };

            httpTask.SetBody(smsData);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["Accept"] = "application/json",
                ["X-Payment-Instance"] = context.Instance?.Id.ToString(),
                ["X-User-Id"] = userId?.ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse
            {
                Key = "sms-notification-error",
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
                return new ScriptResponse
                {
                    Key = "sms-notification-success",
                    Data = new
                    {
                        smsSent = true,
                        messageId = responseData?.messageId,
                        sentAt = DateTime.UtcNow,
                        smsStatus = "sent"
                    },
                    Tags = new[] { "payments", "sms", "notification", "success" }
                };
            }
            else
            {
                return new ScriptResponse
                {
                    Key = "sms-notification-failure",
                    Data = new
                    {
                        smsSent = false,
                        error = "Failed to send SMS notification",
                        errorCode = "sms_send_failed",
                        statusCode = statusCode,
                        smsStatus = "failed"
                    },
                    Tags = new[] { "payments", "sms", "notification", "failure" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "sms-notification-exception",
                Data = new
                {
                    smsSent = false,
                    error = "Internal processing error",
                    errorCode = "processing_error",
                    errorDescription = ex.Message,
                    smsStatus = "exception"
                },
                Tags = new[] { "payments", "sms", "notification", "error" }
            };
        }
    }
}
