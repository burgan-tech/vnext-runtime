using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Account Created Successfully Rule - Checks if account creation was successful
/// This rule determines if the bank account was successfully created in the core banking system.
/// </summary>
public class AccountCreatedSuccessfullyRule : IConditionMapping
{
    /// <summary>
    /// Check if the account creation was successful
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

            // Account creation data
            var accountCreation = context.Instance.Data.accountCreation;
            
            if (accountCreation == null)
            {
                return false;
            }

            // Check if the account creation was successful
            var success = accountCreation.success == true;
            
            // Additional validation checks for successful creation
            if (success)
            {
                // Ensure essential account details are present
                var accountId = accountCreation.accountId;
                var accountNumber = accountCreation.accountNumber;
                var status = accountCreation.status;
                
                // Account must have ID and number
                if (string.IsNullOrEmpty(accountId?.ToString()) || 
                    string.IsNullOrEmpty(accountNumber?.ToString()))
                {
                    success = false;
                }
                
                // Account status should be active
                if (status?.ToString() != "active")
                {
                    success = false;
                }
                
                // Check if account is marked as active
                var isActive = accountCreation.isActive;
                if (isActive != true)
                {
                    success = false;
                }
                
                // Verify IBAN is present for certain currencies
                var currency = accountCreation.currency?.ToString();
                var iban = accountCreation.iban;
                if ((currency == "EUR" || currency == "USD" || currency == "GBP") && 
                    string.IsNullOrEmpty(iban?.ToString()))
                {
                    // IBAN might be required for international currencies
                    // This is configurable based on bank policies
                }
            }

            return success;
        }
        catch (Exception ex)
        {
            // If an exception occurs, log it and return false
            if (context?.MetaData != null)
            {
                context.MetaData["account_created_successfully_rule_error"] = new
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
