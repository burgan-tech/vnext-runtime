using System.Threading.Tasks;
using BBT.Workflow.Scripting;
using BBT.Workflow.Definitions;

/// <summary>
/// Create Bank Account Mapping - Creates the actual bank account in core banking system
/// This mapping creates the demand deposit account in the core banking system.
/// </summary>
public class CreateBankAccountMapping : IMapping
{
    public Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
    {
        try
        {
            var httpTask = task as HttpTask;
            if (httpTask == null)
            {
                throw new InvalidOperationException("Task must be an HttpTask for account creation");
            }

            // Get all required data from workflow context
            var userSession = context.Instance?.Data?.userSession;
            var accountType = context.Instance?.Data?.accountType;
            var policyValidation = context.Instance?.Data?.policyValidation;

            // Prepare the request body for account creation
            var requestBody = new
            {
                // Account information
                accountType = accountType?.accountType ?? "demand-deposit",
                accountName = context.Instance?.Data?.accountName,
                currency = context.Instance?.Data?.currency,
                branchCode = context.Instance?.Data?.branchCode,
                initialDeposit = context.Instance?.Data?.initialDeposit ?? 0,
                accountPurpose = context.Instance?.Data?.accountPurpose,
                
                // Customer information
                customerId = userSession?.userId,
                
                // Account limits from policy validation
                accountLimits = policyValidation?.accountLimits ?? new
                {
                    dailyTransactionLimit = 50000,
                    monthlyTransactionLimit = 500000,
                    minimumBalance = 0,
                    maximumBalance = 1000000
                },
                
                // Notification preferences
                notifications = context.Instance?.Data?.notifications ?? new
                {
                    smsNotifications = true,
                    emailNotifications = true,
                    pushNotifications = true
                },
                
                // Compliance and audit information
                complianceInfo = new
                {
                    validationId = policyValidation?.validationId,
                    policyVersion = policyValidation?.policyVersion,
                    termsAccepted = context.Instance?.Data?.termsAccepted,
                    privacyPolicyAccepted = context.Instance?.Data?.privacyPolicyAccepted,
                    ipAddress = userSession?.ipAddress,
                    userAgent = userSession?.userAgent
                },
                
                // Request metadata
                requestId = context.Headers?["x-request-id"] ?? Guid.NewGuid().ToString(),
                workflowInstanceId = context.Instance?.Id.ToString(),
                requestedAt = DateTime.UtcNow,
                
                // Additional context
                channel = "digital-banking",
                source = "account-opening-workflow"
            };

            httpTask.SetBody(requestBody);

            // Set Headers
            var headers = new Dictionary<string, string?>
            {
                ["Content-Type"] = "application/json",
                ["user_reference"] = userSession?.userId?.ToString(),
                ["X-Request-Id"] = requestBody.requestId,
                ["X-Instance-Id"] = context.Instance?.Id.ToString(),
                ["X-Account-Type"] = accountType?.accountType?.ToString(),
                ["X-Currency"] = context.Instance?.Data?.currency?.ToString(),
                ["X-Branch-Code"] = context.Instance?.Data?.branchCode?.ToString(),
                ["X-Validation-Id"] = policyValidation?.validationId?.ToString()
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
    /// Process the account creation result and merge it into the workflow instance
    /// </summary>
    public async Task<ScriptResponse> OutputHandler(ScriptContext context)
    {
        try
        {
            var statusCode = GetStatusCodeFromResponse(context);
            var responseData = GetResponseDataFromContext(context);

            // Successful account creation
            if (statusCode >= 200 && statusCode < 300)
            {
                var accountCreation = new
                {
                    success = true,
                    createdAt = DateTime.UtcNow,
                    
                    // Account details
                    accountId = responseData?.accountId ?? Guid.NewGuid().ToString(),
                    accountNumber = responseData?.accountNumber,
                    iban = responseData?.iban,
                    accountType = responseData?.accountType ?? "demand-deposit",
                    currency = responseData?.currency,
                    branchCode = responseData?.branchCode,
                    
                    // Account status
                    status = responseData?.status ?? "active",
                    isActive = responseData?.isActive ?? true,
                    
                    // Balance information
                    currentBalance = responseData?.currentBalance ?? 0,
                    availableBalance = responseData?.availableBalance ?? 0,
                    
                    // Account limits
                    limits = responseData?.limits ?? new
                    {
                        dailyTransactionLimit = 50000,
                        monthlyTransactionLimit = 500000,
                        minimumBalance = 0,
                        maximumBalance = 1000000
                    },
                    
                    // Cards and services
                    debitCardRequested = responseData?.debitCardRequested ?? false,
                    onlineBankingEnabled = responseData?.onlineBankingEnabled ?? true,
                    mobileBankingEnabled = responseData?.mobileBankingEnabled ?? true,
                    
                    // Metadata
                    creationReference = responseData?.creationReference ?? Guid.NewGuid().ToString(),
                    coreBankingId = responseData?.coreBankingId,
                    
                    // Additional services
                    services = responseData?.services ?? new[]
                    {
                        "online-banking",
                        "mobile-banking",
                        "sms-banking"
                    }
                };

                return new ScriptResponse
                {
                    Key = "account-creation-success",
                    Data = new
                    {
                        accountCreation = accountCreation,
                        // Include summary for success view
                        accountSummary = new
                        {
                            accountNumber = accountCreation.accountNumber,
                            iban = accountCreation.iban,
                            accountType = accountCreation.accountType,
                            currency = accountCreation.currency,
                            status = accountCreation.status,
                            createdAt = accountCreation.createdAt
                        }
                    },
                    Tags = new[] { "banking", "account-creation", "success", "account-opened", "core-banking" }
                };
            }
            // Failed account creation
            else
            {
                var errorInfo = ExtractErrorInformation(context, statusCode);
                
                var accountCreation = new
                {
                    success = false,
                    createdAt = DateTime.UtcNow,
                    
                    // Error details
                    error = errorInfo.message,
                    errorCode = errorInfo.code,
                    errorDescription = errorInfo.description,
                    statusCode = statusCode,
                    
                    // Failure analysis
                    failureReason = responseData?.failureReason ?? "unknown",
                    failureCategory = responseData?.failureCategory ?? "system_error",
                    
                    // Retry information
                    canRetry = responseData?.canRetry ?? true,
                    retryAfter = responseData?.retryAfter ?? 300, // 5 minutes
                    maxRetries = responseData?.maxRetries ?? 3,
                    
                    // Recommendations
                    recommendations = responseData?.recommendations ?? new[]
                    {
                        "Please check your account information and try again",
                        "If the problem persists, contact customer support"
                    },
                    
                    // Support information
                    supportReference = responseData?.supportReference ?? Guid.NewGuid().ToString(),
                    
                    // Metadata
                    requestId = context.Headers?["x-request-id"],
                    workflowInstanceId = context.Instance?.Id.ToString()
                };

                return new ScriptResponse
                {
                    Key = "account-creation-failure",
                    Data = new
                    {
                        accountCreation = accountCreation
                    },
                    Tags = new[] { "banking", "account-creation", "failure", "error", "core-banking-error" }
                };
            }
        }
        catch (Exception ex)
        {
            return new ScriptResponse
            {
                Key = "account-creation-output-error",
                Data = new
                {
                    accountCreation = new
                    {
                        success = false,
                        error = "Internal processing error",
                        errorCode = "processing_error",
                        errorDescription = ex.Message,
                        createdAt = DateTime.UtcNow,
                        canRetry = true,
                        supportReference = Guid.NewGuid().ToString()
                    }
                },
                Tags = new[] { "banking", "account-creation", "error", "exception", "processing-error" }
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
        
        string message = "Account creation failed";
        string code = "account_creation_error";
        string description = "Unable to create the bank account";

        if (errorData != null)
        {
            message = errorData.message ?? message;
            code = errorData.code ?? code;
            description = errorData.description ?? description;
        }
        else if (statusCode == 400)
        {
            code = "invalid_account_data";
            description = "Invalid account creation data provided";
        }
        else if (statusCode == 409)
        {
            code = "account_already_exists";
            description = "An account with similar details already exists";
        }
        else if (statusCode == 422)
        {
            code = "validation_error";
            description = "Account data validation failed";
        }
        else if (statusCode >= 500)
        {
            code = "core_banking_error";
            description = "Core banking system error during account creation";
        }

        return (message, code, description);
    }

    #endregion
}
