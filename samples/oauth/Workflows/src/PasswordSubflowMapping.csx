using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PasswordSubflowMapping : ISubFlowMapping
{
    public Task<ScriptResponse> InputHandler(ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Instance.Data,
            Headers = null
        });
    }

    public Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        return Task.FromResult(new ScriptResponse
        {
            Data = context.Body,
            Headers = null
        });
    }
}





