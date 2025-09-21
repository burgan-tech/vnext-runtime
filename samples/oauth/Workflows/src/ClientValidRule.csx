using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Client Valid Rule - Client validation success condition mapping
/// This rule checks if the client validation was successful.
/// </summary>
public class ClientValidRule : IConditionMapping
{
    /// <summary>
    /// Check if the client validation was successful.
    /// </summary>
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            // Null check
            if (context?.Instance?.Data == null)
            {
                return false;
            }

            // Client validation data
            var clientValidation = context.Instance.Data.clientValidation;
            
            if (clientValidation == null)
            {
                return false;
            }

            // Check if the client validation was successful.
            return clientValidation.success == true;
        }
        catch (Exception ex)
        {
            // If an exception occurs, return false and write to audit log
            if (context?.MetaData != null)
            {
                context.MetaData["client_valid_rule_error"] = new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    timestamp = DateTime.UtcNow
                };
            }

            return false;
        }
    }
}