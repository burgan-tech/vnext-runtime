using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// OAuth2 User Credentials Validation Mapping - User credential validation mapping
// This mapping is used to validate user credentials in the OAuth2 password grant flow.
/// </summary>
public class ValidateUserCredentialsMapping : IMapping
{
    /// <summary>
    /// Prepare input for user credential validation request
    /// </summary>
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for user credentials validation");
            }

            var requestBody = new
            {
                username = context.Body?.username ?? context.Body?.email,
                password = context.Body?.password
            };

            httpTask.SetBody(requestBody);

            // Set the headers
            var headers = new Dictionary<string, string?>
            {
                ["X-Request-Id"] = context.Instance?.Data?.requestId,
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
    /// Process user authentication result and merge it into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
         var response = new ScriptResponse();
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

            // Successful validation
            if (statusCode >= 200 && statusCode < 300)
            {
                response.Data = new
                {
                    authentication = new 
                    {
                        success = true,
                        userId = responseData?.userId,
                        username = responseData?.username,
                        email = responseData?.email,
                        roles = responseData?.roles,
                        // MFA context
                        mfaRequired = responseData?.mfaRequired ?? true
                    }
                };
            }
            else
            {
                response.Data = new
                {
                    authentication = new
                    {
                        success = false,
                        error = "Invalid username or password",
                        errorCode = "invalid_credentials"
                    }
                };
            }
        }
        catch (Exception ex)
        {
            response.Data = new
            {
                authentication = new
                {
                    success = false,
                    error = "Internal processing error",
                    errorCode = "processing_error",
                    errorDescription = ex.Message
                }
            };
        }
        return response;
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

    #endregion
}
