using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Device Registration Check Mapping - Device registration status check mapping
/// This mapping is used to check whether the device is registered during the MFA process.
/// </summary>
public class CheckDeviceRegistrationMapping : IMapping
{
    /// <summary>
    /// Prepare input for device registration check
    /// </summary>
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for device registration check");
            }
            
            // Get information from authentication context
            var authData = context.Instance?.Data?.authentication;
            var clientData = context.Instance?.Data?.clientValidation;

            // Query parameters to be sent
            var queryParams = new Dictionary<string, string?>
            {
                ["user_id"] = authData?.userId?.ToString(),
                ["device_id"] = context.Instance?.Data?.deviceId,
                ["client_id"] = clientData?.clientId?.ToString() ?? clientData?.clientId?.ToString()
            };

            // Update URL with query parameters
            var baseUrl = httpTask.Url;
            var queryString = string.Join("&", queryParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                                                         .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}"));
            
            if (!string.IsNullOrEmpty(queryString))
            {
                httpTask.Url = $"{baseUrl}?{queryString}";
            }

            // Set headers
            var headers = new Dictionary<string, string?>
            {
                ["X-Device-Id"] = context.Instance?.Data?.deviceId,
                ["X-Request-Id"] = context.Instance?.Data?.requestId,
                ["X-Workflow-Instance"] = context.Instance?.Id.ToString(),
                ["Content-Type"] = "application/json"
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse());
        }
    }

    /// <summary>
    /// Process device registration check result and merge it into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

            // Successful check
            if (statusCode >= 200 && statusCode < 300)
            {
                var deviceRegistration = new
                {
                    checkedAt = DateTime.UtcNow,
                    isRegistered = responseData?.isRegistered ?? false,
                    deviceId = responseData?.deviceId,
                    deviceName = responseData?.deviceName,
                    deviceType = responseData?.deviceType ?? "mobile",
                    supportsPush = responseData?.supportsPush ?? false,
                    registeredAt = responseData?.registeredAt,
                    lastSeenAt = responseData?.lastSeenAt
                };

                return new ScriptResponse
                {
                    Data = new
                    {
                        deviceRegistration = deviceRegistration
                    }
                };
            }
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                
                var deviceRegistration = new
                {
                    checkedAt = DateTime.UtcNow,
                    isRegistered = false,
                    error = errorInfo.message,
                    errorCode = errorInfo.code,
                    errorDescription = errorInfo.description,
                    statusCode = statusCode
                };

                return new ScriptResponse
                {
                    Data = new
                    {
                        deviceRegistration = deviceRegistration
                    }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Data = new
                {
                    deviceRegistration = new
                    {
                        checkedAt = DateTime.UtcNow,
                        isRegistered = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message
                    }
                }
            };
        }
    }

    #region Helper Methods

    private static int GetStatusCodeFromResponse(ScriptContext context)
    {
        if (context.Body?.statusCode != null)
            return (int)context.Body.statusCode;

        return 200;
    }

    private static dynamic? GetResponseDataFromContext(ScriptContext context)
    {
        if (context.Body?.data != null)
            return context.Body.data;

        if (context.Body != null && context.Body.statusCode == null)
            return context.Body;

        return null;
    }
    
    private static (string message, string code, string description) ExtractErrorInformation(ScriptContext context, int statusCode)
    {
        var errorData = context.Body?.data?.error ?? null;
        
        string message = "Device validation failed";
        string code = "invalid_device";
        string description = "The device credentials provided are invalid";

        if (errorData != null)
        {
            message = errorData.message ?? message;
            code = errorData.code ?? code;
            description = errorData.description ?? description;
        }
        else if (statusCode == 401)
        {
            code = "unauthorized_device";
            description = "Device authentication failed";
        }
        else if (statusCode == 403)
        {
            code = "access_denied";
            description = "Device access denied";
        }
        else if (statusCode >= 500)
        {
            code = "server_error";
            description = "Internal server error during device validation";
        }

        return (message, code, description);
    }
    

    #endregion
}