# OAuth2 Authentication Workflow

## Overview

This document describes the comprehensive OAuth2 authentication workflow implementation using the VNext Runtime platform. The workflow supports multiple OAuth2 grant types with advanced Multi-Factor Authentication (MFA) capabilities, including push notifications and OTP verification.

## Architecture

[![](https://mermaid.ink/img/pako:eNqVVctu00AU_ZXRILFKg5PacWOhojRp0jZtkjZpKiCocu3rxqpjR2Ob0jqR2LDjKdhQqBCIPWLH9_AD8AnMI44dkyKRxTiec8-Zc--dGUfY8EzAGj4l-niIerWBi-iv8nCAf3-6eoralTAYFvMSYk9wA9vQA9tz756QO-tHHjmzHO8cdQOdBAP8CK2srKMNTn33AlUdmxJQX3dsk5NohFAX4wYPr0bpuHtTgVUZNrkP_gTVqN7Pq_e_frxCDaLTwN7FGFAXHDAWRAWl5U3QJmNcP89YRnXddsDMmKhxE_UokZ45qHM5g3s7NgiYTEl3qKGGSPB1nGA1AXlZuuEJq8p8IaE01n3_3CPmBG1x_puvqDOb-gdLpyl4xL7kGRyzVk3QNue_veb5zUFUpeBSpYErnls81WbEaKgbGgb4flzvbY7tLsXE2Eg6sscNvPyGet4ZuKgBLpDFBjeSXoiJnf9h72TZzYTd4uwPn9l22KtXUHUIxtmc2cwyd1PM9EwqRIwtXoB2VIPHtgHoAE5tPwDa2bgK7VgJ3Uad0GdVGo89EkxQR_Tju5hueYFtpU8Jc5ltSXtuYl9spi-o3euwhG7oIBs73OJBJAQXWrTPoe4ySIwHqfqnZ1J16CYhvTijhWpkutRdXsget3IYpTlZT4d_uTlcLrbHxfoR3yp-vFeSpvQToSN26D8-yx762cpz1_3lC_nBhQOogizbcbRbULAUC9LI0Qwx1qBklNPI5gyxLMM0i2mkMUesVZDSyNaNyPaNSCdGVkGxlDSyv4DgHL3KbRNrAQkhh0dARjp7xRHjDDCtzggGWKN_TZ3QozNwp5Qz1t0HnjeKacQLT4dYs-idRt_CMb3AoWbr9CORhIBrAql6oRtgbY0rYC3CT7BWLCh5dbWsFNRioVAuyFIphy_otCTnFVmVpbKkqCUKTnP4kq8p5WVpraSWZLkoy4qqlgul6R_HxRkW?type=png)](https://mermaid.live/edit#pako:eNqVVctu00AU_ZXRILFKg5PacWOhojRp0jZtkjZpKiCocu3rxqpjR2Ob0jqR2LDjKdhQqBCIPWLH9_AD8AnMI44dkyKRxTiec8-Zc--dGUfY8EzAGj4l-niIerWBi-iv8nCAf3-6eoralTAYFvMSYk9wA9vQA9tz756QO-tHHjmzHO8cdQOdBAP8CK2srKMNTn33AlUdmxJQX3dsk5NohFAX4wYPr0bpuHtTgVUZNrkP_gTVqN7Pq_e_frxCDaLTwN7FGFAXHDAWRAWl5U3QJmNcP89YRnXddsDMmKhxE_UokZ45qHM5g3s7NgiYTEl3qKGGSPB1nGA1AXlZuuEJq8p8IaE01n3_3CPmBG1x_puvqDOb-gdLpyl4xL7kGRyzVk3QNue_veb5zUFUpeBSpYErnls81WbEaKgbGgb4flzvbY7tLsXE2Eg6sscNvPyGet4ZuKgBLpDFBjeSXoiJnf9h72TZzYTd4uwPn9l22KtXUHUIxtmc2cwyd1PM9EwqRIwtXoB2VIPHtgHoAE5tPwDa2bgK7VgJ3Uad0GdVGo89EkxQR_Tju5hueYFtpU8Jc5ltSXtuYl9spi-o3euwhG7oIBs73OJBJAQXWrTPoe4ySIwHqfqnZ1J16CYhvTijhWpkutRdXsget3IYpTlZT4d_uTlcLrbHxfoR3yp-vFeSpvQToSN26D8-yx762cpz1_3lC_nBhQOogizbcbRbULAUC9LI0Qwx1qBklNPI5gyxLMM0i2mkMUesVZDSyNaNyPaNSCdGVkGxlDSyv4DgHL3KbRNrAQkhh0dARjp7xRHjDDCtzggGWKN_TZ3QozNwp5Qz1t0HnjeKacQLT4dYs-idRt_CMb3AoWbr9CORhIBrAql6oRtgbY0rYC3CT7BWLCh5dbWsFNRioVAuyFIphy_otCTnFVmVpbKkqCUKTnP4kq8p5WVpraSWZLkoy4qqlgul6R_HxRkW)

The OAuth2 workflow is implemented as a state machine with the following key components:

### Main Workflow States

1. **Client Validation** - Validates OAuth2 client credentials
2. **Grant Type Selection** - Routes to appropriate authentication flow
3. **Authentication Flows** - Password, Authorization Code, Client Credentials
4. **MFA Check** - Determines MFA requirements based on device registration
5. **MFA Flows** - Push notification or OTP verification
6. **Device Registration** - Registers new devices after successful OTP MFA
7. **Token Generation** - Issues access and refresh tokens
8. **Final States** - Success or failure endpoints

### Supported Grant Types

- **Client Credentials Flow** - For service-to-service authentication
- **Password Grant Flow** - For trusted applications with user credentials
- **Authorization Code Flow** - For web applications with redirect-based authentication

### Multi-Factor Authentication (MFA)

- **Push Notification MFA** - For registered devices with push support
- **OTP MFA** - SMS/Email-based one-time passwords for unregistered devices
- **Device Registration** - Automatic device registration after successful OTP verification

## Workflow Components

### Tasks

The workflow uses the following HTTP tasks to interact with external services:

| Task | Endpoint | Purpose |
|------|----------|---------|
| `validate-client` | `/api/oauth2/client/validate` | Validates OAuth2 client credentials |
| `validate-user-credentials` | `/api/oauth2/user/authenticate` | Authenticates user credentials |
| `validate-authorization-code` | `/api/oauth2/code/validate` | Validates authorization codes |
| `check-device-registration` | `/api/oauth2/device/check` | Checks device registration status |
| `send-push-notification` | `/api/oauth2/push/send` | Sends push notifications for MFA |
| `check-push-response` | `/api/oauth2/push/check` | Checks push notification responses |
| `send-otp-notification` | `/api/oauth2/otp/send` | Sends OTP codes via SMS/email |
| `verify-otp-code` | `/api/oauth2/otp/verify` | Verifies OTP codes |
| `register-device` | `/api/oauth2/device/register` | Registers new devices |
| `generate-tokens` | `/api/oauth2/tokens/generate` | Generates access and refresh tokens |

### Subflows

The workflow includes specialized subflows for different authentication methods:

- **Password Subflow** (`password-subflow.json`) - Handles password-based authentication
- **Authorization Code Subflow** (`authorization-code-subflow.json`) - Handles authorization code validation
- **OTP MFA Subflow** (`otp-mfa-subflow.json`) - Manages OTP-based MFA with timeout handling
- **Push Notification MFA Subflow** (`push-notification-mfa-subflow.json`) - Manages push-based MFA with timeout handling

## Authentication Flow Details

### 1. Client Validation

Every authentication request begins with client validation:

```json
{
  "client_id": "your-client-id",
  "client_secret": "your-client-secret",
  "grant_type": "password|authorization_code|client_credentials",
  "scope": "requested-scopes"
}
```

**Success Response:**
```json
{
  "clientValidation": {
    "success": true,
    "validatedAt": "2024-01-01T12:00:00Z",
    "clientId": "your-client-id",
    "grantTypes": ["password", "authorization_code"],
    "redirectUris": ["https://example.com/callback"],
    "scopes": ["read", "write"],
    "isActive": true
  }
}
```

### 2. Grant Type Flows

#### Password Grant Flow

For trusted applications that can securely handle user credentials:

```json
{
  "username": "user@example.com",
  "password": "user-password"
}
```

**Success Response:**
```json
{
  "authentication": {
    "success": true,
    "userId": "12345",
    "username": "user@example.com",
    "email": "user@example.com",
    "roles": ["user", "premium"],
    "mfaRequired": true
  }
}
```

#### Authorization Code Flow

For web applications using redirect-based authentication:

```json
{
  "code": "authorization-code",
  "client_id": "your-client-id",
  "client_secret": "your-client-secret",
  "redirect_uri": "https://example.com/callback",
  "code_verifier": "pkce-code-verifier"
}
```

#### Client Credentials Flow

For service-to-service authentication (bypasses MFA):

```json
{
  "client_id": "service-client-id",
  "client_secret": "service-client-secret",
  "scope": "service-scope"
}
```

### 3. Multi-Factor Authentication

#### Device Registration Check

After successful primary authentication, the system checks device registration:

```json
{
  "user_id": "12345",
  "device_id": "device-fingerprint",
  "client_id": "your-client-id"
}
```

**Registered Device Response:**
```json
{
  "deviceRegistration": {
    "checkedAt": "2024-01-01T12:00:00Z",
    "isRegistered": true,
    "deviceId": "device-fingerprint",
    "deviceName": "iPhone 12",
    "deviceType": "mobile",
    "supportsPush": true,
    "registeredAt": "2024-01-01T10:00:00Z",
    "lastSeenAt": "2024-01-01T11:30:00Z"
  }
}
```

#### Push Notification MFA

For registered devices with push support:

1. **Send Push Notification:**
```json
{
  "userId": "12345",
  "deviceId": "device-fingerprint",
  "title": "Authentication Request",
  "message": "Approve this login attempt from your device",
  "actionType": "mfa_authentication",
  "expiresIn": 120,
  "metadata": {
    "requestId": "req-12345",
    "clientId": "your-client-id",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0..."
  }
}
```

2. **Check Push Response:**
```json
{
  "notificationId": "notif-12345",
  "userId": "12345",
  "deviceId": "device-fingerprint"
}
```

**Approved Response:**
```json
{
  "mfa": {
    "success": true,
    "pushApproved": true,
    "method": "push",
    "approvedAt": "2024-01-01T12:01:00Z"
  }
}
```

#### OTP MFA

For unregistered devices or when push is not available:

1. **Send OTP:**
```json
{
  "userId": "12345",
  "email": "user@example.com",
  "phone": "+1234567890",
  "method": "sms",
  "deviceId": "device-fingerprint",
  "language": "en-US"
}
```

**OTP Sent Response:**
```json
{
  "otp": {
    "sent": true,
    "otpId": "otp-12345",
    "expiresAt": "2024-01-01T12:05:00Z",
    "method": "sms",
    "attemptsRemaining": 3
  }
}
```

2. **Verify OTP:**
```json
{
  "otpId": "otp-12345",
  "otpCode": "123456",
  "userId": "12345",
  "deviceId": "device-fingerprint"
}
```

**OTP Verified Response:**
```json
{
  "mfa": {
    "success": true,
    "otpVerified": true,
    "method": "otp",
    "verifiedAt": "2024-01-01T12:02:00Z"
  }
}
```

### 4. Device Registration

After successful OTP MFA, unregistered devices are automatically registered:

```json
{
  "userId": "12345",
  "deviceId": "device-fingerprint",
  "deviceName": "Chrome Browser",
  "deviceType": "desktop",
  "supportsPush": false
}
```

**Registration Response:**
```json
{
  "deviceRegistration": {
    "success": true,
    "deviceId": "device-fingerprint",
    "isRegistered": true,
    "supportsPush": false
  }
}
```

### 5. Token Generation

After successful authentication and MFA, tokens are generated:

```json
{
  "client_id": "your-client-id",
  "client_secret": "your-client-secret",
  "grant_type": "password",
  "user_id": "12345",
  "username": "user@example.com",
  "email": "user@example.com",
  "scope": "read write",
  "user_roles": ["user", "premium"],
  "device_id": "device-fingerprint",
  "device_registered": true,
  "mfa_completed": true,
  "mfa_method": "push",
  "ip_address": "192.168.1.1",
  "user_agent": "Mozilla/5.0..."
}
```

**Token Response:**
```json
{
  "tokens": {
    "success": true,
    "generated_at": "2024-01-01T12:03:00Z",
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "token_type": "Bearer",
    "expires_in": 3600,
    "scope": "read write"
  }
}
```

## Error Handling

The workflow includes comprehensive error handling for various scenarios:

### Client Validation Errors

```json
{
  "clientValidation": {
    "success": false,
    "validatedAt": "2024-01-01T12:00:00Z",
    "error": "Client validation failed",
    "errorCode": "invalid_client",
    "errorDescription": "The client credentials provided are invalid",
    "statusCode": 401
  }
}
```

### Authentication Errors

```json
{
  "authentication": {
    "success": false,
    "error": "Invalid username or password",
    "errorCode": "invalid_credentials"
  }
}
```

### MFA Errors

```json
{
  "mfa": {
    "success": false,
    "error": "Invalid OTP code",
    "errorCode": "invalid_otp",
    "attemptsRemaining": 2
  }
}
```

### Token Generation Errors

```json
{
  "tokens": {
    "success": false,
    "generated_at": "2024-01-01T12:03:00Z",
    "error": "Token generation failed",
    "errorCode": "token_generation_error",
    "errorDescription": "Unable to generate access tokens",
    "statusCode": 500
  }
}
```

### Workflow Configuration

The workflow can be customized by modifying the JSON configuration files:

- **Main Workflow:** `oauth-authentication-workflow.json`
- **Subflows:** `*-subflow.json` files
- **Tasks:** Individual task JSON files in the `Tasks/` directory

## Conclusion

This OAuth2 authentication workflow provides a comprehensive, secure, and scalable solution for modern authentication needs. It supports multiple grant types, advanced MFA capabilities, and includes extensive monitoring and error handling features.

The modular design allows for easy customization and extension, while the robust security features ensure compliance with modern authentication standards and best practices.
