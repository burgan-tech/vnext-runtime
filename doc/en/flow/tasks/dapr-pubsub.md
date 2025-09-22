# Dapr PubSub Task

Dapr PubSub Task is a task type used for event-driven messaging structure using the Dapr publish/subscribe feature. This task type provides asynchronous messaging, event sourcing, and loose coupling.

## Features

- ✅ Event-driven architecture support
- ✅ Multiple message broker support (Redis, Kafka, RabbitMQ, etc.)
- ✅ At-least-once delivery guarantee
- ✅ Message routing and filtering
- ✅ Dead letter queue support
- ✅ Message ordering
- ✅ Bulk publishing
- ✅ Cloud Events standard
- ✅ Message metadata
- ✅ Topic-based subscription

## Task Definition

### Basic Structure

```json
{
  "key": "publish-user-event",
  "flow": "sys-tasks",
  "domain": "core",
  "version": "1.0.0",
  "tags": [
    "pubsub",
    "messaging",
    "event"
  ],
  "attributes": {
    "type": "4",
    "config": {
      "pubSubName": "messagebus",
      "topic": "user-events",
      "data": {
        "eventType": "UserRegistered",
        "userId": "{{data.userId}}",
        "email": "{{data.email}}",
        "timestamp": "{{now()}}"
      },
      "metadata": {
        "priority": "high",
        "source": "user-service"
      }
    }
  }
}
```

### Fields

The following fields are defined in the config section of DAPR PubSub Task:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `pubSubName` | string | - | PubSub component name (Required) |
| `topic` | string | - | Topic to send the message to (Required) |
| `data` | object | {} | Message content |
| `metadata` | object | {} | Message metadata |

## Property Access

Properties in the DaprPubSubTask class are defined as read-only. Special methods must be used to change these properties:

- **PubSubName**: Changed with `SetPubSubName(string pubSubName)` method
- **Topic**: Changed with `SetTopic(string topic)` method
- **Data**: Changed with `SetData(dynamic data)` method
- **Metadata**: Changed with `SetMetadata(Dictionary<string, string?> metadata)` method

## Supported Message Brokers

### Redis Streams
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: redis-pubsub
spec:
  type: pubsub.redis
  version: v1
  metadata:
  - name: redisHost
    value: "redis:6379"
  - name: redisPassword
    value: ""
```

### Apache Kafka
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: kafka-pubsub
spec:
  type: pubsub.kafka
  version: v1
  metadata:
  - name: brokers
    value: "kafka:9092"
  - name: authType
    value: "none"
```

### RabbitMQ
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: rabbitmq-pubsub
spec:
  type: pubsub.rabbitmq
  version: v1
  metadata:
  - name: host
    value: "amqp://rabbitmq:5672"
  - name: durable
    value: "true"
```

### Azure Service Bus
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: servicebus-pubsub
spec:
  type: pubsub.azure.servicebus
  version: v1
  metadata:
  - name: connectionString
    secretKeyRef:
      name: servicebus-secret
      key: connectionString
```

### AWS SNS/SQS
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: aws-pubsub
spec:
  type: pubsub.aws.snssqs
  version: v1
  metadata:
  - name: region
    value: "us-east-1"
  - name: accessKey
    secretKeyRef:
      name: aws-secret
      key: accessKey
```

### Google Cloud Pub/Sub
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: gcp-pubsub
spec:
  type: pubsub.gcp.pubsub
  version: v1
  metadata:
  - name: projectId
    value: "my-gcp-project"
  - name: authProviderX509CertUrl
    value: "https://www.googleapis.com/oauth2/v1/certs"
```

## Message Publishing

### Simple Event Publishing
```json
{
  "pubsubName": "messagebus",
  "topic": "order-events",
  "data": {
    "eventType": "OrderCreated",
    "orderId": "{{data.orderId}}",
    "customerId": "{{data.customerId}}",
    "amount": "{{data.amount}}"
  }
}
```

### Cloud Events Format
```json
{
  "pubsubName": "messagebus",
  "topic": "user-events",
  "data": {
    "specversion": "1.0",
    "type": "com.company.user.created",
    "source": "user-service",
    "id": "{{uuid()}}",
    "time": "{{now()}}",
    "datacontenttype": "application/json",
    "data": {
      "userId": "{{data.userId}}",
      "email": "{{data.email}}"
    }
  },
  "metadata": {
    "cloudevent": "true"
  }
}
```

### Batch Publishing
```json
{
  "pubsubName": "messagebus",
  "topic": "bulk-events",
  "data": [
    {
      "eventType": "UserCreated",
      "userId": "user1",
      "email": "user1@example.com"
    },
    {
      "eventType": "UserCreated", 
      "userId": "user2",
      "email": "user2@example.com"
    }
  ],
  "metadata": {
    "bulkPublish": "true",
    "maxBulkSize": "100"
  }
}
```

## Mapping Examples

### Input Mapping

```csharp
public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var pubsubTask = task as DaprPubSubTask;
    
    // Dynamic pubsub component selection
    if (context.Instance.Data.environment == "production")
    {
        pubsubTask.SetPubSubName("prod-messagebus");
    }
    else
    {
        pubsubTask.SetPubSubName("dev-messagebus");
    }
    
    // Dynamic topic selection based on event category
    string topic = context.Instance.Data.eventCategory switch
    {
        "user" => "user-events",
        "order" => "order-events", 
        "payment" => "payment-events",
        _ => "general-events"
    };
    pubsubTask.SetTopic(topic);
    
    // Event envelope creation with Cloud Events format
    var eventData = new
    {
        // Cloud Events standard
        specversion = "1.0",
        type = $"com.company.{context.Instance.Data.eventCategory}.{context.Instance.Data.eventType}",
        source = context.GetConfiguration("ServiceName"),
        id = Guid.NewGuid().ToString(),
        time = DateTime.UtcNow.ToString("O"),
        datacontenttype = "application/json",
        
        // Event metadata
        subject = context.Instance.Data.entityId,
        
        // Actual event data
        data = new
        {
            entityId = context.Instance.Data.entityId,
            entityType = context.Instance.Data.entityType,
            eventType = context.Instance.Data.eventType,
            timestamp = DateTime.UtcNow,
            version = context.Instance.Data.version ?? 1,
            payload = context.Instance.Data.payload,
            
            // Context information
            causedBy = context.Instance.Data.userId,
            correlationId = context.Instance.Data.correlationId,
            workflowId = context.Instance.WorkflowId,
            instanceId = context.Instance.Id
        }
    };
    
    pubsubTask.SetData(eventData);
    
    // Message metadata preparation
    var metadata = new Dictionary<string, string?>
    {
        // Routing
        ["partition"] = context.Instance.Data.partitionKey ?? context.Instance.Data.entityId,
        ["routingKey"] = $"{context.Instance.Data.eventCategory}.{context.Instance.Data.eventType}",
        
        // Priority
        ["priority"] = context.Instance.Data.priority ?? "normal",
        
        // Correlation
        ["correlationId"] = context.Instance.Data.correlationId,
        ["causationId"] = context.Instance.Data.causationId,
        
        // Expiration
        ["ttl"] = context.Instance.Data.ttl ?? "86400", // 24 hours default
        
        // Custom headers
        ["source"] = context.GetConfiguration("ServiceName"),
        ["version"] = context.GetConfiguration("ServiceVersion"),
        ["environment"] = context.GetConfiguration("Environment")
    };
    
    pubsubTask.SetMetadata(metadata);
    
    return new ScriptResponse();
}
```

### Output Mapping

```csharp
public async Task<ScriptResponse> OutputHandler(ScriptContext context)
{
    var output = new ScriptResponse();
    var response = context.Body;
    
    if (response.isSuccess)
    {
        var publishResult = response.data;
        
        output.Data = new
        {
            eventPublished = true,
            messageId = publishResult?.messageId,
            topic = response.metadata?.Topic,
            pubsubName = response.metadata?.PubSubName,
            publishTime = DateTime.UtcNow,
            
            // Publishing metadata
            publishInfo = new
            {
                pubSubName = response.metadata?.PubSubName,
                topic = response.metadata?.Topic,
                responseTime = response.executionDurationMs,
                taskType = response.taskType
            },
            
            // Delivery info
            deliveryGuarantee = "at-least-once",
            
            // Performance metrics
            publishDuration = response.executionDurationMs
        };
    }
    else
    {
        // Error handling
        output.Data = new
        {
            eventPublishFailed = true,
            error = response.errorMessage,
            
            // Error classification
            errorType = ClassifyPublishError(response.errorMessage),
            retryable = IsRetryablePublishError(response.errorMessage),
            
            // Publishing info for debugging
            publishInfo = new
            {
                pubSubName = response.metadata?.PubSubName,
                topic = response.metadata?.Topic
            },
            
            // Processing timestamp
            failedAt = DateTime.UtcNow
        };
    }
    
    return output;
}

private string ClassifyPublishError(string errorMessage)
{
    if (errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        return "timeout";
    if (errorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase))
        return "connection";
    if (errorMessage.Contains("authentication", StringComparison.OrdinalIgnoreCase))
        return "authentication";
    if (errorMessage.Contains("quota", StringComparison.OrdinalIgnoreCase))
        return "quota";
    if (errorMessage.Contains("topic", StringComparison.OrdinalIgnoreCase))
        return "topic-config";
    if (errorMessage.Contains("component", StringComparison.OrdinalIgnoreCase))
        return "component-config";
    
    return "general-error";
}

private bool IsRetryablePublishError(string errorMessage)
{
    return errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("throttled", StringComparison.OrdinalIgnoreCase) ||
           errorMessage.Contains("temporary", StringComparison.OrdinalIgnoreCase);
}
```

## Standard Response

Dapr PubSub Task returns the following standard response structure:

```csharp
{
    "data": {
        "Published": true,
        "Message": "Event published successfully"
    },
    "isSuccess": true,
    "errorMessage": null,
    "metadata": {
        "PubSubName": "messagebus",
        "Topic": "user-events"
    },
    "executionDurationMs": 45,
    "taskType": "DaprPubSub"
}
```

## Best Practices

### 1. PubSub Component Selection
```csharp
// ✅ Correct - With SetPubSubName method
pubsubTask.SetPubSubName("production-messagebus");

// ❌ Wrong - Direct assignment not possible
pubsubTask.PubSubName = "production-messagebus";  // Read-only property
```

### 2. Topic Management
```csharp
// ✅ Correct - With SetTopic method
pubsubTask.SetTopic("user-events");

// ✅ Correct - Dynamic topic selection
string topic = context.Instance.Data.eventType switch
{
    "user-created" => "user-events",
    "order-placed" => "order-events",
    _ => "general-events"
};
pubsubTask.SetTopic(topic);
```

### 3. Data Preparation
```csharp
// ✅ Correct - Structured object with SetData method
var eventData = new
{
    eventType = "UserRegistered",
    userId = context.Instance.Data.userId,
    timestamp = DateTime.UtcNow
};
pubsubTask.SetData(eventData);

// ❌ Wrong - String serialization
pubsubTask.SetData(JsonSerializer.Serialize(data));
```

### 4. Metadata Handling
```csharp
// ✅ Correct - Dictionary with SetMetadata method
var metadata = new Dictionary<string, string?>
{
    ["priority"] = "high",
    ["correlationId"] = context.Instance.Data.correlationId,
    ["source"] = "user-service"
};
pubsubTask.SetMetadata(metadata);

// ❌ Wrong - Direct assignment
pubsubTask.Metadata = metadata; // Read-only property
```

### 5. Error Handling
```csharp
// ✅ Correct - Response check
if (response.isSuccess)
{
    output.Data = new { published = true, messageId = response.data.messageId };
}
else
{
    output.Data = new { published = false, error = response.errorMessage };
}
```

## Common Problems

### Problem: PubSub component not found
**Solution:** Check PubSub component configuration and app ID

### Problem: Topic configuration error
**Solution:** Check topic name and pubsub component compatibility

### Problem: Message serialization failed
**Solution:** Make sure you pass a valid object to SetData method

### Problem: Metadata format error
**Solution:** Send data in Dictionary<string, string?> format to SetMetadata method
