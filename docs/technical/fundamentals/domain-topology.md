# Domain Topology and Architecture

## Platform Domain Concept

The vNext Runtime platform is based on the **Domain** concept. A domain represents an isolated runtime environment that corresponds to a business area, product group, or team responsibility.

### Domain = Runtime Principle

**Each domain has its own independent runtime.** This principle forms the foundation of the platform architecture:

- One domain = One vNext Runtime instance
- Each domain is unique and independent
- Complete isolation is provided between domains

## Domain Examples

In an organization, domains can be organized as follows:

### Product Group-Based Domain
```
Onboarding Domain
├── vNext Runtime (onboarding)
├── Database (onboarding_db)
├── PubSub (onboarding_events)
└── State Store (onboarding_state)
```

**Example:** The onboarding team managing customer acceptance processes has its own domain.

### Team Responsibility-Based Domains
```
Integration Team
├── IDM Domain (Identity management)
│   ├── vNext Runtime (idm)
│   └── Infrastructure
└── Notification Domain (Notification services)
    ├── vNext Runtime (notification)
    └── Infrastructure
```

**Example:** The integration team manages IDM and Notification systems under their responsibility as separate domains.

## Benefits of Domain Isolation

### 1. Infrastructure Isolation
Each domain has its own infrastructure components:
- **Database**: Domain-specific database
- **PubSub**: Domain-specific messaging channels
- **State Store**: Domain-specific state management
- **Secrets**: Domain-specific security configuration

### 2. Independent Development
- Each domain team can develop at their own pace
- Inter-domain dependencies are minimal
- Version management is done per domain
- Deployment is performed independently

### 3. Scalability
- Each domain scales according to its needs
- High-load domains can receive more resources
- Low-load domains run with minimal resources
- Resource utilization is optimized

### 4. Fault Isolation
- Issues in one domain do not affect others
- Backup and restore are done per domain
- Maintenance and updates are planned independently

## Inter-Domain Communication

Although domains are isolated from each other, they can communicate according to business requirements:

### 1. Through API Gateway
```
┌─────────────┐      API Gateway      ┌─────────────┐
│  Onboarding │◄──────────────────────►│     IDM     │
│   Domain    │    REST/HTTP Calls     │   Domain    │
└─────────────┘                        └─────────────┘
```

- Synchronous communication
- REST API calls
- HTTP Task usage

### 2. Event-Driven Structures
```
┌─────────────┐                        ┌─────────────┐
│  Payments   │──┐                  ┌──│Notification │
│   Domain    │  │  Event Bus       │  │   Domain    │
└─────────────┘  │  (PubSub)        │  └─────────────┘
                 │                  │
                 ├─────────┬────────┤
                 │         │        │
                 └────────Event────┘
```

- Asynchronous communication
- Event-based integration
- Loose coupling
- DaprPubSub Task usage

## C4 Context Diagram - Multi-Domain Architecture

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

title vNext Platform - Multi-Domain Architecture (Context Level)

Person(customer, "Customer", "Mobile/Web user")
Person(employee, "Employee", "Backoffice user")
System_Ext(external_api, "External Systems", "Bank, Payment, KYC systems")

System_Boundary(vnext_platform, "vNext Platform") {
    System(onboarding_domain, "Onboarding Domain", "Customer acceptance processes\nvNext Runtime")
    System(idm_domain, "IDM Domain", "Identity and authorization\nvNext Runtime")
    System(notification_domain, "Notification Domain", "Notification services\nvNext Runtime")
    System(payment_domain, "Payment Domain", "Payment processes\nvNext Runtime")
}

System_Ext(api_gateway, "API Gateway", "Access layer to domains")
System_Ext(event_bus, "Event Bus", "Inter-domain event communication")

Rel(customer, api_gateway, "Uses", "HTTPS")
Rel(employee, api_gateway, "Uses", "HTTPS")
Rel(api_gateway, onboarding_domain, "Routes to", "HTTP/REST")
Rel(api_gateway, idm_domain, "Routes to", "HTTP/REST")
Rel(api_gateway, notification_domain, "Routes to", "HTTP/REST")
Rel(api_gateway, payment_domain, "Routes to", "HTTP/REST")

Rel(onboarding_domain, idm_domain, "Authentication", "HTTP/REST")
Rel(onboarding_domain, notification_domain, "Send notification", "Event")
Rel(payment_domain, notification_domain, "SMS/Push notification", "Event")
Rel(onboarding_domain, external_api, "KYC query", "HTTPS")
Rel(payment_domain, external_api, "Payment transaction", "HTTPS")

Rel(onboarding_domain, event_bus, "Publishes events", "PubSub")
Rel(payment_domain, event_bus, "Publishes events", "PubSub")
Rel(event_bus, notification_domain, "Consumes events", "PubSub")

@enduml
```

## C4 Container Diagram - Domain Internal Structure

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

LAYOUT_WITH_LEGEND()

title Single Domain Internal Structure (Container Level)

Person(user, "User", "Domain user")

System_Boundary(domain, "vNext Domain (e.g., Onboarding)") {
    Container(orchestration, "vnext-app", "Orchestration Service", "Flow management, state machine, transition control")
    Container(execution, "vnext-execution-app", "Execution Service", "Task execution, serverless worker")
    Container(init, "vnext-init", "Seed Service", "Initial setup, system components")
    
    ContainerDb(database, "Domain Database", "PostgreSQL", "Flow instances, state, data")
    ContainerDb(state_store, "State Store", "Redis/Dapr", "Distributed state, cache")
    Container(pubsub, "PubSub", "RabbitMQ/Dapr", "Event messaging")
}

System_Ext(external_service, "External Services", "APIs, webhooks")

Rel(user, orchestration, "Workflow management", "HTTPS/REST")
Rel(orchestration, database, "Instance CRUD", "SQL")
Rel(orchestration, execution, "Execute task", "Dapr Service Invocation")
Rel(orchestration, state_store, "Reads/writes state", "Dapr State API")
Rel(orchestration, pubsub, "Event publish/subscribe", "Dapr PubSub API")

Rel(execution, external_service, "HTTP Task", "HTTPS")
Rel(execution, database, "Reads data", "SQL")
Rel(execution, state_store, "Uses cache", "Dapr State API")

Rel(init, database, "Create schema, seed data", "SQL")
Rel(init, orchestration, "Deploy system flows", "Internal API")

@enduml
```

## Domain Management Best Practices

### Define Domain Boundaries Correctly
- **By business area**: Each domain should represent a specific business function
- **By team responsibility**: Domain ownership should be clear
- **By scale requirements**: Areas with different load characteristics should be separate domains

### Maintain Domain Isolation
- Direct database access between domains is prohibited
- All communication should be through API or Events
- Shared infrastructure should be minimized

### Monitoring and Observability
- Separate monitoring dashboards for each domain
- Domain-based metric collection
- Distributed tracing for inter-domain call tracking

### Version Management
- Domains are versioned independently
- API contracts are managed with semantic versioning
- Breaking changes are coordinated but deployment is independent

## Domain Lifecycle

### 1. Domain Creation
```bash
# Infrastructure provisioning
- Create domain database
- Configure domain state store
- Configure domain PubSub

# vNext Runtime deployment
- System setup with vnext-init
- vnext-app deployment
- vnext-execution-app deployment
```

### 2. Domain Operations
- Flow deployment and management
- Monitoring and alerting
- Scaling and optimization
- Backup and disaster recovery

### 3. Domain Retirement
- Migration planning
- Dependency analysis
- Graceful shutdown
- Data archiving

## Conclusion

Domain topology is the fundamental architectural decision that makes the vNext Runtime platform scalable, flexible, and manageable. Each domain having its own independent runtime enables teams to move quickly, systems to be resilient, and resources to be used efficiently.

## Related Documentation

- [Database Architecture](./database-architecture.md) - Database structure at domain level
- [Persistence](../principles/persistance.md) - Data storage strategies

