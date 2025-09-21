using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// OAuth2 Client Validation Mapping - Client validation mapping
// This mapping is used to validate OAuth2 client credentials.
/// </summary>
public class ValidateClientMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for client validation");
            }

            // Prepare the request body - Required parameters for OAuth2 client validation
            var requestBody = new
            {
                client_id = context.Body?.client_id,
                client_secret = context.Body?.client_secret,
                grant_type = context.Body?.grant_type,
                scope = context.Body?.scope,
                username = context.Body?.username,
                password = context.Body?.password
            };

            httpTask.SetBody(requestBody);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["X-Workflow-Instance"] = context.Instance?.Id.ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse ());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse ());
        }
    }

    /// <summary>
    /// Process the client authentication result and merge it into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

            // Successful verification
            if (statusCode >= 200 && statusCode < 300)
            {
                var clientValidation = new
                {
                    success = true,
                    validatedAt = DateTime.UtcNow,
                    clientId = responseData?.clientId,
                    grantTypes = responseData?.grantTypes,
                    redirectUris = responseData?.redirectUris,
                    scopes = responseData?.scopes,
                    isActive = responseData?.isActive
                };

                return new ScriptResponse
                {
                    Key = "client-validation-success",
                    Data = new
                    {
                        clientValidation = clientValidation,
                        deviceId = context.Headers?["x-device-id"],
                        requestId = context.Headers?["x-request-id"],
                        userAgent = context.Headers?["user-agent"],
                        ipAddress = context.Headers?["x-forwarded-for"] ?? context.Headers?["x-real-ip"]
                    },
                    Tags = new[] { "oauth2", "client-validation", "success", "authenticated" }
                };
            }
            // Failed verification
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                
                var clientValidation = new
                {
                    success = false,
                    validatedAt = DateTime.UtcNow,
                    error = errorInfo.message,
                    errorCode = errorInfo.code,
                    errorDescription = errorInfo.description,
                    statusCode = statusCode
                };

                return new ScriptResponse
                {
                    Key = "client-validation-failure",
                    Data = new
                    {
                        clientValidation = clientValidation
                    },
                    Tags = new[] { "oauth2", "client-validation", "failure", "error" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "client-validation-output-error",
                Data = new
                {
                    clientValidation = new
                    {
                        success = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message,
                        validatedAt = DateTime.UtcNow
                    }
                },
                Tags = new[] { "oauth2", "client-validation", "error", "exception" }
            };
        }
    }

    #region Helper Methods

    private static int GetStatusCodeFromResponse(ScriptContext context)
    {
        // Get status code from standard response structure
        if (context.Body?.statusCode != null)
            return (int)context.Body.statusCode;

        // Accept success by default
        return 200;
    }

    private static dynamic? GetResponseDataFromContext(ScriptContext context)
    {
        // Get data from Body
        if (context.Body?.data != null)
            return context.Body.data;

        // Direct body content
        if (context.Body != null && context.Body.statusCode == null)
            return context.Body;

        return null;
    }

    private static (string message, string code, string description) ExtractErrorInformation(ScriptContext context, int statusCode)
    {
        var errorData = context.Body?.data?.error ?? null;
        
        string message = "Client validation failed";
        string code = "invalid_client";
        string description = "The client credentials provided are invalid";

        if (errorData != null)
        {
            message = errorData.message ?? message;
            code = errorData.code ?? code;
            description = errorData.description ?? description;
        }
        else if (statusCode == 401)
        {
            code = "unauthorized_client";
            description = "Client authentication failed";
        }
        else if (statusCode == 403)
        {
            code = "access_denied";
            description = "Client access denied";
        }
        else if (statusCode >= 500)
        {
            code = "server_error";
            description = "Internal server error during client validation";
        }

        return (message, code, description);
    }

    #endregion
}