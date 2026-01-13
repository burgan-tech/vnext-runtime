# GetInstances Task

The GetInstances Task enables fetching instance data from other workflows with support for pagination, sorting, and filtering. This task is useful for cross-workflow data queries and building aggregated views.

## Overview

| Property | Value |
|----------|-------|
| **Task Type** | `15` |
| **Purpose** | Fetch instances from another workflow |
| **API Endpoint** | `GET /api/v1/{domain}/workflows/{workflow}/functions/data` |

## Configuration

### Task Definition

```json
{
  "key": "fetch-customer-orders",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-tasks",
  "flowVersion": "1.0.0",
  "tags": ["data-fetch", "workflow-communication", "pagination"],
  "attributes": {
    "type": "15",
    "config": {
      "domain": "sales",
      "flow": "order-workflow",
      "page": 1,
      "pageSize": 10,
      "sort": "-CreatedAt",
      "filter": ["status eq 'active'"]
    }
  }
}
```

### Config Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `domain` | string | Yes | - | Target workflow domain |
| `flow` | string | Yes | - | Target workflow name |
| `page` | int | No | `1` | Page number (1-based index) |
| `pageSize` | int | No | `10` | Number of items per page |
| `sort` | string | No | - | Sort field with optional direction prefix |
| `filter` | string[] | No | - | Array of filter expressions |
| `useDapr` | bool | No | `false` | Use Dapr service invocation instead of direct HTTP |

### Sort Parameter

The `sort` parameter specifies the field to sort by with an optional direction prefix:

| Format | Description | Example |
|--------|-------------|---------|
| `FieldName` | Ascending order | `CreatedAt` |
| `-FieldName` | Descending order | `-CreatedAt` |

**Common Sort Fields:**
- `CreatedAt` - Instance creation date
- `UpdatedAt` - Last update date
- `Key` - Instance key

### Filter Parameter

The `filter` parameter accepts an array of filter expressions that are applied to the query:

```json
{
  "filter": [
    "status eq 'active'",
    "amount gt 1000"
  ]
}
```

## Usage Examples

### Basic Usage

Fetch the first 10 instances from a workflow:

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "customer-workflow"
    }
  }
}
```

### With Pagination

Fetch page 3 with 25 items per page:

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "customer-workflow",
      "page": 3,
      "pageSize": 25
    }
  }
}
```

### With Sorting

Fetch instances sorted by creation date (newest first):

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "customer-workflow",
      "sort": "-CreatedAt"
    }
  }
}
```

### With Filtering

Fetch active instances with specific criteria:

```json
{
  "attributes": {
    "type": "15",
    "config": {
      "domain": "core",
      "flow": "order-workflow",
      "filter": [
        "status eq 'active'",
        "total gt 500"
      ]
    }
  }
}
```

### Complete Example

Full configuration with all parameters:

```json
{
  "key": "get-pending-orders",
  "version": "1.0.0",
  "domain": "core",
  "flow": "sys-tasks",
  "flowVersion": "1.0.0",
  "tags": ["task-test", "instances", "data-fetch", "filter-test", "pagination"],
  "attributes": {
    "type": "15",
    "config": {
      "domain": "sales",
      "flow": "order-workflow",
      "page": 1,
      "pageSize": 50,
      "sort": "-CreatedAt",
      "filter": [
        "state eq 'pending'",
        "priority eq 'high'"
      ],
      "useDapr": false
    }
  }
}
```

## Response Mapping

The task returns a paginated response containing instance data. Use mapping to extract and transform the data:

### Response Structure

```json
{
  "links": {
    "self": "/api/v1/core/workflows/order-workflow/functions/data?page=1&pageSize=10",
    "first": "/api/v1/core/workflows/order-workflow/functions/data?page=1&pageSize=10",
    "next": "/api/v1/core/workflows/order-workflow/functions/data?page=2&pageSize=10",
    "prev": ""
  },
  "items": [
    {
      "data": {
        "orderId": "ORDER-001",
        "status": "pending",
        "amount": 1500
      },
      "etag": "01ARZ3NDEKTSV4RRFFQ69G5FAV",
      "extensions": {}
    },
    {
      "data": {
        "orderId": "ORDER-002",
        "status": "active",
        "amount": 2300
      },
      "etag": "01ARZ3NDEKTSV4RRFFQ69G5FAW",
      "extensions": {}
    }
  ]
}
```

### Response Fields

| Field | Description |
|-------|-------------|
| `links.self` | Current page URL |
| `links.first` | First page URL |
| `links.next` | Next page URL (empty if last page) |
| `links.prev` | Previous page URL (empty if first page) |
| `items` | Array of instance data |
| `items[].data` | Instance data object |
| `items[].etag` | ETag for concurrency control |
| `items[].extensions` | Extension data (if any) |

### Mapping Example

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var response = context.Body;
    
    var items = response?.items;
    var links = response?.links;
    
    // Process each item
    var orders = new List<dynamic>();
    foreach (var item in items)
    {
        var data = item?.data;
        var etag = item?.etag;
        orders.Add(new { data, etag });
    }
    
    return new ScriptResponse
    {
        Data = new
        {
            orders = orders,
            hasNextPage = !string.IsNullOrEmpty(links?.next?.ToString())
        }
    };
}
```

## Use Cases

### 1. Cross-Workflow Data Aggregation

Fetch data from multiple workflows to build aggregated views or reports.

### 2. Reference Data Lookup

Look up reference data from master data workflows during processing.

### 3. Validation Against Existing Records

Check for existing records before creating new instances.

### 4. Dashboard Data Collection

Collect instance data from various workflows for dashboard displays.

## Best Practices

1. **Use Pagination**: Always use pagination for large datasets to avoid performance issues.

2. **Limit Page Size**: Keep `pageSize` reasonable (10-50) for optimal performance.

3. **Use Filters**: Apply filters to reduce the amount of data transferred.

4. **Cache Results**: Consider caching frequently accessed data to reduce API calls.

5. **Handle Empty Results**: Always handle cases where no instances match the criteria.

## Related Documentation

- [Task Overview](../task.md) - Task types and execution
- [Mapping](../mapping.md) - Data mapping between tasks
- [Trigger Task](./trigger-task.md) - Workflow instance management
