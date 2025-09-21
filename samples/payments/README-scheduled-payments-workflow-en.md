# Scheduled Payments Workflow

## Overview

This document describes the comprehensive scheduled payments workflow implementation using the VNext Runtime platform. The workflow is designed to manage recurring payments with automatic retry mechanisms, notification systems, and advanced payment scheduling capabilities.

## Architecture

![Payment Architecture](https://kroki.io/mermaid/svg/eNqNVVtP2zAUfu-vsIqYiuRO0IoBfUDaCkx7YKrIpj1EfTCO3VqEpIqdQaX-eE58i5206_pQ2yff-c7Vx6uKbNbo17cBgp-sn1f6PHwkokB_yuqF5-UbGqOErllW5yxDC7J9ZYWSQ63R_BpsokilRqlelmdoPL5F87LgYpVavD3WFVGiLJZe2Yi1wleqxF_mFcyxRZqzod7SnM3XjL609I0IaVlA7nGBgX2ED6IQEiL0dE6w1_wdI7GrTtCinWSfXcetvzXJuy-yUQp_yzONYUWm105JkvpZFyOBcjjDi6qkTAa1sKCwHA4kipWtjBUswBAIfRT23AYR40LV-3dG66aQqRU4j3rKHhmqJzVtltSu_6f0QAT0X2qWnooRh_gnpqptqv97aC3dG1AXaV3UWJvcTrn-6cScFJTljd9-13PGf-kYOdIMG5d56IefpRJcUH23omawoKgfQrDtiO9M_ZasSmFFzQb9KHjZOmo_G__AI80A5YNtxBZUsoUZrcfEwGFzELSo5dqgml0Ae0xcamw8-0oAKn1UP4enpxCcYisziNCiFDDMBuEN_zy-3Q3nJM-lKwYaJWfDXXS5BsFtAztWC_qqrgqJVAnw4NoPutMotuFKOVpYM2HZBnEpDxhryaNQ74gi6AF8HMQzbDf0swsQx2IL8U9M1rnaE15IrrvlE-rbOBZYoxx2FAKkquXhABO1zWE46SPNiZR3jKNXGKr-8eIiz2cnbMonPMNSVeULm51c3Fx9ySb2OH4TmVrPJpv3mEba6hsGfsGv2Y1nmF5fsyk9zuBqa0k4n7JzT8Ivr-j5-RESEbSrU7yhk6vnjuJ08-4EGZFrUlVkO0OX6LJNmOa0by42BcNtYrF7mbB_vsJMBgzxy4C7QxTHwxNH4xGHMxj3ZqBNemDMTh_cTgsMEwHrC98m-AMbOM18)


### Scheduled Payments Main Flow
![Ana Akış](https://kroki.io/mermaid/svg/eNp1VUtrGzEQvvdXCEogBjltUgyhhJRgN9BDi_Emh2J8ULSjWFTeNbvaFGP3v1erx2pWVnyQrdHMN49vZixU_ZdvWaPJ0-IDMZ9Cm8vl2n6RJTvsoNKkAN3tNxMynd6TeV0J-boOT-7aNUzLurp7aT7dF-wNSPZ5Yz3Yw8kRYNFxDm17HBsOgOW3f3m7RyYVlMfzMNyDN0O23lEPcfoN7Yk8cC3fYEjIXS2E_cl0zKbgWyg7BRuE5_xEOCeNXh2ejfgnqzqmFsA87tEJrK8o9SEju2VT9yHP691egTEL4Xi5tQ-P59YPSg2W5ndIJmtmjzTMmJuXxWIFQZLCOwVL8kA1O3AF8y3wP7GvehGxMgv-q9ZSSG75JUX3sndYDhilGEEfZSVb436ADALHbcO3EjXqCnjdlKhDQ2qIuVWWuVXKXMbyeV8mVk7yvsUCLGGjDskTtcoQ5ehHSs5dtkuxv6jwBM1OVsYm1i-K8CAP1IWh1LLqwIqP4Raq7Fquf4rjHNWz4dsjMGddfK_K68u1Ocl0eNhMrF6MMGjeBE0U_CQCX1ygXiI_Kg2vboWMhujKxIXUilPoZCMUZoGucwPp3zZnVUrQlieCezuFxG8Bd9T7Qx76oGTlKOWKte0CBGn7Nd4XQEilvn6EazETMFYJcTgN8QVmYjbWKIHLth87ryKM0ufET8zHe7oVM7iNAVpN9-dCewL742YID6m4xqSu9DQMBo0FpIFzitj2zs9w_LKneFXTdL_RZC9RtE5oOmMUzxPFs0PHzRyqhmIadw3N0I4K-R9iB5AK)

### Payment Process SubFlow

![Sub Flow](https://kroki.io/mermaid/svg/eNp9VG1r2zAQ_r5fIRiFBhS2dgt0Y3SUpoPBNkKTMUbIB9c-paKuPCR7aXD33yef3k5esnxwdKfneU53Op2om115X-iWreYvmP0tdFOCMcvW-k7X-McWxf4RVBv2pNpuJmw6vWQLUJW11hHg7A0qOTnnQfSyvIeqq6Gad9BH48OdfnUZBOzOxz__EL8Wqitqi_nW7HpnBBazLs_AD40xcJ9X8hH0czi6j7P2ZsjMqeU-tirMg8uExkfRn2DGkiTlzO_q5NbLrhx2ehrV-0LWR7k3Wjc6Y6KH5p5HSQf1DnuXLcSr8k53hmFnQzVQOyl8KqQtaS7gfGM-fggcU7jq2uYWWr3vhxXDJb12n8SY5sruiG7NvhvQyFxpud2CNk7sqMDTsC3BWP4T82vk30IxdMoR4o9GPwj7MuYW1crf1l1ZBaniBmqQXXoNMdtUPzTz8qUifFalhuRj102nWtC09Y7KeUxIM0GuC1VCbZOKAaPHCR9I8QA7ZZViYoXQ_NJsZdmnTND2pUgAlL2qd8X-P4-G9qhrewe8UdXZ6dp-2TRgNhNkxDOO4OcBnhKepDgnJ2zZ7ms7WdAs68KYOQhmhkE38ISs6_cv4UzMBOSQX34-OIR4AzMxyxEVlNLIRgWIsKDXozj-cfowF2IGFzlCYC9GCbiD0Tk0NkMM8a58m9JDUDbDOSkkWZ_HjCnPzVyeXxInV-9rQDh04HI6KHk-jjidLDy-EU4anKdG5gfak5OWCpWmB6Ed5MtMtskT56l1XLH_AtJPXbU)

### Notification Sub Process

![Sub Process](https://kroki.io/mermaid/svg/eNp9U1FLwzAQfvdXHMhgww6VuQdFJuJEfFCG3R6k7KFrLy6YtaPJHGPzv5tc2zShxT6kvbvvvvvucmUi3yfruFAwn56BfsLdKlTa7kf0gvdcccaTWPE8g1mRJyjlcgDD4QReUC0kFq8ZyyP9DcYAYxUbgt-visuJF4B5LL-XVIcOh4EYa-MDVcHxB9OjTSUy63_4bUhaSYbq9InyBCFmqduAjIzH60kScfgWwgXMdnLtqGtlk0YNNV2UTLP4sMFMEYc3KY1a_kNiKnWzmIgn0BVUliYGj1OzqGNHV9pdTaou2J3blGiFaJiPYh8f9Dyf8s1WoMKoayuoZI1wVNeucnq71XOW9iN9wNBfLps6aHJ7PQjVQfDsi8xExFJOkYE0u2k4GBfi7hyv2ZihD9mWqioEG-GYjX2EMvOowozdJjd-OMWESyPMQkZ41UgjnP1dgrIxK8yBOEsetLYhsNOp9Lrc5XUH9u6MYCfeWvygfXt1E3-AJkyd)


The scheduled payments workflow is implemented as a state machine with the following key components:

### Main Workflow States

1. **Payment Configuration** - Saves payment schedule settings
2. **Active State** - Initiates payment processing via Subflow
3. **Deactive State** - Temporarily pauses the payment schedule
4. **Payment Cycle Check** - Sends notifications via Subprocess
5. **Finished** - Payment schedule completed successfully
6. **Terminated** - Payment schedule manually canceled

### Supported Features

- **Scheduled Payments** - Daily, weekly, monthly, quarterly, yearly
- **Automatic Retry** - Configurable retry mechanism for failed payments
- **Notification System** - SMS and Push notifications
- **Cyclic Processing** - Timer-based scheduled triggers
- **Manual Intervention** - User-driven operations

## Workflow Components

### Tasks

The workflow uses the following HTTP tasks to interact with external services:

| Task | Endpoint | Purpose |
|------|----------|---------|
| `save-payment-configuration` | `/api/payments/schedules` | Saves payment schedule configuration |
| `activate-payment-schedule` | `/api/payments/schedules/activate` | Activates payment schedule |
| `deactivate-payment-schedule` | `/api/payments/schedules/deactivate` | Deactivates payment schedule |
| `process-payment` | `/api/payments/process` | Processes payment transaction |
| `increment-retry-counter` | `/api/payments/retry-counter` | Increments retry counter |
| `get-user-info` | `/api/users/{userId}` | Retrieves user information |
| `send-payment-notification-sms` | `/api/payments/notify/sms` | Sends SMS notification |
| `send-payment-push-notification` | `/api/payments/notify/push` | Sends push notification |
| `archive-payment-record` | `/api/payments/schedules/archive` | Archives payment record |

### Subflows and Subprocesses

The workflow includes specialized sub-components for different payment processes:

- **Payment Process Subflow** (`payment-process.json`) - Manages main payment processing logic
- **Payment Notification Subprocess** (`payment-notification-subflow.json`) - Manages post-payment notification delivery

## Payment Flow Details

### 1. Payment Configuration

Every scheduled payment begins with configuration setup:

```json
{
  "userId": "12345",
  "amount": 100.00,
  "currency": "USD",
  "frequency": "monthly",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "paymentMethodId": "pm_12345",
  "description": "Monthly subscription payment",
  "recipientId": "recipient_789",
  "isAutoRetry": true,
  "maxRetries": 3
}
```

**Success Response:**
```json
{
  "paymentSchedule": {
    "success": true,
    "scheduleId": "sched_12345",
    "status": "inactive",
    "nextPaymentDate": "2024-01-01T00:00:00Z",
    "maxPayments": 12,
    "createdAt": "2024-01-01T12:00:00Z",
    "completedPayments": 0,
    "lastPaymentAt": "2024-01-01T12:00:00Z",
    "isActive": false,
    "activatedAt": "2024-01-01T12:00:00Z",
    "archived": false,
    "finishedAt": "2024-01-01T12:00:00Z"
  }
}
```

### 2. Payment Processing (Subflow)

The payment processing subflow includes the following states:

#### Payment Pending
- **Scheduled Trigger** - Timer-based payment due date checking
- **Manual Trigger** - "Pay Now" functionality

#### Payment Processing

```json
{
  "scheduleId": "sched_12345",
  "userId": "12345",
  "amount": 100.00,
  "currency": "USD",
  "processedAt": "2024-01-01T12:00:00Z"
}
```

**Successful Payment Response:**
```json
{
  "paymentResult": {
    "status": "success",
    "success": true,
    "transactionId": "txn_67890",
    "amount": 100.00,
    "currency": "USD",
    "processedAt": "2024-01-01T12:01:00Z",
    "paymentMethod": "credit_card",
    "fees": 2.50
  }
}
```

**Failed Payment Response:**
```json
{
  "paymentResult": {
    "status": "failed",
    "success": false,
    "error": "Insufficient funds",
    "errorCode": "insufficient_funds",
    "errorDescription": "The payment method has insufficient funds",
    "statusCode": 402,
    "failedAt": "2024-01-01T12:01:00Z",
    "retryCount": 1
  }
}
```

#### Retry Mechanism

Automatic retry for failed payments:

1. **Auto Retry Check:**
   - `isAutoRetry` must be enabled
   - Current retry count must be less than `maxRetries`

2. **Increment Retry Counter:**
```json
{
  "scheduleId": "sched_12345",
  "retryCount": 2,
  "retryAt": "2024-01-01T12:02:00Z"
}
```

3. **Maximum Retries Control:**
   - Payment is canceled when maximum retries reached
   - Subflow is terminated when main workflow is deactivated

### 3. Notification System (Subprocess)

After payment processing completion, notification subprocess executes:

#### Get User Information

```json
{
  "userId": "12345"
}
```

**User Information Response:**
```json
{
  "user": {
    "userId": "12345",
    "phoneNumber": "+1234567890",
    "registeredDevices": [
      {
        "deviceId": "device_123",
        "deviceType": "mobile",
        "supportsPush": true
      }
    ],
    "language": "en-US"
  }
}
```

#### SMS Notification

**Successful Payment SMS:**
```
Payment successful! Amount: 100.00 USD. Reference: txn_67890
```

**Failed Payment SMS:**
```
Payment failed. Reason: Insufficient funds. Please check your payment details.
```

#### Push Notification

```json
{
  "title": "Payment Successful",
  "body": "Your payment of 100.00 USD has been processed successfully.",
  "userId": "12345",
  "devices": ["device_123"],
  "pushType": "payment-notification",
  "data": {
    "paymentId": "txn_67890",
    "success": true
  }
}
```

### 4. Scheduling and Cycle Management

#### Timer Configuration

```csharp
// Daily payment
TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(1))

// Weekly payment  
TimerSchedule.FromDateTime(DateTime.UtcNow.AddDays(7))

// Monthly payment
TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(1))

// Quarterly payment
TimerSchedule.FromDateTime(DateTime.UtcNow.AddMonths(3))

// Yearly payment
TimerSchedule.FromDateTime(DateTime.UtcNow.AddYears(1))
```

#### Cycle Control

After payment completion:

1. **More Payments Check:**
   - Has maximum payment count been reached?
   - Has end date been passed?
   - Is the schedule still active?

2. **Cycle Continuation:**
   - If conditions are met, timer is set for next payment
   - Notification subprocess executes
   - Main flow returns to "Active" state

3. **Cycle Completion:**
   - If conditions are not met, moves to "Finished" state
   - Payment records are archived

## Error Handling

### Configuration Errors

```json
{
  "paymentSchedule": {
    "success": false,
    "error": "Failed to save payment configuration",
    "errorCode": "config_save_failed",
    "statusCode": 400
  }
}
```

### Payment Processing Errors

```json
{
  "paymentResult": {
    "success": false,
    "error": "Payment processing failed",
    "errorCode": "payment_processing_error",
    "errorDescription": "Unable to process payment",
    "statusCode": 500
  }
}
```

### Notification Errors

```json
{
  "smsResult": {
    "smsSent": false,
    "error": "Failed to send SMS notification",
    "errorCode": "sms_send_failed",
    "smsStatus": "failed"
  },
  "pushResult": {
    "pushSent": false,
    "error": "Failed to send push notification",
    "errorCode": "push_send_failed"
  }
}
```

## State Machine Transitions

### Automatic Transitions

- **payment-config-saved** → `payment-active`: Configuration saved successfully
- **payment-process-complete** → `payment-cycle-check`: Payment successful and more payments remaining
- **payments-all-complete** → `payment-finished`: All payments completed
- **payment-success** → `payment-success-state`: Payment successful
- **payment-error** → `payment-failed-state`: Payment failed
- **auto-retry-payment** → `payment-retry`: Automatic retry
- **max-retries-reached** → `payment-cancelled`: Maximum retry count reached

### Manual Transitions

- **manual-deactivate-payment** → `payment-deactive`: Deactivate payment
- **manual-reactivate-payment** → `payment-active`: Reactivate payment
- **manual-update-payment** → `payment-configuration`: Update payment settings
- **manual-delete-payment** → `payment-terminated`: Permanently delete payment
- **manual-pay-now** → `process-payment`: Immediate payment
- **manual-user-triggers-retry** → `payment-retry`: User triggers retry

### Scheduled Transitions

- **scheduled-payment-due** → `process-payment`: Payment due date reached (Timer)

## Security and Best Practices

### Data Security

- Payment information is stored encrypted
- API calls are made over HTTPS
- Sensitive information is not visible in logs

### Error Tolerance

- Retry mechanisms
- Graceful degradation (payment continues even if notification fails)
- Comprehensive error logging

### Performance

- Asynchronous task execution
- Optimal timer scheduling
- Efficient database queries

## Examples and Use Cases

### Subscription Payments

Monthly subscription service with automatic renewal:

```json
{
  "userId": "user_123",
  "amount": 29.99,
  "currency": "USD",
  "frequency": "monthly",
  "description": "Premium Subscription",
  "isAutoRetry": true,
  "maxRetries": 3
}
```

### Loan Payments

Weekly loan installments with specific end date:

```json
{
  "userId": "borrower_456",
  "amount": 250.00,
  "currency": "USD",
  "frequency": "weekly",
  "maxPayments": 52,
  "endDate": "2025-01-01T00:00:00Z",
  "description": "Personal Loan Payment"
}
```

### Utility Bills

Quarterly utility payments with notification preferences:

```json
{
  "userId": "customer_789",
  "amount": 150.00,
  "currency": "USD",
  "frequency": "quarterly",
  "description": "Electricity Bill",
  "notifications": {
    "sms": true,
    "push": true,
    "beforePayment": 3
  }
}
```

## Workflow Configuration

The workflow can be customized by modifying the JSON configuration files:

- **Main Workflow:** `scheduled-payments-workflow.json`
- **Subflows:** `payment-process.json`, `payment-notification-subflow.json`
- **Tasks:** Individual task JSON files in the `Tasks/` directory

### Key Configuration Parameters

- **Timer Settings:** Adjust payment due date calculations
- **Retry Policies:** Configure maximum retry attempts and intervals
- **Notification Templates:** Customize SMS and push message formats
- **Error Handling:** Define error codes and fallback behaviors

## Conclusion

This scheduled payments workflow provides a comprehensive, secure, and scalable solution for modern payment automation needs. With subflow and subprocess examples, timer-based scheduling, and advanced error handling features, it offers a robust payment automation platform.

The modular design allows for easy customization and extension, while robust security features ensure compliance with modern payment standards and best practices.
