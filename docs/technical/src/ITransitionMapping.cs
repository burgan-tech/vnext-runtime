namespace BBT.Workflow.Scripting;

public interface ITransitionMapping 
{
    Task<dynamic> Handler(
        ScriptContext context)
    {
        return Task.FromResult(new { });
    }
}