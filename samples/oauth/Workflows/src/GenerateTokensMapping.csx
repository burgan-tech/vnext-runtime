using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Generate Tokens Mapping - OAuth2 token generation mapping
// This mapping is used to generate access and refresh tokens after the authentication process is complete.
/// </summary>
public class GenerateTokensMapping : IMapping
{
    /// <summary>
    /// Preparing input for token creation request
    /// </summary>
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for token generation");
            }

            // Gather all information from the workflow context
            var clientData = context.Instance?.Data?.clientValidation;
            var authData = context.Instance?.Data?.authentication;
            var mfaData = context.Instance?.Data?.mfa;
            var deviceData = context.Instance?.Data?.deviceRegistration;

            // Token generation request body
            var requestBody = new
            {
                // Client information
                client_id = clientData?.clientId,
                client_secret = context.Instance?.Data?.client_secret,
                
                // Grant type and user information
                grant_type = context.Instance?.Data?.grant_type,
                user_id = authData?.userId,
                username = authData?.username,
                email = authData?.email,
                
                // Scope ve permissions
                scope = context.Instance?.Data?.scope,
                user_roles = authData?.roles ?? new[] { "retail-user" }, 
                
                // Device context
                device_id = deviceData?.deviceId,
                device_registered = deviceData?.isRegistered,
                
                // MFA context
                mfa_completed = mfaData?.success ?? false,
                mfa_method = mfaData?.method ?? "none",
                
                // Security context
                ip_address = context.Headers?["x-forwarded-for"] ?? context.Headers?["x-real-ip"],
                user_agent = context.Headers?["user-agent"]
            };

            httpTask.SetBody(requestBody);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["X-Request-Id"] = context.Instance?.Data?.deviceId,
                ["Content-Type"] = "application/json",
                ["X-Workflow-Instance"] = context.Instance?.Id.ToString(),
                ["X-User-Id"] = requestBody.user_id?.ToString(),
                ["X-Device-Id"] = requestBody.device_id,
                ["X-Grant-Type"] = requestBody.grant_type
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse
            {
                Key = "token-generation-input-error",
                Data = new
                {
                    error = new
                    {
                        message = ex.Message,
                        type = ex.GetType().Name,
                        timestamp = DateTime.UtcNow,
                        workflow_instance = context.Instance?.Id
                    }
                },
                Tags = new[] { "oauth2", "token-generation", "input-error" }
            });
        }
    }

    /// <summary>
   /// Process the token creation result and merge it into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

           // Successful token generation
            if (statusCode >= 200 && statusCode < 300)
            {
                var tokens = new
                {
                    success = true,
                    generated_at = DateTime.UtcNow,
                    access_token = responseData?.access_token,
                    refresh_token = responseData?.refresh_token,
                    token_type = responseData?.token_type ?? "Bearer",
                    expires_in = responseData?.expires_in,
                    scope = responseData?.scope
                };

                return new ScriptResponse
                {
                    Key = "token-generation-success",
                    Data = new
                    {
                        tokens = tokens
                    },
                    Tags = new[] { "oauth2", "token-generation", "success", "authentication-completed", "tokens-issued" }
                };
            }
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                
                var tokens = new
                {
                    success = false,
                    generated_at = DateTime.UtcNow,
                    error = errorInfo.message,
                    error_code = errorInfo.code,
                    error_description = errorInfo.description,
                    status_code = statusCode,
                    // Metadata
                    request_id = context.Instance?.Data?.deviceId
                };

                return new ScriptResponse
                {
                    Key = "token-generation-failure",
                    Data = new
                    {
                        tokens = tokens
                    },
                    Tags = new[] { "oauth2", "token-generation", "failure", "error", 
                                  "authentication-failed", "tokens-not-issued" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "token-generation-output-error",
                Data = new
                {
                    tokens = new
                    {
                        success = false,
                        error = "Internal processing error",
                        error_code = "processing_error",
                        error_description = ex.Message,
                        generated_at = DateTime.UtcNow
                    }
                },
                Tags = new[] { "oauth2", "token-generation", "error", "exception", "processing-error" }
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
        var errorData = context.Body?.error ?? context.Body?.data?.error;
        
        string message = "Token generation failed";
        string code = "token_generation_error";
        string description = "Unable to generate access tokens";

        if (errorData != null)
        {
            message = errorData.message ?? errorData.error ?? message;
            code = errorData.code ?? errorData.error_code ?? code;
            description = errorData.description ?? errorData.error_description ?? description;
        }
        else if (statusCode == 400)
        {
            code = "invalid_request";
            description = "Invalid token generation request";
        }
        else if (statusCode == 401)
        {
            code = "invalid_client";
            description = "Client authentication failed during token generation";
        }
        else if (statusCode == 403)
        {
            code = "access_denied";
            description = "Access denied for token generation";
        }
        else if (statusCode >= 500)
        {
            code = "server_error";
            description = "Internal server error during token generation";
        }

        return (message, code, description);
    }

    #endregion
}