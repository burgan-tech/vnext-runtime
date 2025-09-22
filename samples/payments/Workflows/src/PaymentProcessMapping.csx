using System.Threading.Tasks;
using BBT.Workflow.Scripting;

public class PaymentProcessMapping : ISubFlowMapping
{
    public Task<ScriptResponse> InputHandler(ScriptContext context)
    {
        // Pass payment schedule data to subprocess
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
            Tags = new[] { "payments", "subflow", "completed", "payment-result-added" }
        });
    }
}