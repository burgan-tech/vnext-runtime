using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Account Policies Validation Mapping - Validates account opening against bank policies
/// This mapping validates account opening request against compliance and risk policies.
/// </summary>
public class ValidateAccountPoliciesMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for policy validation");
            }

            // Get user session and account details from workflow context
            var userSession = context.Instance?.Data?.userSession;
            var accountType = context.Instance?.Data?.accountType;

            // Prepare the request body for policy validation
            var requestBody = new
            {
                // User information
                userId = userSession?.userId,
                
                // Account details
                accountType = accountType?.accountType ?? "demand-deposit",
                accountName = context.Instance?.Data?.accountName,
                currency = context.Instance?.Data?.currency,
                branchCode = context.Instance?.Data?.branchCode,
                initialDeposit = context.Instance?.Data?.initialDeposit ?? 0,
                accountPurpose = context.Instance?.Data?.accountPurpose,
                
                // Risk assessment data
                requestedAt = DateTime.UtcNow,
                ipAddress = userSession.ipAddress; //context.Headers?["x-forwarded-for"] ?? context.Headers?["x-real-ip"],
                userAgent = userSerssion.userAgent; //context.Headers?["user-agent"],
                deviceId = userSession.deviceId; //context.Headers?["x-device-id"],
                
                // Compliance checks
                termsAccepted = context.Instance?.Data?.termsAccepted,
                privacyPolicyAccepted = context.Instance?.Data?.privacyPolicyAccepted
            };

            httpTask.SetBody(requestBody);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["user_reference"] = userSession?.userId?.ToString(),
                ["X-Request-Id"] = context.Headers?["x-request-id"] ?? Guid.NewGuid().ToString(),
                ["X-Instance-Id"] = context.Instance?.Id.ToString()
            };

            httpTask.SetHeaders(headers);

            return Task.FromResult(new ScriptResponse());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ScriptResponse());
        }
    }

    /// <summary>
    /// Process the policy validation result and merge it into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

            // Successful validation
            if (statusCode >= 200 && statusCode < 300)
            {
                var policyValidation = new
                {
                    passed = true,
                    validatedAt = DateTime.UtcNow,
                    validationId = responseData?.validationId ?? Guid.NewGuid().ToString(),
                    
                    // Policy check results
                    complianceChecks = responseData?.complianceChecks ?? new
                    {
                        kycCompliant = true,
                        amlCompliant = true,
                        sanctionsCheck = true,
                        riskAssessment = "low"
                    },
                    
                    // Account limits and restrictions
                    accountLimits = responseData?.accountLimits ?? new
                    {
                        dailyTransactionLimit = 50000,
                        monthlyTransactionLimit = 500000,
                        minimumBalance = 0,
                        maximumBalance = 1000000
                    },
                    
                    // Additional requirements
                    additionalRequirements = responseData?.additionalRequirements ?? new string[] { },
                    
                    // Approval details
                    approvedBy = responseData?.approvedBy ?? "system",
                    approvalLevel = responseData?.approvalLevel ?? "automated",
                    
                    // Metadata
                    policyVersion = responseData?.policyVersion ?? "1.0.0",
                    validationScore = responseData?.validationScore ?? 100
                };

                return new ScriptResponse
                {
                    Key = "policy-validation-success",
                    Data = new
                    {
                        policyValidation = policyValidation
                    },
                    Tags = new[] { "banking", "policy-validation", "compliance", "success", "approved" }
                };
            }
            // Failed validation
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                
                var policyValidation = new
                {
                    passed = false,
                    validatedAt = DateTime.UtcNow,
                    validationId = responseData?.validationId ?? Guid.NewGuid().ToString(),
                    
                    // Failure details
                    error = errorInfo.message,
                    errorCode = errorInfo.code,
                    errorDescription = errorInfo.description,
                    statusCode = statusCode,
                    
                    // Failed checks
                    failedChecks = responseData?.failedChecks ?? new[] { "unknown" },
                    
                    // Recommendations
                    recommendations = responseData?.recommendations ?? new[] 
                    { 
                        "Please review your account information and try again" 
                    },
                    
                    // Retry information
                    canRetry = responseData?.canRetry ?? true,
                    retryAfter = responseData?.retryAfter ?? 0
                };

                return new ScriptResponse
                {
                    Key = "policy-validation-failure",
                    Data = new
                    {
                        policyValidation = policyValidation
                    },
                    Tags = new[] { "banking", "policy-validation", "compliance", "failure", "rejected" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "policy-validation-output-error",
                Data = new
                {
                    policyValidation = new
                    {
                        passed = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message,
                        validatedAt = DateTime.UtcNow,
                        canRetry = true
                    }
                },
                Tags = new[] { "banking", "policy-validation", "error", "exception" }
            };
        }
    }

    #region Helper Methods

    private static int GetStatusCodeFromResponse(ScriptContext context)
    {
        if (context.Body?.statusCode != null)
            return (int)context.Body.statusCode;

        return 200;
    }

    private static dynamic? GetResponseDataFromContext(ScriptContext context)
    {
        if (context.Body?.data != null)
            return context.Body.data;

        if (context.Body != null && context.Body.statusCode == null)
            return context.Body;

        return null;
    }

    private static (string message, string code, string description) ExtractErrorInformation(ScriptContext context, int statusCode)
    {
        var errorData = context.Body?.data?.error ?? null;
        
        string message = "Policy validation failed";
        string code = "policy_violation";
        string description = "Account opening request does not meet policy requirements";

        if (errorData != null)
        {
            message = errorData.message ?? message;
            code = errorData.code ?? code;
            description = errorData.description ?? description;
        }
        else if (statusCode == 400)
        {
            code = "invalid_request";
            description = "Invalid account opening request";
        }
        else if (statusCode == 403)
        {
            code = "compliance_violation";
            description = "Account opening violates compliance policies";
        }
        else if (statusCode == 409)
        {
            code = "policy_conflict";
            description = "Account opening conflicts with existing policies";
        }
        else if (statusCode >= 500)
        {
            code = "server_error";
            description = "Internal server error during policy validation";
        }

        return (message, code, description);
    }

    #endregion
}
