# Dapr PubSub Task

Dapr PubSub Task, Dapr publish/subscribe özelliği kullanarak event-driven messaging yapısı için kullanılan görev türüdür. Bu görev türü ile asenkron mesajlaşma, event sourcing ve loose coupling sağlanır.

## Özellikler

- ✅ Event-driven architecture desteği
- ✅ Multiple message broker desteği (Redis, Kafka, RabbitMQ, etc.)
- ✅ At-least-once delivery guarantee
- ✅ Message routing ve filtering
- ✅ Dead letter queue desteği
- ✅ Message ordering
- ✅ Bulk publishing
- ✅ Cloud Events standardı
- ✅ Message metadata
- ✅ Topic-based subscription

## Görev Tanımı

### Temel Yapı

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

### Alanlar

DAPR PubSub Task'ın config bölümünde aşağıdaki alanlar tanımlanır:

| Alan | Tip | Varsayılan | Açıklama |
|------|-----|------------|----------|
| `pubSubName` | string | - | PubSub component adı (Zorunlu) |
| `topic` | string | - | Mesajın gönderileceği topic (Zorunlu) |
| `data` | object | {} | Mesaj içeriği |
| `metadata` | object | {} | Mesaj metadata'sı |

## Property Erişimi

DaprPubSubTask sınıfında property'ler read-only olarak tanımlanmıştır. Bu property'leri değiştirmek için özel metodlar kullanılmalıdır:

- **PubSubName**: `SetPubSubName(string pubSubName)` metoduyla değiştirilir
- **Topic**: `SetTopic(string topic)` metoduyla değiştirilir
- **Data**: `SetData(dynamic data)` metoduyla değiştirilir
- **Metadata**: `SetMetadata(Dictionary<string, string?> metadata)` metoduyla değiştirilir

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

## Message Metadata

### Message Priority
```json
{
  "metadata": {
    "priority": "high",
    "urgency": "immediate"
  }
}
```

### Message Routing
```json
{
  "metadata": {
    "routingKey": "user.created.premium",
    "partition": "{{data.userId % 10}}",
    "messageGroup": "user-operations"
  }
}
```

### Message Expiration
```json
{
  "metadata": {
    "ttl": "3600",  // 1 hour
    "expireAt": "{{now().addHours(24)}}"
  }
}
```

### Message Correlation
```json
{
  "metadata": {
    "correlationId": "{{data.correlationId}}",
    "causationId": "{{data.causationId}}",
    "messageId": "{{uuid()}}"
  }
}
```

## Event Types

### Domain Events
```json
{
  "topic": "domain-events",
  "data": {
    "eventType": "UserRegistered",
    "aggregateId": "{{data.userId}}",
    "aggregateType": "User",
    "version": 1,
    "timestamp": "{{now()}}",
    "payload": {
      "userId": "{{data.userId}}",
      "email": "{{data.email}}",
      "registrationDate": "{{now()}}"
    }
  }
}
```

### Integration Events
```json
{
  "topic": "integration-events",
  "data": {
    "eventType": "PaymentProcessed",
    "eventId": "{{uuid()}}",
    "source": "payment-service",
    "target": ["order-service", "notification-service"],
    "payload": {
      "paymentId": "{{data.paymentId}}",
      "orderId": "{{data.orderId}}",
      "amount": "{{data.amount}}",
      "status": "completed"
    }
  }
}
```

### System Events
```json
{
  "topic": "system-events", 
  "data": {
    "eventType": "ServiceHealthCheck",
    "serviceName": "{{config.serviceName}}",
    "status": "healthy",
    "timestamp": "{{now()}}",
    "metrics": {
      "cpu": "{{metrics.cpu}}",
      "memory": "{{metrics.memory}}"
    }
  }
}
```

### Business Events
```json
{
  "topic": "business-events",
  "data": {
    "eventType": "CustomerUpgraded",
    "customerId": "{{data.customerId}}",
    "previousTier": "{{data.oldTier}}",
    "newTier": "{{data.newTier}}",
    "effectiveDate": "{{data.upgradeDate}}",
    "benefits": "{{data.benefits}}"
  }
}
```

## Message Ordering

### Partition-based Ordering
```json
{
  "metadata": {
    "partition": "{{data.customerId}}",
    "orderingKey": "customer-{{data.customerId}}"
  }
}
```

### Topic-level Ordering
```json
{
  "metadata": {
    "enableMessageOrdering": "true",
    "sequenceNumber": "{{data.sequenceId}}"
  }
}
```

### FIFO Queue
```json
{
  "metadata": {
    "messageGroupId": "order-processing",
    "messageDeduplicationId": "{{data.orderId}}-{{data.version}}"
  }
}
```

## Error Handling

### Dead Letter Queue
```json
{
  "metadata": {
    "deadLetterTopic": "failed-events",
    "maxRetries": "3",
    "retryDelay": "5s"
  }
}
```

### Custom Error Handling
```json
{
  "metadata": {
    "errorHandler": "custom",
    "errorTopic": "error-events",
    "includeStackTrace": "true"
  }
}
```

## Mapping Örnekleri

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
        // Cloud Events standardı
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

## Advanced Features

### Message Filtering
```json
{
  "metadata": {
    "filter": {
      "eventType": "UserRegistered",
      "priority": "high"
    }
  }
}
```

### Message Transformation
```json
{
  "metadata": {
    "transform": {
      "format": "avro",
      "schema": "user-event-schema-v1"
    }
  }
}
```

### Conditional Publishing
```csharp
public async Task<ScriptResponse> InputHandler(WorkflowTask task, ScriptContext context)
{
    var data = context.GetData<dynamic>();
    
    // Conditional publishing
    if (data.amount > 10000)
    {
        pubsubTask.Topic = "high-value-transactions";
        pubsubTask.Metadata["priority"] = "critical";
    }
    else
    {
        pubsubTask.Topic = "standard-transactions";
    }
    
    return ScriptResponse.Success();
}
```

## Event Sourcing Patterns

### Event Store Integration
```json
{
  "pubsubName": "event-store",
  "topic": "{{data.aggregateType}}-{{data.aggregateId}}",
  "data": {
    "eventType": "{{data.eventType}}", 
    "aggregateId": "{{data.aggregateId}}",
    "aggregateType": "{{data.aggregateType}}",
    "version": "{{data.version}}",
    "timestamp": "{{now()}}",
    "data": "{{data.eventData}}"
  }
}
```

### Snapshot Events
```json
{
  "topic": "snapshots",
  "data": {
    "eventType": "AggregateSnapshot",
    "aggregateId": "{{data.aggregateId}}",
    "snapshotVersion": "{{data.version}}",
    "snapshotData": "{{data.snapshot}}"
  }
}
```

## Standart Yanıt

Dapr PubSub Task aşağıdaki standart yanıt yapısını döner:

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

## Performance Optimization

### Batch Publishing
```json
{
  "metadata": {
    "bulkPublish": "true",
    "maxBulkSize": "100",
    "bulkTimeout": "100ms"
  }
}
```

### Async Publishing
```json
{
  "metadata": {
    "publishAsync": "true",
    "ackTimeout": "30s"
  }
}
```

### Connection Pooling
```json
{
  "metadata": {
    "connectionPool": {
      "maxConnections": 50,
      "idleTimeout": "30s"
    }
  }
}
```

## En İyi Uygulamalar

### 1. PubSub Component Selection
```csharp
// ✅ Doğru - SetPubSubName metoduyla
pubsubTask.SetPubSubName("production-messagebus");

// ❌ Yanlış - Direct assignment mümkün değil
pubsubTask.PubSubName = "production-messagebus";  // Read-only property
```

### 2. Topic Management
```csharp
// ✅ Doğru - SetTopic metoduyla
pubsubTask.SetTopic("user-events");

// ✅ Doğru - Dynamic topic selection
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
// ✅ Doğru - SetData metoduyla structured object
var eventData = new
{
    eventType = "UserRegistered",
    userId = context.Instance.Data.userId,
    timestamp = DateTime.UtcNow
};
pubsubTask.SetData(eventData);

// ❌ Yanlış - String serialize etme
pubsubTask.SetData(JsonSerializer.Serialize(data));
```

### 4. Metadata Handling
```csharp
// ✅ Doğru - SetMetadata metoduyla Dictionary
var metadata = new Dictionary<string, string?>
{
    ["priority"] = "high",
    ["correlationId"] = context.Instance.Data.correlationId,
    ["source"] = "user-service"
};
pubsubTask.SetMetadata(metadata);

// ❌ Yanlış - Direct assignment
pubsubTask.Metadata = metadata; // Read-only property
```

### 5. Error Handling
```csharp
// ✅ Doğru - Response kontrolü
if (response.isSuccess)
{
    output.Data = new { published = true, messageId = response.data.messageId };
}
else
{
    output.Data = new { published = false, error = response.errorMessage };
}
```

## Sık Karşılaşılan Sorunlar

### Problem: PubSub component not found
**Çözüm:** PubSub component configuration ve app ID'yi kontrol edin

### Problem: Topic configuration error
**Çözüm:** Topic name ve pubsub component compatibility'sini kontrol edin

### Problem: Message serialization failed
**Çözüm:** SetData metoduna valid object geçtiğinizden emin olun

### Problem: Metadata format error
**Çözüm:** SetMetadata metoduna Dictionary<string, string?> formatında data gönderin