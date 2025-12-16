# Schema Management

Schemas are JSON Schema-based definitions used for data validation and normalization in the workflow system. They ensure data integrity and consistency by checking whether request payloads conform to defined rules.

:::highlight green 💡
Schemas are defined in JSON format and automatically loaded by the system. Each schema is identified by a domain-specific key and supports version management.
:::

## Table of Contents

1. [Schema Definition](#schema-definition)
2. [Schema Properties](#schema-properties)
3. [Use Cases](#use-cases)
4. [Type Values](#type-values)
5. [JSON Schema Structure](#json-schema-structure)
6. [Usage Examples](#usage-examples)
7. [Best Practices](#best-practices)

## Schema Definition

A Schema is a domain entity that defines the structure of input and output data for workflow components. Schemas are versioned and can be referenced by workflows, transitions, and tasks.

### Basic Structure

```json
{
  "key": "account-confirmation",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "banking",
    "account-confirmation",
    "input-schema",
    "approval"
  ],
  "attributes": {
    "type": "workflow",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/banking/account-confirmation.json",
      "title": "Account Confirmation Schema",
      "description": "Schema for account opening confirmation",
      "type": "object",
      "required": [
        "confirmed",
        "termsAccepted"
      ],
      "properties": {
        "confirmed": {
          "type": "boolean",
          "description": "User confirmation status"
        },
        "termsAccepted": {
          "type": "boolean",
          "description": "Terms and conditions acceptance status"
        }
      }
    }
  }
}
```

## Schema Properties

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| `key` | `string` | Unique identifier for the schema |
| `version` | `string` | Version information (semantic versioning) |
| `domain` | `string` | Domain the schema belongs to |
| `flow` | `string` | Flow stream information (default: `sys-schemas`) |
| `flowVersion` | `string` | Flow version information |
| `tags` | `string[]` | Tags for categorization and searching |
| `attributes` | `object` | Schema content and metadata |

### Tags Property

Tags are used for categorization and searching of schemas:

```json
{
  "tags": [
    "banking",
    "account-confirmation",
    "input-schema",
    "approval"
  ]
}
```

**Tag Usage Recommendations:**
- Specify the domain area (e.g., `banking`, `payments`, `user-management`)
- Specify the schema type (e.g., `input-schema`, `output-schema`, `validation`)
- Specify the business process (e.g., `approval`, `registration`, `transfer`)
- Specify the component type (e.g., `workflow`, `task`, `view`)

### Attributes Property

The `attributes` object contains the schema content and metadata information:

| Property | Type | Description |
|----------|------|-------------|
| `type` | `string` | Which component type the schema is used for |
| `schema` | `object` | Actual JSON Schema definition |

## Use Cases

Schemas are used for data validation in the following scenarios:

### 1. Workflow Initialization

Validation of incoming data payload when starting a workflow:

```json
{
  "startTransition": {
    "key": "start",
    "target": "initial-state",
    "schema": {
      "ref": "Schemas/start-input-schema.json"
    }
  }
}
```

### 2. Transition Validation

Validation of data sent during a transition:

```json
{
  "key": "approve",
  "target": "approved",
  "triggerType": 0,
  "schema": {
    "ref": "Schemas/approval-schema.json"
  }
}
```

### 3. Task Input/Output Control

Validation of data sent to and returned from tasks:

```json
{
  "task": {
    "ref": "Tasks/validate-user.json"
  },
  "inputSchema": {
    "ref": "Schemas/user-input-schema.json"
  },
  "outputSchema": {
    "ref": "Schemas/user-output-schema.json"
  }
}
```

### 4. View Form Validation

Validation of form data coming from user interfaces.

## Type Values

The `attributes.type` property specifies which component type the schema is designed for:

| Type Value | Description | Use Case |
|------------|-------------|----------|
| `workflow` | Schemas used at workflow level | Workflow initialization, general data structures |
| `task` | Task input/output validation | HTTP tasks, script tasks |
| `function` | Schemas for platform functions | Function parameters and return values |
| `view` | View form validation | User form inputs |
| `schema` | Meta-schema definitions | Schema validation |
| `extension` | Extension data structures | Extension input/output data |
| `headers` | HTTP header validation | Header structures for API calls |

### Type Selection Guide

```
┌─────────────────────────────────────────────────────────────┐
│                    Type Selection Guide                      │
├─────────────────────────────────────────────────────────────┤
│ Workflow initialization data?         → type: "workflow"    │
│ Task input/output?                    → type: "task"        │
│ Platform function?                    → type: "function"    │
│ User form input?                      → type: "view"        │
│ Validating another schema?            → type: "schema"      │
│ Extension data?                       → type: "extension"   │
│ HTTP header structure?                → type: "headers"     │
└─────────────────────────────────────────────────────────────┘
```

## JSON Schema Structure

Schema definitions use the [JSON Schema Draft 2020-12](https://json-schema.org/draft/2020-12/schema) standard.

### Basic JSON Schema Elements

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.vnext.com/banking/example.json",
  "title": "Example Schema",
  "description": "This is an example schema definition",
  "type": "object",
  "required": ["field1", "field2"],
  "properties": {
    "field1": {
      "type": "string",
      "description": "Required text field",
      "minLength": 1,
      "maxLength": 100
    },
    "field2": {
      "type": "integer",
      "description": "Required number field",
      "minimum": 0,
      "maximum": 1000
    }
  }
}
```

### Supported JSON Schema Features

#### Type Validations

| Type | Description | Example |
|------|-------------|---------|
| `string` | Text values | `"type": "string"` |
| `number` | Decimal numbers | `"type": "number"` |
| `integer` | Whole numbers | `"type": "integer"` |
| `boolean` | True/False values | `"type": "boolean"` |
| `array` | Array values | `"type": "array"` |
| `object` | Object values | `"type": "object"` |
| `null` | Null value | `"type": "null"` |

#### String Validations

```json
{
  "type": "string",
  "minLength": 1,
  "maxLength": 255,
  "pattern": "^[A-Z][a-z]+$",
  "format": "email"
}
```

**Supported Format Values:**
- `email`: Email address
- `uri`: URI format
- `date`: ISO 8601 date (YYYY-MM-DD)
- `date-time`: ISO 8601 date-time
- `time`: ISO 8601 time
- `uuid`: UUID format
- `ipv4`: IPv4 address
- `ipv6`: IPv6 address

#### Number/Integer Validations

```json
{
  "type": "number",
  "minimum": 0,
  "maximum": 1000000,
  "exclusiveMinimum": 0,
  "exclusiveMaximum": 1000000,
  "multipleOf": 0.01
}
```

#### Array Validations

```json
{
  "type": "array",
  "items": {
    "type": "string"
  },
  "minItems": 1,
  "maxItems": 10,
  "uniqueItems": true
}
```

#### Object Validations

```json
{
  "type": "object",
  "properties": {
    "name": { "type": "string" },
    "age": { "type": "integer" }
  },
  "required": ["name"],
  "additionalProperties": false
}
```

### Conditional Validations

#### If-Then-Else

```json
{
  "type": "object",
  "properties": {
    "accountType": {
      "type": "string",
      "enum": ["individual", "corporate"]
    }
  },
  "if": {
    "properties": {
      "accountType": { "const": "corporate" }
    }
  },
  "then": {
    "required": ["taxNumber", "companyName"]
  },
  "else": {
    "required": ["identityNumber"]
  }
}
```

#### OneOf / AnyOf / AllOf

```json
{
  "oneOf": [
    {
      "type": "object",
      "properties": {
        "paymentType": { "const": "credit-card" },
        "cardNumber": { "type": "string" }
      },
      "required": ["paymentType", "cardNumber"]
    },
    {
      "type": "object",
      "properties": {
        "paymentType": { "const": "bank-transfer" },
        "iban": { "type": "string" }
      },
      "required": ["paymentType", "iban"]
    }
  ]
}
```

## Usage Examples

### Example 1: Account Opening Schema

```json
{
  "key": "account-opening-input",
  "version": "1.0.0",
  "domain": "banking",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "banking",
    "account-opening",
    "input-schema"
  ],
  "attributes": {
    "type": "workflow",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/banking/account-opening-input.json",
      "title": "Account Opening Input Schema",
      "description": "Required data for account opening application",
      "type": "object",
      "required": [
        "customerType",
        "accountType",
        "currency"
      ],
      "properties": {
        "customerType": {
          "type": "string",
          "enum": ["individual", "corporate"],
          "description": "Customer type"
        },
        "accountType": {
          "type": "string",
          "enum": ["checking", "savings", "investment"],
          "description": "Account type"
        },
        "currency": {
          "type": "string",
          "enum": ["TRY", "USD", "EUR", "GBP"],
          "description": "Currency"
        },
        "initialDeposit": {
          "type": "number",
          "minimum": 0,
          "description": "Initial deposit amount"
        }
      }
    }
  }
}
```

### Example 2: Approval Schema

```json
{
  "key": "approval-confirmation",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "core",
    "approval",
    "confirmation"
  ],
  "attributes": {
    "type": "task",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/core/approval-confirmation.json",
      "title": "Approval Confirmation Schema",
      "description": "Required data for approval process",
      "type": "object",
      "required": [
        "approved",
        "approverNote"
      ],
      "properties": {
        "approved": {
          "type": "boolean",
          "description": "Approval status"
        },
        "approverNote": {
          "type": "string",
          "minLength": 10,
          "maxLength": 500,
          "description": "Approver's note"
        },
        "approvedAt": {
          "type": "string",
          "format": "date-time",
          "description": "Approval timestamp"
        }
      }
    }
  }
}
```

### Example 3: Money Transfer Schema

```json
{
  "key": "money-transfer-input",
  "version": "1.0.0",
  "domain": "payments",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "payments",
    "transfer",
    "input-schema"
  ],
  "attributes": {
    "type": "workflow",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/payments/money-transfer-input.json",
      "title": "Money Transfer Input Schema",
      "description": "Required data for money transfer",
      "type": "object",
      "required": [
        "sourceAccount",
        "targetAccount",
        "amount",
        "currency"
      ],
      "properties": {
        "sourceAccount": {
          "type": "string",
          "pattern": "^[0-9]{16}$",
          "description": "Source account number"
        },
        "targetAccount": {
          "type": "string",
          "description": "Target account number or IBAN"
        },
        "amount": {
          "type": "number",
          "exclusiveMinimum": 0,
          "maximum": 1000000,
          "description": "Transfer amount"
        },
        "currency": {
          "type": "string",
          "enum": ["TRY", "USD", "EUR"],
          "description": "Currency"
        },
        "description": {
          "type": "string",
          "maxLength": 140,
          "description": "Transfer description"
        },
        "scheduled": {
          "type": "boolean",
          "default": false,
          "description": "Is this a scheduled transfer?"
        },
        "scheduledDate": {
          "type": "string",
          "format": "date",
          "description": "Scheduled transfer date"
        }
      },
      "if": {
        "properties": {
          "scheduled": { "const": true }
        }
      },
      "then": {
        "required": ["scheduledDate"]
      }
    }
  }
}
```

### Example 4: HTTP Header Schema

```json
{
  "key": "api-request-headers",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-schemas",
  "flowVersion": "1.0.0",
  "tags": [
    "core",
    "headers",
    "api"
  ],
  "attributes": {
    "type": "headers",
    "schema": {
      "$schema": "https://json-schema.org/draft/2020-12/schema",
      "$id": "https://schemas.vnext.com/core/api-request-headers.json",
      "title": "API Request Headers Schema",
      "description": "Required headers for API requests",
      "type": "object",
      "required": [
        "X-Request-Id",
        "X-Correlation-Id"
      ],
      "properties": {
        "X-Request-Id": {
          "type": "string",
          "format": "uuid",
          "description": "Unique request identifier"
        },
        "X-Correlation-Id": {
          "type": "string",
          "format": "uuid",
          "description": "Correlation identifier"
        },
        "X-Client-Id": {
          "type": "string",
          "description": "Client identifier"
        },
        "Accept-Language": {
          "type": "string",
          "pattern": "^[a-z]{2}-[A-Z]{2}$",
          "description": "Language preference (e.g., en-US)"
        }
      }
    }
  }
}
```

## Best Practices

### 1. Use Descriptive Metadata

Add meaningful `title` and `description` for each schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "User Registration Schema",
  "description": "Defines all required fields for new user registration"
}
```

### 2. Add Field Descriptions

Define `description` for each property:

```json
{
  "properties": {
    "email": {
      "type": "string",
      "format": "email",
      "description": "User's email address. Will be used for verification."
    }
  }
}
```

### 3. Use Appropriate Validation Rules

Add constraints that match business rules:

```json
{
  "amount": {
    "type": "number",
    "minimum": 1,
    "maximum": 100000,
    "multipleOf": 0.01,
    "description": "Transfer amount (between 1-100,000, with cent precision)"
  }
}
```

### 4. Prefer Enum Values

Use enum for fields with specific value sets:

```json
{
  "status": {
    "type": "string",
    "enum": ["pending", "approved", "rejected", "cancelled"],
    "description": "Transaction status"
  }
}
```

### 5. Define Required Fields Correctly

Only add truly required fields to the `required` list:

```json
{
  "required": ["email", "password"],
  "properties": {
    "email": { "type": "string" },
    "password": { "type": "string" },
    "nickname": { "type": "string" }
  }
}
```

### 6. Using additionalProperties

Use `additionalProperties: false` to prevent unexpected fields:

```json
{
  "type": "object",
  "properties": {
    "name": { "type": "string" }
  },
  "additionalProperties": false
}
```

:::warning Caution
When `additionalProperties: false` is used, all fields not defined in the schema will be rejected. This is important for data integrity but may affect backward compatibility.
:::

### 7. Follow Schema Versioning

- **Major** version: Breaking changes (adding required fields, removing fields)
- **Minor** version: Backward-compatible additions (adding optional fields)
- **Patch** version: Bug fixes (description updates)

```json
{
  "key": "user-registration",
  "version": "2.1.0"
}
```

### 8. Use Meaningful Tags

Follow a consistent tagging strategy to categorize schemas:

```json
{
  "tags": [
    "domain:banking",
    "type:input-schema",
    "workflow:account-opening",
    "version:v2"
  ]
}
```

## Common Errors

### 1. Schema Validation Error

```
Error: JSON schema validation failed for property 'email'
```

**Solution**: Ensure the submitted data conforms to the type and rules defined in the schema.

### 2. Missing Required Field

```
Error: Required property 'amount' is missing
```

**Solution**: Ensure all fields in the `required` list are present in the payload.

### 3. Invalid Enum Value

```
Error: Value 'invalid' is not one of the allowed values: ['pending', 'approved', 'rejected']
```

**Solution**: Use only values defined in the enum.

### 4. Format Error

```
Error: String 'not-an-email' does not match format 'email'
```

**Solution**: Send values that conform to format rules.

## Related Documentation

- [📄 Workflow Definition](./flow.md) - Workflow structure and components
- [📄 Transition Management](./transition.md) - Using schemas in transitions
- [📄 Task Management](./task.md) - Task input/output schemas
- [📄 View Management](./view.md) - Form validation schemas

This documentation provides a comprehensive guide for schema definition and usage. Developers can follow this guide to create schemas that ensure data integrity.

