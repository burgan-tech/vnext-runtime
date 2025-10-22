using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Policies Failed Rule - Checks if policy validation failed
/// This rule determines if the account opening request failed policy validations.
/// </summary>
public class PoliciesFailedRule : IConditionMapping
{
    /// <summary>
    /// Check if the policy validation failed
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

            // Policy validation data
            var policyValidation = context.Instance.Data.policyValidation;
            
            if (policyValidation == null)
            {
                return true; // If no policy validation data, consider it failed
            }

            // Check if the policy validation failed
            var failed = policyValidation.passed == false;
            
            // Additional failure checks
            if (!failed)
            {
                // Check compliance failures
                var complianceChecks = policyValidation.complianceChecks;
                if (complianceChecks != null)
                {
                    var kycFailed = complianceChecks.kycCompliant == false;
                    var amlFailed = complianceChecks.amlCompliant == false;
                    var sanctionsFailed = complianceChecks.sanctionsCheck == false;
                    
                    failed = kycFailed || amlFailed || sanctionsFailed;
                }
                
                // Check validation score if available
                var validationScore = policyValidation.validationScore;
                if (validationScore != null)
                {
                    failed = failed || validationScore < 70; // Below minimum threshold
                }
                
                // Check for specific error codes that indicate failure
                var errorCode = policyValidation.errorCode;
                if (!string.IsNullOrEmpty(errorCode?.ToString()))
                {
                    var failureCodes = new[] 
                    { 
                        "policy_violation", 
                        "compliance_violation", 
                        "risk_assessment_failed",
                        "kyc_failed",
                        "aml_failed",
                        "sanctions_check_failed"
                    };
                    
                    failed = failed || failureCodes.Contains(errorCode.ToString());
                }
            }

            return failed;
        }
        catch (Exception ex)
        {
            // If an exception occurs, log it and return true (failed)
            if (context?.MetaData != null)
            {
                context.MetaData["policies_failed_rule_error"] = new
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
