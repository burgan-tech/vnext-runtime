# Instance Filtering Guide

## Overview

The vNext workflow system provides powerful filtering capabilities for querying instances. You can filter on both **Instance table columns** and **JSON data fields** using either legacy format or GraphQL-style JSON format.

## Supported Routes

### 1. Function/Data Route

```http
GET /{domain}/workflows/{workflow}/functions/data?filter={...}
```

### 2. Workflow Instances Route

```http
GET /{domain}/workflows/{workflow}/instances?filter={...}
```

Both routes support the same filtering capabilities via the `filter` query parameter.

---

## Filter Formats

### Legacy Format

Simple key-value format: `field=operator:value`

### GraphQL Format (Recommended)

JSON-based format with logical operator support: `{"field":{"operator":"value"}}`

---

## Filterable Fields

### Instance Table Columns

Direct database columns:

| Column | Type | Description | Supported Operators |
|--------|------|-------------|---------------------|
| `key` | string | Instance key | eq, ne, like, startswith, endswith, in, nin |
| `flow` | string | Workflow name | eq, ne, like, startswith, endswith, in, nin |
| `status` | string | Instance status | eq, ne, in, nin |
| `currentState` (or `state`) | string | Current state | eq, ne, like, startswith, endswith, in, nin |
| `createdAt` | DateTime | Creation time | eq, ne, gt, ge, lt, le, between |
| `modifiedAt` | DateTime | Modification time | eq, ne, gt, ge, lt, le, between |
| `completedAt` | DateTime | Completion time | eq, ne, gt, ge, lt, le, between |
| `isTransient` | boolean | Transient flag | eq, ne |

### JSON Data Fields (attributes)

Any field stored in the instance's JSON data can be filtered using the `attributes` prefix.

---

## Supported Operators

| Operator | Description | Example Value |
|----------|-------------|---------------|
| `eq` | Equals | `"1111"` |
| `ne` | Not equals | `"test"` |
| `gt` | Greater than | `"100"` |
| `ge` | Greater than or equal | `"100"` |
| `lt` | Less than | `"100"` |
| `le` | Less than or equal | `"100"` |
| `between` | Between (inclusive) | `["2024-01-01", "2024-12-31"]` |
| `like` | Contains (case insensitive) | `"workflow"` |
| `startswith` | Starts with | `"payment"` |
| `endswith` | Ends with | `"flow"` |
| `in` | In list | `["Active", "Busy"]` |
| `nin` | Not in list | `["Completed", "Faulted"]` |
| `isnull` | Null or not null | `true` or `false` |

---

## Status Values

The `status` field accepts both code and name:

| Status Name | Code | Description |
|-------------|------|-------------|
| `Active` | `A` | Instance is active |
| `Busy` | `B` | Instance is processing |
| `Completed` | `C` | Instance completed successfully |
| `Faulted` | `F` | Instance encountered an error |
| `Passive` | `P` | Instance is passive |

---

## GraphQL Format Examples

### 1. Simple Instance Column Filter

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"key":{"eq":"payment-12345"}}
```

### 2. Multiple Instance Column Filters (AND Logic)

Multiple fields at the same level are combined with AND logic:

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"status":{"eq":"Active"},"createdAt":{"gt":"2024-01-01"}}
```

### 3. JSON Data Field Filter (attributes)

Filter on JSON data fields using the `attributes` prefix:

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"attributes":{"customerId":{"eq":"CUST-123"}}}
```

### 4. Mixed Filter (Instance + JSON Fields)

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"key":{"like":"payment"},"status":{"eq":"Active"},"attributes":{"amount":{"gt":"500"}}}
```

### 5. Date Range Filter

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"createdAt":{"between":["2024-01-01","2024-01-31"]}}
```

### 6. Status IN Filter

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"status":{"in":["Active","Busy"]}}
```

---

## Logical Operators

### AND Operator

Combines multiple conditions where all must be true:

```json
{
  "and": [
    {"status": {"eq": "Active"}},
    {"attributes": {"amount": {"gt": "500"}}}
  ]
}
```

### OR Operator

Combines multiple conditions where any can be true:

```json
{
  "or": [
    {"key": {"eq": "payment-12345"}},
    {"key": {"eq": "payment-12346"}}
  ]
}
```

### NOT Operator

Negates a condition:

```json
{
  "not": {"status": {"in": ["Completed", "Faulted"]}}
}
```

### Complex Nested Example

```json
{
  "and": [
    {"status": {"eq": "Active"}},
    {
      "or": [
        {"attributes": {"priority": {"eq": "high"}}},
        {"attributes": {"amount": {"gt": "10000"}}}
      ]
    }
  ]
}
```

---

## Group By and Aggregations

### Group By with Count

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"groupBy":{"field":"attributes.status","aggregations":{"count":true}}}
```

**Response:**
```json
{
  "groups": [
    {"name": "pending", "count": 45},
    {"name": "approved", "count": 123},
    {"name": "rejected", "count": 12}
  ]
}
```

### Group By with Multiple Aggregations

```http
GET /banking/workflows/payment-workflow/functions/data?filter={"groupBy":{"field":"attributes.currency","aggregations":{"count":true,"sum":"attributes.amount","avg":"attributes.amount","min":"attributes.amount","max":"attributes.amount"}}}
```

**Response:**
```json
{
  "groups": [
    {"name": "USD", "count": 150, "sum": 450000, "avg": 3000, "min": 10, "max": 50000},
    {"name": "EUR", "count": 75, "sum": 180000, "avg": 2400, "min": 50, "max": 25000}
  ]
}
```

### Supported Aggregations

| Aggregation | Description |
|-------------|-------------|
| `count` | Count of items in group |
| `sum` | Sum of numeric field |
| `avg` | Average of numeric field |
| `min` | Minimum value |
| `max` | Maximum value |

---

## Best Practices

### 1. Use GraphQL Format for Complex Queries

GraphQL format is more readable and supports logical operators.

**Good:**
```json
{
  "and": [
    {"status": {"eq": "Active"}},
    {"attributes": {"amount": {"gt": "500"}}}
  ]
}
```

### 2. Use Specific Fields for Better Performance

Filter on indexed Instance columns when possible.

**Better Performance:**
```json
{"key": {"eq": "payment-12345"}}
```

**Slower:**
```json
{"attributes": {"unindexedField": {"eq": "value"}}}
```

### 3. Use Status Names for Readability

```json
{"status": {"eq": "Active"}}
```
is equivalent to:
```json
{"status": {"eq": "A"}}
```

### 4. Use Group By for Analytics

When you need statistics, use group by instead of fetching all records.

```json
{
  "groupBy": {
    "field": "attributes.status",
    "aggregations": {"count": true, "sum": "attributes.amount"}
  }
}
```

### 5. Always Use Pagination

Always use `page` and `pageSize` parameters:

```http
GET /banking/workflows/payment-workflow/functions/data?filter={...}&page=1&pageSize=20
```

---

## Error Handling

### Invalid Filter Syntax

```json
{
  "error": {
    "code": "invalid_filter",
    "message": "Invalid filter syntax. Valid JSON expected."
  }
}
```

### Unsupported Operator

```json
{
  "error": {
    "code": "unsupported_operator",
    "message": "'regex' operator is not supported",
    "supportedOperators": ["eq", "ne", "gt", "ge", "lt", "le", "between", "like", "startswith", "endswith", "in", "nin", "isnull"]
  }
}
```

### Invalid Column Name

```json
{
  "error": {
    "code": "invalid_column",
    "message": "'invalidColumn' is not a valid Instance column. Use 'attributes.fieldName' for JSON fields.",
    "validColumns": ["key", "flow", "status", "currentState", "createdAt", "modifiedAt", "completedAt", "isTransient"]
  }
}
```

---

## Performance Tips

1. **Use Pagination**: Always use `page` and `pageSize` parameters
2. **Filter on Indexed Columns**: Prefer `key`, `status`, `createdAt` for better performance
3. **Limit Group By Fields**: Use maximum 2-3 fields for optimal performance
4. **Use Date Ranges Wisely**: Narrow date ranges improve query performance
5. **Avoid Wildcard Searches on Large Datasets**: Use `startswith` or `endswith` instead of `like` when possible

---

## Related Documentation

- [Function APIs](./function.md) - Built-in system functions (State, Data, View)
- [Custom Functions](./custom-function.md) - User-defined functions
- [Instance Lifecycle](../how-to/start-instance.md) - Starting and managing instances
