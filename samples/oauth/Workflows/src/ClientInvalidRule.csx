using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Client Invalid Rule - Client validation failure condition mapping
/// This rule checks if the client validation was successful.
/// </summary>
public class ClientInvalidRule : IConditionMapping
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
                return true;
            }

            // Client validation data
            var clientValidation = context.Instance.Data.clientValidation;
            
            if (clientValidation == null)
            {
                return true;
            }

            // Check if the client validation was successful.
            return clientValidation.success == false;
        }
        catch (Exception ex)
        {
            // Exception durumunda true döndür (invalid)
            if (context?.MetaData != null)
            {
                context.MetaData["client_invalid_rule_error"] = new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    timestamp = DateTime.UtcNow
                };
            }

            return true; // Hata varsa invalid kabul et
        }
    }
}