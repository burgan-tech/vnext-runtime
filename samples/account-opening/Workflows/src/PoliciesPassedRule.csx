using System.Threading.Tasks;
using BBT.Workflow.Scripting;

/// <summary>
/// Policies Passed Rule - Checks if policy validation was successful
/// This rule determines if the account opening request passed all policy validations.
/// </summary>
public class PoliciesPassedRule : IConditionMapping
{
    /// <summary>
    /// Check if the policy validation was successful
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

            // Policy validation data
            var policyValidation = context.Instance.Data.policyValidation;
            
            if (policyValidation == null)
            {
                return false;
            }

            // Check if the policy validation passed
            var passed = policyValidation.passed == true;
            
            // Additional validation checks
            if (passed)
            {
                // Ensure compliance checks are present and valid
                var complianceChecks = policyValidation.complianceChecks;
                if (complianceChecks != null)
                {
                    var kycCompliant = complianceChecks.kycCompliant == true;
                    var amlCompliant = complianceChecks.amlCompliant == true;
                    var sanctionsCheck = complianceChecks.sanctionsCheck == true;
                    
                    passed = passed && kycCompliant && amlCompliant && sanctionsCheck;
                }
                
                // Check validation score if available
                var validationScore = policyValidation.validationScore;
                if (validationScore != null)
                {
                    passed = passed && validationScore >= 70; // Minimum score threshold
                }
            }

            return passed;
        }
        catch (Exception ex)
        {
            // If an exception occurs, log it and return false
            if (context?.MetaData != null)
            {
                context.MetaData["policies_passed_rule_error"] = new
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
