using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Account Creation Failed Rule - Checks if account creation failed
/// This rule determines if the bank account creation failed in the core banking system.
/// </summary>
public class AccountCreationFailedRule : IConditionMapping
{
    /// <summary>
    /// Check if the account creation failed
    /// </summary>
    public async Task<bool> Handler(ScriptContext context)
    {
        try
        {
            // Null check
            if (context?.Instance?.Data == null)
            {
                return true; // If no data, consider it failed
            }

            // Account creation data
            var accountCreation = context.Instance.Data.accountCreation;
            
            if (accountCreation == null)
            {
                return true; // If no account creation data, consider it failed
            }

            // Check if the account creation failed
            var failed = accountCreation.success == false;
            
            // Additional failure checks
            if (!failed)
            {
                // Check for missing essential data even if marked as success
                var accountId = accountCreation.accountId;
                var accountNumber = accountCreation.accountNumber;
                var status = accountCreation.status;
                
                // Missing essential account details indicates failure
                if (string.IsNullOrEmpty(accountId?.ToString()) || 
                    string.IsNullOrEmpty(accountNumber?.ToString()))
                {
                    failed = true;
                }
                
                // Non-active status indicates failure
                if (status?.ToString() != "active")
                {
                    failed = true;
                }
                
                // Check if account is marked as inactive
                var isActive = accountCreation.isActive;
                if (isActive == false)
                {
                    failed = true;
                }
                
                // Check for specific error indicators
                var errorCode = accountCreation.errorCode;
                if (!string.IsNullOrEmpty(errorCode?.ToString()))
                {
                    var failureCodes = new[] 
                    { 
                        "account_creation_error", 
                        "core_banking_error", 
                        "invalid_account_data",
                        "account_already_exists",
                        "validation_error",
                        "system_error",
                        "processing_error"
                    };
                    
                    failed = failed || failureCodes.Contains(errorCode.ToString());
                }
                
                // Check failure category
                var failureCategory = accountCreation.failureCategory;
                if (!string.IsNullOrEmpty(failureCategory?.ToString()))
                {
                    var criticalCategories = new[] 
                    { 
                        "system_error", 
                        "validation_error", 
                        "business_rule_violation",
                        "core_banking_failure"
                    };
                    
                    failed = failed || criticalCategories.Contains(failureCategory.ToString());
                }
            }

            return failed;
        }
        catch (Exception ex)
        {
            // If an exception occurs, log it and return true (failed)
            if (context?.MetaData != null)
            {
                context.MetaData["account_creation_failed_rule_error"] = new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    timestamp = DateTime.UtcNow
                };
            }

            return true; // Exception means failure
        }
    }
}
