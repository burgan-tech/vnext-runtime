# VNext Runtime - Local Development Environment

[![TR](https://img.shields.io/badge/🇹🇷-Türkçe-red)](README.tr.md) [![EN](https://img.shields.io/badge/🇺🇸-English-blue)](README.md)

This project is designed to enable developers to set up and run the VNext Runtime system in their local environments for development purposes. This Docker-based setup includes all dependencies and quickly prepares the development environment.

> **⚠️ Important Note:** Until the deployment versioning method is finalized, system components should be reset and reinstalled locally with each version transition.

> **Languages:** This README is available in [English](README.en.md) | [Türkçe](README.md)

## Environment Configuration

The repository includes ready-made environment files (`.env`, `.env.orchestration`, `.env.execution`) in the `vnext/docker/` directory. These files control system versions, database connections, Redis configuration, telemetry settings, and other runtime parameters.

**Purpose:** You can customize these environment files to match your infrastructure and development needs. All available environment variables and their default values can be found in the respective files within the repository.

## 🎯 Domain Configuration (Important!)

**Domain configuration is a critical concept** in vNext Runtime. Each developer must configure their own domain to work with the platform. To set up your domain, update the `APP_DOMAIN` value in the following files:

1. **`vnext/docker/.env`** - Runtime domain configuration
2. **`vnext/docker/.env.orchestration`** - Orchestration service domain
3. **`vnext/docker/.env.execution`** - Execution service domain
4. **`vnext.config.json`** - Project domain configuration (in your own workflow repository)

```bash
# Example: Change from default "core" to your domain
APP_DOMAIN=my-company
```

This ensures all workflow components, tasks, and system resources are properly scoped to your domain namespace.

## Quick Start

### Easy Setup with Makefile (Recommended)

The Makefile in the project provides the most comfortable running environment for developers. The system checks environment files and starts the development environment with a single command:

```bash
# Check environment files and start development environment
make dev

# Start lightweight development environment (without monitoring/analytics tools)
make dev-lightweight

# Display help menu
make help

# Setup network and check environment
make setup
```

### 🪶 Lightweight Mode

For resource-constrained environments or when you only need core functionality, use **lightweight mode**. This mode excludes heavy monitoring and analytics tools:

**Excluded Services:**
- Prometheus (Metrics collection)
- Grafana (Metrics visualization)
- Metabase (BI Analytics)
- ClickHouse (Analytics database)
- PgAdmin (PostgreSQL GUI)
- Redis Insight (Redis GUI)

**Included Services:**
- VNext Orchestration & Execution services
- PostgreSQL, Redis, Vault
- DAPR runtime components
- OpenObserve & OpenTelemetry Collector
- Mockoon API mock server

**Usage:**

```bash
# Start in lightweight mode
make dev-lightweight

# Or start services directly
make up-lightweight

# Start with rebuild
make up-build-lightweight

# Stop lightweight services
make down-lightweight

# Restart lightweight services
make restart-lightweight

# View lightweight services status
make status-lightweight

# View lightweight services logs
make logs-lightweight
```

**Benefits:**
- ⚡ Faster startup time
- 💾 Lower memory usage (~2GB vs ~4GB)
- 🚀 Lighter resource footprint
- 🎯 Focus on core workflow development

### Manual Setup

If you don't want to use Makefile, you can set up manually:

#### 1. Check Environment Files

Ensure the `.env`, `.env.orchestration`, and `.env.execution` files exist in the `vnext/docker/` directory and customize them as needed.

#### 2. Create Docker Network

```bash
docker network create vnext-development
```

#### 3. Start Services

```bash
# Navigate to vnext/docker directory
cd vnext/docker

# Start all services in background
docker-compose up -d

# Follow logs
docker-compose logs -f vnext-app

# Restart a specific service
docker-compose restart vnext-app
```

#### 4. Check System Status

```bash
# Display running services status
docker-compose ps

# vnext-app health check
curl http://localhost:4201/health
```

## 🚀 Getting Started with vNext Development

To develop workflows and components for vNext Runtime, you'll need the following tools:

### 1. vNext Template

**Repository:** https://github.com/burgan-tech/vnext-template

A structured template package for vNext workflow components with domain-based architecture. This template creates a complete project structure with built-in validation and build capabilities.

**Installation & Usage:**

```bash
# Create a new vNext project with your domain name
npx @burgan-tech/vnext-template YOUR_DOMAIN_NAME

# Example
npx @burgan-tech/vnext-template user-management
```

This will create a new directory with your domain name containing the following structure:

```
YOUR_DOMAIN_NAME/
├── Extensions/    # Custom extension definitions
├── Functions/     # Custom function definitions
├── Schemas/       # JSON schema definitions
├── Tasks/         # Task definitions
├── Views/         # View components
└── Workflows/     # Workflow definitions
```

**Available Scripts:**

| Script | Description |
|--------|-------------|
| `npm run validate` | Validate project structure and schemas |
| `npm run build` | Build runtime package to dist/ |
| `npm run build:runtime` | Build runtime package explicitly |
| `npm run build:reference` | Build reference package with exports only |

**Install Specific Version:**

```bash
npx @burgan-tech/vnext-template@<version> YOUR_DOMAIN_NAME
```

For detailed documentation, visit the [vnext-template repository](https://github.com/burgan-tech/vnext-template).

### 2. vNext Flow Studio

**Repository:** https://github.com/burgan-tech/vnext-flow-studio

A powerful Visual Studio Code extension for visual workflow design and management.

**Features:**
- 🎨 Visual workflow design interface
- 📦 Manage workflows and components visually
- 🚀 Deploy workflows directly from VS Code
- 🔍 IntelliSense and validation support

**Installation:**
1. Open VS Code
2. Search for "vNext Flow Studio" in Extensions
3. Install and start designing your workflows visually

For detailed usage instructions, visit the [vnext-flow-studio repository](https://github.com/burgan-tech/vnext-flow-studio).

### 3. vNext Schema

**Repository:** https://github.com/burgan-tech/vnext-schema

Contains JSON schemas for all supported vNext components (workflows, tasks, functions, etc.).

**Purpose:**
- 📚 Learn about available components and their properties
- 🤖 Integrate with AI tools for schema validation
- ✅ Ensure your workflows conform to platform standards

Reference the [vnext-schema repository](https://github.com/burgan-tech/vnext-schema) to understand component structures and validation rules.

---

## VNext Core Runtime Initialization

The `vnext-core-init` service automatically runs after the vnext-app service becomes healthy and performs the following operations:

1. Downloads the `@burgan-tech/vnext-core-runtime` npm package (version controlled via `.env` file)
2. Reads system components from the core folder within the package:
   - Extensions
   - Functions
   - Schemas
   - Tasks
   - Views
   - Workflows
3. **Merges custom components** (if mounted volume is available)
4. **🆕 Domain Replacement**: Replaces all `"domain"` property values in JSON files with the `APP_DOMAIN` environment variable value
   - This allows each developer to work with their own domain locally
   - Default domain is `"core"`, but can be customized via `APP_DOMAIN=mydomain` in `.env` file
   - Applies to both core system components and custom components
5. Sends merged and domain-updated components as POST requests to the `vnext-app/api/admin` endpoint

This way, the vnext-app application becomes ready with both system and custom components.

## Custom Components

You can add your own custom components by mounting a volume to the `vnext-core-init` container.

### Setup

1. Create a custom components directory with the following structure:
   ```
   vnext/docker/custom-components/
   ├── Extensions/    # Custom extension definitions
   ├── Functions/     # Custom function definitions  
   ├── Schemas/       # Custom JSON schema definitions
   ├── Tasks/         # Custom task definitions
   ├── Views/         # Custom view components
   └── Workflows/     # Custom workflow definitions
   ```

2. Set the `CUSTOM_COMPONENTS_PATH` environment variable in the `.env` file:
   ```bash
   CUSTOM_COMPONENTS_PATH=./vnext/docker/custom-components
   ```

3. If not set, it defaults to `./vnext/docker/custom-components` relative to the docker-compose.yml file.

### How Custom Components Work

- **Merging**: When a custom component has the same filename as a core component, their `data` arrays are merged
- **Custom-only**: Components that don't exist in core are uploaded as standalone components
- **JSON Schema**: Each component must follow the same JSON schema format as core components

See `vnext/docker/custom-components/README.md` for detailed documentation and examples.

## Instance Filtering

VNext Runtime provides powerful filtering capabilities for querying workflow instances based on their JSON attributes. This feature allows you to search and filter instances using various operators through simple API calls.

### Basic Usage

Filter instances using query parameters in your HTTP requests:

```bash
# Find instances where clientId equals "122"
curl -X GET "http://localhost:4201/api/v1.0/{domain}/workflows/{workflow}/instances?filter=attributes=clientId=eq:122"

# Find instances where testValue is greater than 2
curl -X GET "http://localhost:4201/api/v1.0/{domain}/workflows/{workflow}/instances?filter=attributes=testValue=gt:2"

# Find instances where status is not "completed"
curl -X GET "http://localhost:4201/api/v1.0/{domain}/workflows/{workflow}/instances?filter=attributes=status=ne:completed"
```

### Filter Syntax

The filtering uses the format: `filter=attributes={field}={operator}:{value}`

#### Available Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal to | `filter=attributes=clientId=eq:122` |
| `ne` | Not equal to | `filter=attributes=status=ne:inactive` |
| `gt` | Greater than | `filter=attributes=amount=gt:100` |
| `ge` | Greater than or equal | `filter=attributes=score=ge:80` |
| `lt` | Less than | `filter=attributes=count=lt:10` |
| `le` | Less than or equal | `filter=attributes=age=le:65` |
| `between` | Between two values | `filter=attributes=amount=between:50,200` |
| `like` | Contains substring | `filter=attributes=name=like:john` |
| `startswith` | Starts with | `filter=attributes=email=startswith:test` |
| `endswith` | Ends with | `filter=attributes=email=endswith:.com` |
| `in` | Value in list | `filter=attributes=status=in:active,pending` |
| `nin` | Value not in list | `filter=attributes=type=nin:test,debug` |

### Practical Examples

#### Single Filter Examples

```bash
# Find all active orders
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=status=eq:active"

# Find high-value transactions
curl "http://localhost:4201/api/v1.0/finance/workflows/payment/instances?filter=attributes=amount=gt:1000"

# Find recent orders (assuming timestamp field)
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=createdDate=ge:2024-01-01"

# Search by customer email domain
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/customer/instances?filter=attributes=email=endswith:@company.com"
```

#### Multiple Filter Examples

```bash
# Combine multiple filters (AND logic)
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=status=eq:pending&filter=attributes=priority=eq:high"

# Find orders within price range
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=totalAmount=between:100,500"

# Find specific customer types
curl "http://localhost:4201/api/v1.0/crm/workflows/customer/instances?filter=attributes=customerType=in:premium,vip"
```

### Sample Instance Data

When working with workflow instances, you might have JSON data like:

```json
{
  "clientId": "122",
  "testValue": 4,
  "status": "active",
  "email": "customer@example.com",
  "amount": 150.50,
  "priority": "high",
  "tags": ["vip", "premium"]
}
```

### Filter Testing with cURL

```bash
# Test basic equality filter
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=clientId=eq:122"

# Test numeric comparison
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=amount=gt:100"

# Test string operations
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=email=endswith:.com"

# Test multiple filters
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=status=eq:active&filter=attributes=priority=eq:high"
```

### Pagination with Filters

```bash
# Filter with pagination
curl "http://localhost:4201/api/v1.0/ecommerce/workflows/order-processing/instances?filter=attributes=status=eq:active&page=1&pageSize=10"

# Large dataset filtering with pagination
curl "http://localhost:4201/api/v1.0/analytics/workflows/events/instances?filter=attributes=eventType=eq:purchase&page=1&pageSize=50"
```

### Response Format

Filtered results return in the standard format:

```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "flow": "order-processing",
      "flowVersion": "1.0.0",
      "domain": "ecommerce",
      "key": "ORDER-2024-001",
      "attributes": {
        "clientId": "122",
        "amount": 150.50,
        "status": "active"
      },
      "etag": "abc123def456"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

### Common Use Cases

1. **Customer Service**: Find all orders for a specific customer
2. **Financial Reporting**: Filter transactions by amount ranges
3. **Order Management**: Find pending or failed orders
4. **User Analytics**: Filter users by registration date or activity
5. **Error Monitoring**: Find instances with error status

### cURL Examples for Testing

You can use these cURL commands for testing filtering capabilities:

```bash
# Test basic equality filter
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=clientId=eq:122"

# Test numeric comparison  
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=testValue=gt:2"

# Test string operations
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=status=startswith:act"

# Test multiple filters
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=status=eq:active&filter=attributes=priority=ne:low"

# Test range filtering
curl -X GET "http://localhost:4201/api/v1.0/test/workflows/sample/instances?filter=attributes=amount=between:100,500"
```

This filtering system provides high-performance querying capabilities optimized for production workloads, making it easy to find specific workflow instances based on their business data.

## Makefile Commands

The Makefile located in the project root directory contains many commands that facilitate the development process. To see all commands:

```bash
make help
```

### Basic Commands

| Command | Description | Usage |
|---------|-------------|-------|
| `make help` | Lists all available commands | `make help` |
| `make dev` | Sets up and starts development environment | `make dev` |
| `make setup` | Checks environment files and creates network | `make setup` |
| `make info` | Shows project information and access URLs | `make info` |

### Environment Setup

| Command | Description | Usage |
|---------|-------------|-------|
| `make check-env` | Checks existence of environment files | `make check-env` |
| `make create-network` | Creates Docker network | `make create-network` |

### Docker Operations

| Command | Description | Usage |
|---------|-------------|-------|
| `make up` | Starts services | `make up` |
| `make up-lightweight` | Starts services (lightweight mode) | `make up-lightweight` |
| `make up-build` | Starts services with build | `make up-build` |
| `make up-build-lightweight` | Starts services with build (lightweight) | `make up-build-lightweight` |
| `make down` | Stops services | `make down` |
| `make down-lightweight` | Stops services (lightweight mode) | `make down-lightweight` |
| `make restart` | Restarts services | `make restart` |
| `make restart-lightweight` | Restarts services (lightweight mode) | `make restart-lightweight` |
| `make build` | Builds Docker images | `make build` |
| `make build-lightweight` | Builds Docker images (lightweight mode) | `make build-lightweight` |

### Service Management

| Command | Description | Usage |
|---------|-------------|-------|
| `make status` | Shows service status | `make status` |
| `make status-lightweight` | Shows service status (lightweight mode) | `make status-lightweight` |
| `make health` | Checks service health | `make health` |
| `make logs` | Shows logs for all services | `make logs` |
| `make logs-lightweight` | Shows logs for all services (lightweight) | `make logs-lightweight` |
| `make logs-orchestration` | Shows only orchestration service logs | `make logs-orchestration` |
| `make logs-execution` | Shows only execution service logs | `make logs-execution` |
| `make logs-init` | Shows core init service logs | `make logs-init` |
| `make logs-dapr` | Shows DAPR service logs | `make logs-dapr` |
| `make logs-db` | Shows database service logs | `make logs-db` |
| `make logs-monitoring` | Shows monitoring service logs | `make logs-monitoring` |
| `make logs-prometheus` | Shows Prometheus service logs | `make logs-prometheus` |
| `make logs-grafana` | Shows Grafana service logs | `make logs-grafana` |

### Development Tools

| Command | Description | Usage |
|---------|-------------|-------|
| `make shell-orchestration` | Opens shell in orchestration container | `make shell-orchestration` |
| `make shell-execution` | Opens shell in execution container | `make shell-execution` |
| `make shell-postgres` | Opens PostgreSQL shell | `make shell-postgres` |
| `make shell-redis` | Opens Redis CLI | `make shell-redis` |

### Monitoring

| Command | Description | Usage |
|---------|-------------|-------|
| `make ps` | Lists running containers | `make ps` |
| `make top` | Shows container resource usage | `make top` |
| `make stats` | Shows container statistics | `make stats` |
| `make monitoring-up` | Start only monitoring services (Prometheus & Grafana) | `make monitoring-up` |
| `make monitoring-down` | Stop monitoring services | `make monitoring-down` |
| `make monitoring-restart` | Restart monitoring services | `make monitoring-restart` |
| `make monitoring-status` | Show status of monitoring services | `make monitoring-status` |
| `make logs-monitoring` | Show logs for monitoring services | `make logs-monitoring` |
| `make logs-prometheus` | Show logs for Prometheus service | `make logs-prometheus` |
| `make logs-grafana` | Show logs for Grafana service | `make logs-grafana` |
| `make prometheus-config-reload` | Reload Prometheus configuration | `make prometheus-config-reload` |
| `make grafana-reset-password` | Reset Grafana admin password to 'admin' | `make grafana-reset-password` |

### Custom Components

| Command | Description | Usage |
|---------|-------------|-------|
| `make init-custom-components` | Creates custom components directory structure | `make init-custom-components` |
| `make reload-components` | Reloads custom components | `make reload-components` |

### Maintenance

| Command | Description | Usage |
|---------|-------------|-------|
| `make clean` | Removes stopped containers and unused networks | `make clean` |
| `make clean-all` | ⚠️ Removes ALL containers, images and volumes | `make clean-all` |
| `make reset` | Resets environment (stop, clean, setup) | `make reset` |
| `make update` | Pulls latest images and restarts services | `make update` |

### Common Usage Scenarios

```bash
# Running project for the first time
make dev

# Running project in lightweight mode (recommended for development)
make dev-lightweight

# Following logs only
make logs-orchestration
make logs-lightweight  # All logs in lightweight mode

# Checking service status
make status
make status-lightweight  # Status in lightweight mode
make health

# Restarting during development
make restart
make restart-lightweight  # Restart in lightweight mode

# Reloading after adding custom components
make reload-components

# Cleanup and reinstall
make reset
make dev
# or for lightweight
make down-lightweight
make dev-lightweight

# Container access
make shell-orchestration
make shell-postgres

# Monitoring specific operations (not available in lightweight mode)
make monitoring-up          # Start only monitoring services
make logs-monitoring        # Monitor Prometheus & Grafana logs
make monitoring-status      # Check monitoring service status
make prometheus-config-reload  # Reload Prometheus config
make grafana-reset-password    # Reset Grafana password
```

## Services and Ports

| Service | Description | Port | Access URL | Lightweight Mode |
|---------|-------------|------|------------|------------------|
| **vnext-app** | Main orchestration application | 4201 | http://localhost:4201 | ✅ Available |
| **vnext-execution-app** | Execution service application | 4202 | http://localhost:4202 | ✅ Available |
| **vnext-core-init** | Init container that loads system components | - | - | ✅ Available |
| **vnext-orchestration-dapr** | Dapr sidecar for orchestration service | 42110/42111 | - | ✅ Available |
| **vnext-execution-dapr** | Dapr sidecar for execution service | 43110/43111 | - | ✅ Available |
| **dapr-placement** | Dapr placement service | 50005 | - | ✅ Available |
| **dapr-scheduler** | Dapr scheduler service | 50007 | - | ✅ Available |
| **vnext-redis** | Redis cache | 6379 | - | ✅ Available |
| **vnext-postgres** | PostgreSQL database | 5432 | - | ✅ Available |
| **vnext-vault** | HashiCorp Vault (optional) | 8200 | http://localhost:8200 | ✅ Available |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 | ✅ Available |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - | ✅ Available |
| **mockoon** | API Mock Server | 3001 | http://localhost:3001 | ✅ Available |
| **prometheus** | Metrics collection and storage | 9090 | http://localhost:9090 | ❌ Not included |
| **grafana** | Metrics visualization and dashboards | 3000 | http://localhost:3000 | ❌ Not included |
| **metabase** | BI Analytics Platform | 3002 | http://localhost:3002 | ❌ Not included |
| **clickhouse** | Analytics database | 8123, 9000 | http://localhost:8123 | ❌ Not included |

## Management Tools

| Tool | URL | Username | Password | Lightweight Mode |
|------|-----|----------|----------|------------------|
| **Redis Insight** | http://localhost:5501 | - | - | ❌ Not included |
| **PgAdmin** | http://localhost:5502 | info@info.com | admin | ❌ Not included |
| **OpenObserve** | http://localhost:5080 | root@example.com | Complexpass#@123 | ✅ Available |
| **Vault UI** | http://localhost:8200 | - | admin (token) | ✅ Available |
| **Prometheus** | http://localhost:9090 | - | - | ❌ Not included |
| **Grafana** | http://localhost:3000 | admin | admin | ❌ Not included |
| **Metabase** | http://localhost:3002 | - | - | ❌ Not included |

## Development Tips

### Customizing Environment Variables

To customize environment files:

```bash
# Check existing environment files
make check-env

# Edit .env files in vnext/docker/ directory as needed
```

Important configurations:

1. **Changing database connection**:
   ```bash
   # In vnext/docker/.env.orchestration file
   ConnectionStrings__Default=Host=my-postgres;Port=5432;Database=MyWorkflowDb;Username=myuser;Password=mypass;
   ```

2. **Changing Redis settings**:
   ```bash
   # In vnext/docker/.env.orchestration file
   Redis__Standalone__EndPoints__0=my-redis:6379
   Redis__Password=myredispassword
   ```

3. **Changing log level**:
   ```bash
   # In vnext/docker/.env.orchestration file
   Logging__LogLevel__Default=Debug
   Telemetry__Logging__MinimumLevel=Debug
   ```

### Debugging

With Makefile commands:

```bash
# Display all service logs
make logs

# Specific service logs
make logs-orchestration
make logs-execution
make logs-init

# Check service status
make status
make health

# Container access
make shell-orchestration
make shell-postgres
make shell-redis
```

Manual commands:

```bash
# From vnext/docker directory
cd vnext/docker

# Docker compose commands
docker-compose logs -f vnext-app
docker-compose exec vnext-app sh
docker-compose ps
```

### Common Issues and Solutions

1. **Port conflicts**: 
   ```bash
   # Reset with Makefile
   make reset
   # Change port numbers in .env files
   ```

2. **Memory insufficiency**: 
   - Increase memory limit in Docker Desktop (min 4GB recommended)
   - Check container resource usage: `make stats`

3. **Volume mount issues**: 
   ```bash
   # Create custom components directory
   make init-custom-components
   # Check and fix path
   ```

4. **Missing environment files**:
   ```bash
   # Environment check
   make check-env
   # Ensure files exist in vnext/docker/ directory
   ```

### Performance Tuning

```bash
# In .env.orchestration file
TaskFactory__UseObjectPooling=true
TaskFactory__MaxPoolSize=100
Redis__ConnectionTimeout=3000
```

Development workflow recommendations:

```bash
# Daily development routine
make dev              # Initial startup
make logs-orchestration  # Log monitoring
make restart          # Restart after changes
make health          # Health check

# Weekly cleanup
make clean           # Light cleanup
make reset           # Deep reset (if needed)
```

## 📚 Documentation

For comprehensive documentation about the VNext Runtime platform, workflows, and development guides, please refer to:

- **📖 [Complete Documentation (English)](doc/en/README.md)** - Comprehensive developer guide covering platform architecture, workflow components, and detailed API references
- **🇹🇷 [Türkçe Dokümantasyon](doc/tr/README.md)** - Platform mimarisi, iş akışı bileşenleri ve detaylı API referansları içeren kapsamlı geliştirici rehberi

### Quick Documentation Links

| Topic | English | Turkish |
|-------|---------|---------|
| **Platform Fundamentals** | [fundamentals/readme.md](doc/en/fundamentals/readme.md) | [fundamentals/readme.md](doc/tr/fundamentals/readme.md) |
| **Workflow States** | [flow/state.md](doc/en/flow/state.md) | [flow/state.md](doc/tr/flow/state.md) |
| **Task Types** | [flow/task.md](doc/en/flow/task.md) | [flow/task.md](doc/tr/flow/task.md) |
| **Mapping Guide** | [flow/mapping.md](doc/en/flow/mapping.md) | [flow/mapping.md](doc/tr/flow/mapping.md) |
| **How to Start Instance** | [how-to/start-instance.md](doc/en/how-to/start-instance.md) | [how-to/start-instance.md](doc/tr/how-to/start-instance.md) |

## 📊 Monitoring and Metrics

VNext Runtime includes comprehensive monitoring capabilities with Prometheus and Grafana integration for real-time system observability.

### 🚀 Quick Start for Monitoring

```bash
# Start monitoring services along with the main application
make dev

# Or start only monitoring services
cd vnext/docker
docker-compose up -d prometheus grafana
```

### 📈 Metrics Dashboard Access

- **Grafana Dashboard**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090

### 🎯 Available Metrics

#### Counter Metrics
- `workflow_state_transitions_total` - State transitions
- `workflow_errors_total` - Total errors by type/severity  
- `workflow_exceptions_total` - Unhandled exceptions
- `workflow_validation_failures_total` - Validation failures
- `http_requests_total` - HTTP requests
- `workflow_db_queries_total` - Database queries
- `script_executions_total` - Script executions
- `background_jobs_scheduled_total` - Background jobs
- `external_service_calls_total` - External service calls
- `dapr_service_invocations_total` - DAPR invocations

#### Gauge Metrics
- `workflow_health_status` - System health (0=unhealthy, 1=healthy)
- `workflow_error_rate` - Current error rate %
- `workflow_instances_by_status` - Instance count by status
- `task_factory_pool_size` - Object pool metrics
- `workflow_cache_size_bytes` - Cache size
- `background_jobs_pending` - Pending job count

#### Histogram Metrics
- `workflow_state_duration_seconds` - Time in each state
- `workflow_db_query_duration_seconds` - Database query time
- `http_request_duration_seconds` - HTTP request time
- `background_job_duration_seconds` - Job execution time
- `script_execution_duration_seconds` - Script execution time
- `external_service_duration_seconds` - External call time

### 📊 Dashboard Features

#### System Health Overview
- Overall System Health Status (Healthy/Unhealthy)
- Overall Error Rate (%)
- Real-time Error Rate by Type/Severity

#### Workflow State Metrics
- State Transitions (per minute)
- Instance Status Distribution (pie chart)
- State Duration P95 (seconds)

#### Database Metrics
- Database Queries by Type/Table (per minute)
- Query Duration P95/P50

#### HTTP API Metrics
- HTTP Requests by Endpoint/Status (per minute)
- Request Duration P95
- HTTP Errors by Type

#### Background Jobs & Script Engine
- Background Jobs Status (Pending/Running)
- Script Executions by Type/Language

#### Cache & External Services
- Cache Hit/Miss Rates
- External Service Calls by Status
- DAPR Integration Metrics

### 📈 Metrics Endpoints

Workflow applications expose metrics at the following endpoints:
- **Orchestration API**: http://vnext-app:5000/metrics
- **Execution API**: http://vnext-execution-app:5000/metrics

### 🔧 Configuration Files

#### Prometheus Configuration
- `vnext/docker/config/prometheus/prometheus.yml` - Prometheus scraping configuration

#### Grafana Configuration
- `vnext/docker/config/grafana/provisioning/datasources/` - Auto-configured Prometheus datasource
- `vnext/docker/config/grafana/provisioning/dashboards/` - Dashboard provisioning
- `vnext/docker/config/grafana/dashboards/workflow-metrics.json` - Main workflow dashboard

### 🛠 Troubleshooting Monitoring

#### Grafana Dashboard Not Showing?
1. Check if containers are running:
   ```bash
   docker ps | grep -E "(grafana|prometheus)"
   ```

2. Check Prometheus targets:
   - Visit http://localhost:9090/targets

#### Metrics Not Coming?
1. Check workflow application `/metrics` endpoint
2. Verify Prometheus configuration targets are correct
3. Check network connectivity

### 📝 Customizing Dashboards

To customize the dashboard:
1. Edit in Grafana UI
2. Export in JSON format
3. Update `vnext/docker/config/grafana/dashboards/workflow-metrics.json`
