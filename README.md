# VNext Runtime - Local Development Environment

[![TR](https://img.shields.io/badge/🇹🇷-Türkçe-red)](README.tr.md) [![EN](https://img.shields.io/badge/🇺🇸-English-blue)](README.md)

This project is designed to enable developers to set up and run the VNext Runtime system in their local environments for development purposes. This Docker-based setup includes all dependencies and quickly prepares the development environment.

> **Languages:** This README is available in [English](README.en.md) | [Türkçe](README.md)

## Required Files

To run the system, you need to create the following environment files:

### .env (Main Environment Variables)
```bash
# VNext Core Runtime Version
VNEXT_CORE_VERSION=latest

# Custom Components Path (optional)
CUSTOM_COMPONENTS_PATH=./vnext/docker/custom-components

# Docker Image Versions (optional - you can override default values)
VNEXT_ORCHESTRATOR_VERSION=0.0.6
VNEXT_EXECUTION_VERSION=0.0.6
DAPR_RUNTIME_VERSION=latest
```

### .env.orchestration
```bash
# VNext Orchestration Environment Variables
# These values override the AppSettings configuration of vnext-app service

# Application Settings
ApplicationName=vnext
APP_PORT=4201
APP_HOST=0.0.0.0

# Database Configuration (ConnectionStrings:Default)
ConnectionStrings__Default=Host=vnext-postgres;Port=5432;Database=Aether_WorkflowDb;Username=postgres;Password=postgres;

# Redis Configuration
Redis__Standalone__EndPoints__0=vnext-redis:6379
Redis__InstanceName=workflow-api
Redis__ConnectionTimeout=5000
Redis__DefaultDatabase=0
Redis__Password=
Redis__Ssl=false

# vNext API Configuration
vNextApi__BaseUrl=http://localhost:4201
vNextApi__ApiVersion=1
vNextApi__TimeoutSeconds=30
vNextApi__MaxRetryAttempts=3
vNextApi__RetryDelayMilliseconds=1000

# Telemetry Configuration
Telemetry__ServiceName=vNext-orchestration
Telemetry__ServiceVersion=1.0.0
Telemetry__Environment=Development
Telemetry__Otlp__Endpoint=http://otel-collector:4318

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning
Telemetry__Logging__MinimumLevel=Information

# Execution Service
ExecutionService__AppId=vnext-execution-app

# Vault Configuration
Vault__Enabled=false

# Dapr Configuration
DAPR_HTTP_PORT=42110
DAPR_GRPC_PORT=42111
```

### .env.execution
```bash
# VNext Execution Environment Variables
# Note: Currently commented out in docker-compose.yml

# Application Settings
ApplicationName=vnext-execution
APP_PORT=5000
APP_HOST=0.0.0.0

# Database Configuration
ConnectionStrings__Default=Host=vnext-postgres;Port=5432;Database=Aether_ExecutionDb;Username=postgres;Password=postgres;

# Redis Configuration
Redis__Standalone__EndPoints__0=vnext-redis:6379
Redis__InstanceName=execution-api
Redis__ConnectionTimeout=5000
Redis__DefaultDatabase=1

# Telemetry Configuration
Telemetry__ServiceName=vNext-execution
Telemetry__ServiceVersion=1.0.0
Telemetry__Environment=Development
Telemetry__Otlp__Endpoint=http://otel-collector:4318

# Dapr Configuration
DAPR_HTTP_PORT=43110
DAPR_GRPC_PORT=43111
```

## Supported Environment Variables

The following table shows environment variables derived from AppSettings configuration that you can use in `.env.orchestration` / `.env.execution` files:

### Basic Application Settings
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `ApplicationName` | Application name | `vnext` |
| `APP_HOST` | Host for the application to listen on | `0.0.0.0` |
| `APP_PORT` | Port for the application to listen on | `4201` |

### Database Configuration
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `ConnectionStrings__Default` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=Aether_WorkflowDb;Username=postgres;Password=postgres;` |

### Redis Configuration
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `Redis__Standalone__EndPoints__0` | Redis endpoint | `localhost:6379` |
| `Redis__InstanceName` | Redis instance name | `workflow-api` |
| `Redis__ConnectionTimeout` | Connection timeout (ms) | `5000` |
| `Redis__DefaultDatabase` | Default database index | `0` |
| `Redis__Password` | Redis password | `` |
| `Redis__Ssl` | SSL usage | `false` |

### vNext API Configuration
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `vNextApi__BaseUrl` | API base URL | `http://localhost:4201` |
| `vNextApi__ApiVersion` | API version | `1` |
| `vNextApi__TimeoutSeconds` | Request timeout | `30` |
| `vNextApi__MaxRetryAttempts` | Maximum retry attempts | `3` |
| `vNextApi__RetryDelayMilliseconds` | Retry delay | `1000` |

### Telemetry and Logging
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `Telemetry__ServiceName` | Telemetry service name | `vNext-orchestration` |
| `Telemetry__ServiceVersion` | Service version | `1.0.0` |
| `Telemetry__Environment` | Environment name | `Development` |
| `Telemetry__Otlp__Endpoint` | OpenTelemetry endpoint | `http://localhost:4318` |
| `Logging__LogLevel__Default` | Default log level | `Information` |
| `Logging__LogLevel__Microsoft.AspNetCore` | ASP.NET Core log level | `Warning` |
| `Telemetry__Logging__MinimumLevel` | Minimum telemetry log level | `Information` |

### Task Factory
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `TaskFactory__UseObjectPooling` | Object pooling usage | `false` |
| `TaskFactory__MaxPoolSize` | Maximum pool size | `50` |
| `TaskFactory__InitialPoolSize` | Initial pool size | `5` |
| `TaskFactory__EnableMetrics` | Enable metrics | `true` |

### Other Services
| Environment Variable | Description | Default Value |
|---------------------|-------------|---------------|
| `ExecutionService__AppId` | Execution service app ID | `vnext-execution-app` |
| `Vault__Enabled` | Vault usage | `false` |
| `ResponseCompression__Enable` | Response compression | `true` |

## Quick Start

### Easy Setup with Makefile (Recommended)

The Makefile in the project provides the most comfortable running environment for developers. You can handle all complex operations with a single command:

```bash
# Set up and start development environment with one command
make dev

# Display help menu
make help

# Create only environment files
make setup
```

### Manual Setup

If you don't want to use Makefile, you can set up manually:

#### 1. Create Required Files

Create the `.env`, `.env.orchestration`, and `.env.execution` files mentioned above in the `vnext/docker/` directory.

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
4. Sends merged components as POST requests to the `vnext-app/api/admin` endpoint

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

### Example: E-Commerce Workflow

We provide a complete e-commerce workflow example that demonstrates the full capabilities of the VNext Runtime system:

- **HTTP Test File**: `vnext/docker/custom-components/ecommerce-workflow.http` - Ready-to-use HTTP requests for testing
- **Documentation**: 
  - 🇺🇸 [English Guide](vnext/docker/custom-components/README-ecommerce-workflow-en.md)
  - 🇹🇷 [Turkish Guide](vnext/docker/custom-components/README-ecommerce-workflow-tr.md)
- **Features Demonstrated**:
  - State-based workflow management
  - Authentication flow
  - Product browsing and selection
  - Cart management
  - Order processing
  - Error handling and retry mechanisms

This example provides a practical starting point for understanding how to implement complex business workflows using VNext Runtime.

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
| `make setup` | Creates environment files and network | `make setup` |
| `make info` | Shows project information and access URLs | `make info` |

### Environment Setup

| Command | Description | Usage |
|---------|-------------|-------|
| `make create-env-files` | Creates environment files | `make create-env-files` |
| `make create-network` | Creates Docker network | `make create-network` |
| `make check-env` | Checks existence of environment files | `make check-env` |

### Docker Operations

| Command | Description | Usage |
|---------|-------------|-------|
| `make up` | Starts services | `make up` |
| `make up-build` | Starts services with build | `make up-build` |
| `make down` | Stops services | `make down` |
| `make restart` | Restarts services | `make restart` |
| `make build` | Builds Docker images | `make build` |

### Service Management

| Command | Description | Usage |
|---------|-------------|-------|
| `make status` | Shows service status | `make status` |
| `make health` | Checks service health | `make health` |
| `make logs` | Shows logs for all services | `make logs` |
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

# Following logs only
make logs-orchestration

# Checking service status
make status
make health

# Restarting during development
make restart

# Reloading after adding custom components
make reload-components

# Cleanup and reinstall
make reset
make dev

# Container access
make shell-orchestration
make shell-postgres

# Monitoring specific operations
make monitoring-up          # Start only monitoring services
make logs-monitoring        # Monitor Prometheus & Grafana logs
make monitoring-status      # Check monitoring service status
make prometheus-config-reload  # Reload Prometheus config
make grafana-reset-password    # Reset Grafana password
```

## Services and Ports

| Service | Description | Port | Access URL |
|---------|-------------|------|------------|
| **vnext-app** | Main orchestration application | 4201 | http://localhost:4201 |
| **vnext-execution-app** | Execution service application | 4202 | http://localhost:4202 |
| **vnext-core-init** | Init container that loads system components | - | - |
| **vnext-orchestration-dapr** | Dapr sidecar for orchestration service | 42110/42111 | - |
| **vnext-execution-dapr** | Dapr sidecar for execution service | 43110/43111 | - |
| **dapr-placement** | Dapr placement service | 50005 | - |
| **dapr-scheduler** | Dapr scheduler service | 50007 | - |
| **vnext-redis** | Redis cache | 6379 | - |
| **vnext-postgres** | PostgreSQL database | 5432 | - |
| **vnext-vault** | HashiCorp Vault (optional) | 8200 | http://localhost:8200 |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - |
| **prometheus** | Metrics collection and storage | 9090 | http://localhost:9090 |
| **grafana** | Metrics visualization and dashboards | 3000 | http://localhost:3000 |

## Management Tools

| Tool | URL | Username | Password |
|------|-----|----------|----------|
| **Redis Insight** | http://localhost:5501 | - | - |
| **PgAdmin** | http://localhost:5502 | info@info.com | admin |
| **OpenObserve** | http://localhost:5080 | root@example.com | Complexpass#@123 |
| **Vault UI** | http://localhost:8200 | - | admin (token) |
| **Prometheus** | http://localhost:9090 | - | - |
| **Grafana** | http://localhost:3000 | admin | admin |

## Development Tips

### Customizing Environment Variables

To create environment files:

```bash
# Automatic creation with Makefile (recommended)
make create-env-files

# Manually create .env files in vnext/docker/ directory
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
   # Create files
   make create-env-files
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

For detailed monitoring setup and configuration, see [README-Grafana.md](README-Grafana.md).
