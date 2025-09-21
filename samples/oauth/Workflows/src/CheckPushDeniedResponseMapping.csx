using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

public class CheckPushDeniedResponseMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse());
    }

    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        var response = new ScriptResponse();
        response.Data = new
                {
                    mfa = new
                    {
                        success = false,
                        pushApproved = false,
                        method = "push",
                        approvedAt = DateTime.UtcNow
                    }
                };

        return response;
    }
}