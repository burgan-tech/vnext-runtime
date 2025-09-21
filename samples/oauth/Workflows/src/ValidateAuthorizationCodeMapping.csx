using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class ValidateAuthorizationCodeMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        var httpTask = task as HttpTask;
        var authCodeData = new
        {
            code = context.Body.code,
            clientId = context.Body.client_id,
            clientSecret = context.Body.client_secret,
            redirectUri = context.Body.redirect_uri,
            codeVerifier = context.Body.code_verifier // PKCE for enhanced security
        };
        httpTask.SetBody(authCodeData);
        var headers = new Dictionary<string, string?>
        {
            ["Content-Type"] = "application/json",
            ["X-Request-Id"] = context.Instance?.Data?.requestId,
        };
        httpTask.SetHeaders(headers);
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        var statusCode = context.Body.statusCode ?? 500;

        if (statusCode == 200)
        {
            response.Data = new
            {
                authentication = new
                {
                    success = true,
                    userId = context.Body.data?.userId,
                    grantType = "authorization_code",
                    redirectUri = context.Body.data?.redirectUri,
                    codeChallenge = context.Body.data?.codeChallenge,
                    codeChallengeMethod = context.Body.data?.codeChallengeMethod,
                    isValidationSuccess = context.Body.data?.isValid,
                    expiresAt = context.Body.data?.expiresAt,
                    authorizationCode = context.Body.data?.code
                }
            };
        }
        else if (statusCode == 400)
        {
            response.Data = new
            {
                authentication = new
                {
                    success = false,
                    error = "Invalid authorization code",
                    errorCode = "invalid_grant"
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
                    error = "Server error occurred",
                    errorCode = "server_error"
                }
            };
        }

        return response;
    }
}