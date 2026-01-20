# VNext Runtime - Local Development Environment

[![TR](https://img.shields.io/badge/🇹🇷-Türkçe-red)](README.tr.md) [![EN](https://img.shields.io/badge/🇺🇸-English-blue)](README.md)

This project is designed to enable developers to set up and run the VNext Runtime system in their local environments for development purposes. This Docker-based setup includes all dependencies and quickly prepares the development environment.

> **⚠️ Important Note:** Until the deployment versioning method is finalized, system components should be reset and reinstalled locally with each version transition.

> **Languages:** This README is available in [English](README.en.md) | [Türkçe](README.md)

## Environment Configuration

The repository includes template files in the `vnext/docker/templates/` directory that are used to generate domain-specific configurations. When you create a domain using `make create-domain`, these templates are processed and the resulting configuration files are placed in `vnext/docker/domains/<domain_name>/`.

**Template Files:**
- `.env` - Main environment with versions and ports
- `.env.orchestration` - Orchestration service configuration
- `.env.execution` - Execution service configuration
- `.env.worker-inbox` - Worker inbox service configuration
- `.env.worker-outbox` - Worker outbox service configuration
- `appsettings.*.Development.json` - Application settings

**Purpose:** You can customize template files in `vnext/docker/templates/` to change default values for all new domains, or edit domain-specific files in `vnext/docker/domains/<domain_name>/` for individual domain customization.

## 🎯 Multi-Domain Support (New!)

**VNext Runtime now supports running multiple domains simultaneously** on the same infrastructure. This allows teams to run isolated domain environments (e.g., `core`, `sales`, `hr`) sharing the same PostgreSQL, Redis, Vault, and Dapr services.

### Folder Structure

```
vnext/docker/
├── templates/                              # Template files for new domains
│   ├── .env                                # Main env template with {{PLACEHOLDERS}}
│   ├── .env.orchestration                  # Orchestration template
│   ├── .env.execution                      # Execution template
│   ├── .env.worker-inbox                   # Inbox worker template
│   ├── .env.worker-outbox                  # Outbox worker template
│   └── appsettings.*.Development.json      # App settings templates
├── domains/                                # Domain configurations (created per domain)
│   ├── core/                               # Domain: core
│   ├── sales/                              # Domain: sales
│   └── <domain_name>/                      # Your domain
│       ├── .env
│       ├── .env.orchestration
│       ├── .env.execution
│       ├── .env.worker-inbox
│       ├── .env.worker-outbox
│       └── appsettings.*.Development.json
├── config/                                 # Shared infrastructure config
├── docker-compose.yml
└── create-domain.sh                        # Script to create domains from templates
```

### Creating a New Domain

Use the `create-domain` command to generate domain configuration from templates:

```bash
# Create domain with port offset (to avoid port conflicts)
make create-domain DOMAIN=core PORT_OFFSET=0
make create-domain DOMAIN=sales PORT_OFFSET=10
make create-domain DOMAIN=hr PORT_OFFSET=20
```

The database name is automatically generated from your domain name:
- `core` → `vNext_Core`
- `sales` → `vNext_Sales`
- `user-management` → `vNext_User_Management`

### Port Allocation

Each domain uses unique ports based on the `PORT_OFFSET`:

| Offset | App Port | Execution | Inbox | Outbox | Init |
|--------|----------|-----------|-------|--------|------|
| 0      | 4201     | 4202      | 4203  | 4204   | 3005 |
| 10     | 4211     | 4212      | 4213  | 4214   | 3015 |
| 20     | 4221     | 4222      | 4223  | 4224   | 3025 |

Dapr ports use `offset * 100` to avoid conflicts.

### Running Multiple Domains

```bash
# 1. Start shared infrastructure
make up-infra

# 2. Create and start first domain
make create-domain DOMAIN=core PORT_OFFSET=0
make db-create-domain DOMAIN=core
make up-vnext DOMAIN=core

# 3. Create and start second domain
make create-domain DOMAIN=sales PORT_OFFSET=10
make db-create-domain DOMAIN=sales
make up-vnext DOMAIN=sales

# 4. View all running services
make status-all-domains

# 5. Check health of a specific domain
make health DOMAIN=core
make health DOMAIN=sales
```

### Managing Domains

```bash
# List all configured domains
make list-domains

# Stop a specific domain
make down-vnext DOMAIN=sales

# Restart a specific domain
make restart-vnext DOMAIN=sales

# Stop all domains but keep infrastructure
make down-all-vnext

# View logs for a specific domain
make logs-vnext DOMAIN=core
```

### Customizing Templates

Templates are located in `vnext/docker/templates/`. You can customize them to change default values. Templates use `{{PLACEHOLDER}}` syntax:

| Placeholder | Description |
|-------------|-------------|
| `{{DOMAIN_NAME}}` | Domain name (e.g., `core`, `sales`) |
| `{{PORT_OFFSET}}` | Port offset value |
| `{{DB_NAME}}` | Database name (e.g., `vNext_Core`) |
| `{{VNEXT_APP_PORT}}` | Orchestration port |
| `{{DAPR_*_PORT}}` | Dapr sidecar ports |

### Legacy Single Domain Mode

For backward compatibility, the legacy `change-domain` command is still available:

```bash
make change-domain DOMAIN=my-company
```

This updates all domain-related settings but doesn't support running multiple domains simultaneously.

## Quick Start

### Easy Setup with Makefile (Recommended)

The Makefile in the project provides the most comfortable running environment for developers. The system checks environment files and starts the development environment with a single command:

```bash
# Check environment files and start development environment
make dev

# Display help menu
make help

# Setup network and check environment
make setup
```

### What `make dev` Does

When you run `make dev`, the following happens automatically:

1. ✅ **Environment Setup** - Creates `.env` files and Docker network
2. ✅ **PostgreSQL** starts → `vNext_WorkflowDb` database is automatically created
3. ✅ **vnext-app** starts → after postgres is healthy
4. ✅ **vnext-init** starts → after vnext-app is healthy
5. ✅ **vnext-component-publisher** runs → automatically publishes components after vnext-init is healthy
6. ✅ All other services start

This means with a single command, you get:
- Database ready with schema
- Components loaded
- All infrastructure running

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

The `vnext-init` service automatically runs after the vnext-app service becomes healthy and performs the following operations:

1. Downloads the `@burgan-tech/vnext-core-runtime` npm package (version controlled via `.env` file)
2. Reads system components from the core folder within the package:
   - Extensions
   - Functions
   - Schemas
   - Tasks
   - Views
   - Workflows
3. **🆕 Domain Replacement**: Replaces all `"domain"` property values in JSON files with the `APP_DOMAIN` environment variable value
   - This allows each developer to work with their own domain locally
   - Default domain is `"core"`, but can be customized via `APP_DOMAIN=mydomain` in `.env` file
4. Sends merged and domain-updated components as POST requests to the `vnext-app/api/admin` endpoint

## Automatic Database Initialization

When the Docker Compose starts, PostgreSQL automatically creates the `vNext_WorkflowDb` database using an init script. This ensures:

- Database is ready before any service tries to connect
- Services depending on postgres wait until the database is healthy
- No manual database creation needed

### Database Commands

```bash
# Check database status
make db-status

# Manually create database (if needed)
make db-create

# Drop and recreate database
make db-reset

# Connect to database via psql
make db-connect
```

## Automatic Component Publishing

The `vnext-component-publisher` service automatically runs after `vnext-init` becomes healthy:

1. Waits for vnext-init to be ready
2. Publishes components using the configured version and domain
3. Completes and exits

To manually republish components:

```bash
# Re-run the component publisher
make republish-component

# Or use the script directly
make publish-component
```

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

### Multi-Domain Management

| Command | Description | Usage |
|---------|-------------|-------|
| `make create-domain` | Create domain from templates | `make create-domain DOMAIN=mydom PORT_OFFSET=10` |
| `make list-domains` | List all configured domains | `make list-domains` |
| `make up-vnext` | Start vnext services for a domain | `make up-vnext DOMAIN=mydom` |
| `make down-vnext` | Stop vnext services for a domain | `make down-vnext DOMAIN=mydom` |
| `make restart-vnext` | Restart vnext services for a domain | `make restart-vnext DOMAIN=mydom` |
| `make status-vnext` | Show status for a domain | `make status-vnext DOMAIN=mydom` |
| `make logs-vnext` | Show logs for a domain | `make logs-vnext DOMAIN=mydom` |
| `make status-all-domains` | Show all running vnext services | `make status-all-domains` |
| `make down-all-vnext` | Stop all domain services (keep infra) | `make down-all-vnext` |
| `make db-create-domain` | Create database for a domain | `make db-create-domain DOMAIN=mydom` |
| `make health` | Check health (with optional DOMAIN) | `make health DOMAIN=mydom` |

### Infrastructure Management

| Command | Description | Usage |
|---------|-------------|-------|
| `make up-infra` | Start only infrastructure services | `make up-infra` |
| `make down-infra` | Stop only infrastructure services | `make down-infra` |
| `make status-infra` | Show infrastructure status | `make status-infra` |
| `make logs-infra` | Show infrastructure logs | `make logs-infra` |

### Legacy Domain Configuration

| Command | Description | Usage |
|---------|-------------|-------|
| `make change-domain` | Change domain (legacy single-domain mode) | `make change-domain DOMAIN=mydomain` |

### Environment Setup

| Command | Description | Usage |
|---------|-------------|-------|
| `make check-env` | Checks existence of environment files | `make check-env` |
| `make create-network` | Creates Docker network | `make create-network` |

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
| `make logs-init` | Shows init service logs | `make logs-init` |
| `make logs-dapr` | Shows DAPR service logs | `make logs-dapr` |
| `make logs-db` | Shows database service logs | `make logs-db` |

### Database Operations

| Command | Description | Usage |
|---------|-------------|-------|
| `make db-status` | Shows database status and lists databases | `make db-status` |
| `make db-create` | Creates vNext database | `make db-create` |
| `make db-drop` | Drops vNext database (destructive!) | `make db-drop` |
| `make db-reset` | Drops and recreates database | `make db-reset` |
| `make db-connect` | Connects to database via psql | `make db-connect` |

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

### Custom Components

| Command | Description | Usage |
|---------|-------------|-------|
| `make publish-component` | Publishes component package | `make publish-component` |
| `make republish-component` | Re-runs component publisher container | `make republish-component` |

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

# Database operations
make db-status
make db-reset

# Restarting during development
make restart

# Reloading after adding custom components
make reload-components

# Re-publishing components
make republish-component

# Cleanup and reinstall
make reset
make dev

# Container access
make shell-orchestration
make shell-postgres
```

## Services and Ports

### Infrastructure Services (Shared)

| Service | Description | Port | Access URL |
|---------|-------------|------|------------|
| **dapr-placement** | Dapr placement service | 50005 | - |
| **dapr-scheduler** | Dapr scheduler service | 50007 | - |
| **vnext-redis** | Redis cache | 6379 | - |
| **vnext-postgres** | PostgreSQL database | 5432 | - |
| **vnext-vault** | HashiCorp Vault | 8200 | http://localhost:8200 |
| **openobserve** | Observability dashboard | 5080 | http://localhost:5080 |
| **otel-collector** | OpenTelemetry Collector | 4317, 4318, 8888 | - |
| **mockoon** | API Mock Server | 3001 | http://localhost:3001 |

### VNext Domain Services (Per Domain)

Ports vary by domain based on `PORT_OFFSET`. Default (offset 0):

| Service | Description | Port | Container Name |
|---------|-------------|------|----------------|
| **vnext-app** | Orchestration application | 4201 | vnext-app-{domain} |
| **vnext-execution-app** | Execution service | 4202 | vnext-execution-app-{domain} |
| **vnext-worker-inbox** | Worker inbox service | 4203 | vnext-worker-inbox-{domain} |
| **vnext-worker-outbox** | Worker outbox service | 4204 | vnext-worker-outbox-{domain} |
| **vnext-init** | Init container | 3005 | vnext-init-{domain} |
| **vnext-orchestration-dapr** | Dapr sidecar for orchestration | 42110/42111 | vnext-orchestration-dapr-{domain} |
| **vnext-execution-dapr** | Dapr sidecar for execution | 43110/43111 | vnext-execution-dapr-{domain} |

For domains with `PORT_OFFSET=10`, ports become 4211, 4212, 4213, 4214, 3015, etc.

## Management Tools

| Tool | URL | Username | Password |
|------|-----|----------|----------|
| **OpenObserve** | http://localhost:5080 | root@example.com | Complexpass#@123 |
| **Vault UI** | http://localhost:8200 | - | admin (token) |

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

3. **Missing environment files**:
   ```bash
   # Environment check
   make check-env
   # Ensure files exist in vnext/docker/ directory
   ```

4. **Database not created**:
   ```bash
   # Check database status
   make db-status
   # Manually create if needed
   make db-create
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
