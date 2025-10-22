using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class UserSessionMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    /// <summary>
    /// Populate the user session data into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return new ScriptResponse
            {
                Key = "user-session-output",
                Data = new
                {
                    userSession = new
                    {
                        userId = context.Headers?["user_reference"],
                        deviceId = context.Headers?["x-device-id"],
                        userAgent = context.Headers?["user-agent"],
                        ipAddress = context.Headers?["x-forwarded-for"] ?? context.Headers?["x-real-ip"]
                    }
                }
            };
    }
}
