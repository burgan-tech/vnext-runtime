# VNext Runtime Makefile
# This Makefile provides convenient commands for managing the VNext Runtime application

# Container Runtime Detection (OrbStack, Docker, Docker v2, or Podman)
CONTAINER_RUNTIME := $(shell \
	if command -v orb >/dev/null 2>&1; then \
		echo "docker"; \
	elif command -v docker >/dev/null 2>&1; then \
		echo "docker"; \
	elif command -v podman >/dev/null 2>&1; then \
		echo "podman"; \
	else \
		echo "none"; \
	fi)

# Detect if OrbStack is being used
IS_ORBSTACK := $(shell command -v orb >/dev/null 2>&1 && echo "yes" || echo "no")

# Compose Command Detection
COMPOSE_CMD := $(shell \
	if command -v orb >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then \
		echo "docker compose"; \
	elif docker compose version >/dev/null 2>&1; then \
		echo "docker compose"; \
	elif command -v docker-compose >/dev/null 2>&1; then \
		echo "docker-compose"; \
	elif command -v podman-compose >/dev/null 2>&1; then \
		echo "podman-compose"; \
	elif command -v podman >/dev/null 2>&1 && podman compose version >/dev/null 2>&1; then \
		echo "podman compose"; \
	else \
		echo "none"; \
	fi)

# Variables
DOCKER_COMPOSE_FILE = vnext/docker/docker-compose.yml
DOCKER_DIR = vnext/docker
ENV_FILE = $(DOCKER_DIR)/.env
ENV_ORCHESTRATION_FILE = $(DOCKER_DIR)/.env.orchestration
ENV_EXECUTION_FILE = $(DOCKER_DIR)/.env.execution
NETWORK_NAME = vnext-development

# Default target
.DEFAULT_GOAL := help

# Colors for output
RED = \033[0;31m
GREEN = \033[0;32m
YELLOW = \033[1;33m
BLUE = \033[0;34m
PURPLE = \033[0;35m
NC = \033[0m # No Color

##@ Help
help: ## Display this help message
	@echo "$(BLUE)VNext Runtime Management$(NC)"
	@echo "=========================="
	@echo "$(PURPLE)Container Runtime:$(NC) $(CONTAINER_RUNTIME)$(if $(filter yes,$(IS_ORBSTACK)), (OrbStack),)"
	@echo "$(PURPLE)Compose Command:$(NC) $(COMPOSE_CMD)"
	@echo ""
	@awk 'BEGIN {FS = ":.*##"; printf "\nUsage:\n  make \033[36m<target>\033[0m\n"} /^[a-zA-Z_0-9-]+:.*?##/ { printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2 } /^##@/ { printf "\n\033[1m%s\033[0m\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

check-runtime: ## Validate container runtime availability
	@if [ "$(CONTAINER_RUNTIME)" = "none" ]; then \
		echo "$(RED)❌ No container runtime detected!$(NC)"; \
		echo "$(YELLOW)Please install one of:$(NC)"; \
		echo "  • OrbStack: https://orbstack.dev"; \
		echo "  • Docker: https://docs.docker.com/get-docker/"; \
		echo "  • Podman: https://podman.io/getting-started/installation"; \
		exit 1; \
	fi
	@if [ "$(COMPOSE_CMD)" = "none" ]; then \
		echo "$(RED)❌ No compose command detected!$(NC)"; \
		echo "$(YELLOW)Please install docker-compose or podman-compose$(NC)"; \
		exit 1; \
	fi
	@if [ "$(IS_ORBSTACK)" = "yes" ]; then \
		echo "$(GREEN)✅ OrbStack detected and ready$(NC)"; \
	elif [ "$(CONTAINER_RUNTIME)" = "docker" ]; then \
		echo "$(GREEN)✅ Docker detected and ready$(NC)"; \
	else \
		echo "$(GREEN)✅ Podman detected and ready$(NC)"; \
	fi
	@echo "$(GREEN)✅ Compose command: $(COMPOSE_CMD)$(NC)"

##@ Environment Setup
setup: ## Setup environment files and network
	@echo "$(YELLOW)Setting up VNext Runtime environment...$(NC)"
	@$(MAKE) create-env-files
	@$(MAKE) create-network
	@echo "$(GREEN)Environment setup completed!$(NC)"

create-env-files: ## Create environment files from templates
	@echo "$(YELLOW)Creating environment files...$(NC)"
	@if [ ! -f $(ENV_FILE) ]; then \
		echo "# VNext Core Runtime Version" > $(ENV_FILE); \
		echo "VNEXT_CORE_VERSION=latest" >> $(ENV_FILE); \
		echo "" >> $(ENV_FILE); \
		echo "# Component Configuration" >> $(ENV_FILE); \
		echo "VNEXT_COMPONENT_VERSION=latest" >> $(ENV_FILE); \
		echo "APP_DOMAIN=core" >> $(ENV_FILE); \
		echo "" >> $(ENV_FILE); \
		echo "# Docker Image Versions" >> $(ENV_FILE); \
		echo "VNEXT_VERSION=latest" >> $(ENV_FILE); \
		echo "DAPR_RUNTIME_VERSION=latest" >> $(ENV_FILE); \
		echo "DAPR_PLACEMENT_VERSION=latest" >> $(ENV_FILE); \
		echo "DAPR_SCHEDULER_VERSION=latest" >> $(ENV_FILE); \
		echo "REDIS_VERSION=latest" >> $(ENV_FILE); \
		echo "POSTGRES_VERSION=latest" >> $(ENV_FILE); \
		echo "VAULT_VERSION=1.13.3" >> $(ENV_FILE); \
		echo "ALPINE_CURL_VERSION=latest" >> $(ENV_FILE); \
		echo "OPENOBSERVE_VERSION=latest" >> $(ENV_FILE); \
		echo "OTEL_COLLECTOR_VERSION=latest" >> $(ENV_FILE); \
		echo "MOCKOON_VERSION=latest" >> $(ENV_FILE); \
		echo "$(GREEN)Created $(ENV_FILE)$(NC)"; \
	else \
		echo "$(YELLOW)$(ENV_FILE) already exists$(NC)"; \
	fi
	@if [ ! -f $(ENV_ORCHESTRATION_FILE) ]; then \
		echo "# VNext Orchestration Environment Variables" > $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Application Settings" >> $(ENV_ORCHESTRATION_FILE); \
		echo "APP_NAME=vnext-app" >> $(ENV_ORCHESTRATION_FILE); \
		echo "APP_PORT=5000" >> $(ENV_ORCHESTRATION_FILE); \
		echo "APP_HOST=0.0.0.0" >> $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Database Configuration" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DATABASE_URL=postgresql://postgres:postgres@vnext-postgres:5432/vnext_orchestration" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DATABASE_HOST=vnext-postgres" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DATABASE_PORT=5432" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DATABASE_USER=postgres" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DATABASE_PASSWORD=postgres" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DATABASE_NAME=vnext_orchestration" >> $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Redis Configuration" >> $(ENV_ORCHESTRATION_FILE); \
		echo "REDIS_HOST=vnext-redis" >> $(ENV_ORCHESTRATION_FILE); \
		echo "REDIS_PORT=6379" >> $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Vault Configuration" >> $(ENV_ORCHESTRATION_FILE); \
		echo "VAULT_URL=http://vnext-vault:8200" >> $(ENV_ORCHESTRATION_FILE); \
		echo "VAULT_TOKEN=admin" >> $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Dapr Configuration" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DAPR_HTTP_PORT=42110" >> $(ENV_ORCHESTRATION_FILE); \
		echo "DAPR_GRPC_PORT=42111" >> $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Logging" >> $(ENV_ORCHESTRATION_FILE); \
		echo "LOG_LEVEL=info" >> $(ENV_ORCHESTRATION_FILE); \
		echo "" >> $(ENV_ORCHESTRATION_FILE); \
		echo "# Environment" >> $(ENV_ORCHESTRATION_FILE); \
		echo "NODE_ENV=development" >> $(ENV_ORCHESTRATION_FILE); \
		echo "$(GREEN)Created $(ENV_ORCHESTRATION_FILE)$(NC)"; \
	else \
		echo "$(YELLOW)$(ENV_ORCHESTRATION_FILE) already exists$(NC)"; \
	fi
	@if [ ! -f $(ENV_EXECUTION_FILE) ]; then \
		echo "# VNext Execution Environment Variables" > $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Application Settings" >> $(ENV_EXECUTION_FILE); \
		echo "APP_NAME=vnext-execution-app" >> $(ENV_EXECUTION_FILE); \
		echo "APP_PORT=5000" >> $(ENV_EXECUTION_FILE); \
		echo "APP_HOST=0.0.0.0" >> $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Database Configuration" >> $(ENV_EXECUTION_FILE); \
		echo "DATABASE_URL=postgresql://postgres:postgres@vnext-postgres:5432/vnext_execution" >> $(ENV_EXECUTION_FILE); \
		echo "DATABASE_HOST=vnext-postgres" >> $(ENV_EXECUTION_FILE); \
		echo "DATABASE_PORT=5432" >> $(ENV_EXECUTION_FILE); \
		echo "DATABASE_USER=postgres" >> $(ENV_EXECUTION_FILE); \
		echo "DATABASE_PASSWORD=postgres" >> $(ENV_EXECUTION_FILE); \
		echo "DATABASE_NAME=vnext_execution" >> $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Redis Configuration" >> $(ENV_EXECUTION_FILE); \
		echo "REDIS_HOST=vnext-redis" >> $(ENV_EXECUTION_FILE); \
		echo "REDIS_PORT=6379" >> $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Vault Configuration" >> $(ENV_EXECUTION_FILE); \
		echo "VAULT_URL=http://vnext-vault:8200" >> $(ENV_EXECUTION_FILE); \
		echo "VAULT_TOKEN=admin" >> $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Dapr Configuration" >> $(ENV_EXECUTION_FILE); \
		echo "DAPR_HTTP_PORT=43110" >> $(ENV_EXECUTION_FILE); \
		echo "DAPR_GRPC_PORT=43111" >> $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Logging" >> $(ENV_EXECUTION_FILE); \
		echo "LOG_LEVEL=info" >> $(ENV_EXECUTION_FILE); \
		echo "" >> $(ENV_EXECUTION_FILE); \
		echo "# Environment" >> $(ENV_EXECUTION_FILE); \
		echo "NODE_ENV=development" >> $(ENV_EXECUTION_FILE); \
		echo "$(GREEN)Created $(ENV_EXECUTION_FILE)$(NC)"; \
	else \
		echo "$(YELLOW)$(ENV_EXECUTION_FILE) already exists$(NC)"; \
	fi

create-network: check-runtime ## Create container network
	@echo "$(YELLOW)Creating container network: $(NETWORK_NAME)...$(NC)"
	@$(CONTAINER_RUNTIME) network inspect $(NETWORK_NAME) >/dev/null 2>&1 || \
	($(CONTAINER_RUNTIME) network create $(NETWORK_NAME) && echo "$(GREEN)Network $(NETWORK_NAME) created$(NC)") || \
	echo "$(YELLOW)Network $(NETWORK_NAME) already exists$(NC)"

check-env: ## Check if environment files exist
	@echo "$(YELLOW)Checking environment files...$(NC)"
	@if [ ! -f $(ENV_FILE) ]; then \
		echo "$(RED)❌ $(ENV_FILE) not found$(NC)"; \
		echo "$(YELLOW)Run 'make create-env-files' to create it$(NC)"; \
		exit 1; \
	else \
		echo "$(GREEN)✅ $(ENV_FILE) found$(NC)"; \
	fi
	@if [ ! -f $(ENV_ORCHESTRATION_FILE) ]; then \
		echo "$(RED)❌ $(ENV_ORCHESTRATION_FILE) not found$(NC)"; \
		echo "$(YELLOW)Run 'make create-env-files' to create it$(NC)"; \
		exit 1; \
	else \
		echo "$(GREEN)✅ $(ENV_ORCHESTRATION_FILE) found$(NC)"; \
	fi
	@if [ ! -f $(ENV_EXECUTION_FILE) ]; then \
		echo "$(RED)❌ $(ENV_EXECUTION_FILE) not found$(NC)"; \
		echo "$(YELLOW)Run 'make create-env-files' to create it$(NC)"; \
		exit 1; \
	else \
		echo "$(GREEN)✅ $(ENV_EXECUTION_FILE) found$(NC)"; \
	fi

##@ Container Operations
build: check-env check-runtime ## Build container images
	@echo "$(YELLOW)Building container images...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) build
	@echo "$(GREEN)Build completed!$(NC)"

up: check-env check-runtime ## Start all services
	@echo "$(YELLOW)Starting VNext Runtime services...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) up -d
	@echo "$(GREEN)Services started!$(NC)"
	@$(MAKE) status

start: up-build ## Start services with build

up-build: check-env check-runtime ## Start services with build
	@echo "$(YELLOW)Starting VNext Runtime services with build...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) up -d --build
	@echo "$(GREEN)Services started!$(NC)"
	@$(MAKE) status

down: check-runtime ## Stop all services
	@echo "$(YELLOW)Stopping VNext Runtime services...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) down
	@echo "$(GREEN)Services stopped!$(NC)"

stop: down ## Alias for 'down'

restart: ## Restart all services
	@echo "$(YELLOW)Restarting VNext Runtime services...$(NC)"
	@$(MAKE) down
	@$(MAKE) up
	@echo "$(GREEN)Services restarted!$(NC)"

##@ Service Management
status: check-runtime ## Show status of all services
	@echo "$(BLUE)VNext Runtime Services Status:$(NC)"
	@echo "================================="
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) ps

logs: check-runtime ## Show logs for all services
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) logs -f

logs-orchestration: check-runtime ## Show logs for orchestration service
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) logs -f vnext-app

logs-execution: check-runtime ## Show logs for execution service
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) logs -f vnext-execution-app

logs-init: check-runtime ## Show logs for init service
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) logs -f vnext-init

logs-dapr: check-runtime ## Show logs for DAPR services
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) logs -f vnext-orchestration-dapr vnext-execution-dapr

logs-db: check-runtime ## Show logs for database services
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) logs -f postgres redis

health: ## Check health of services
	@echo "$(BLUE)Service Health Check:$(NC)"
	@echo "===================="
	@echo "$(YELLOW)VNext Orchestration:$(NC)"
	@curl -s http://localhost:4201/health || echo "$(RED)❌ Orchestration service not healthy$(NC)"
	@echo ""
	@echo "$(YELLOW)VNext Execution:$(NC)"
	@curl -s http://localhost:4202/health || echo "$(RED)❌ Execution service not healthy$(NC)"
	@echo ""
	@echo "$(YELLOW)Management Interfaces:$(NC)"
	@echo "• Vault: http://localhost:8200"
	@echo "• OpenObserve: http://localhost:5080"

##@ Database Operations
db-create: check-runtime ## Create vNext database in running postgres container
	@echo "$(YELLOW)Creating vNext database...$(NC)"
	@cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec -T postgres psql -U postgres -c "SELECT 1 FROM pg_database WHERE datname = 'vNext_WorkflowDb'" | grep -q 1 && \
		echo "$(YELLOW)Database vNext_WorkflowDb already exists$(NC)" || \
		($(COMPOSE_CMD) exec -T postgres psql -U postgres -f /docker-entrypoint-initdb.d/init-db.sql && \
		echo "$(GREEN)Database vNext_WorkflowDb created successfully!$(NC)")

db-drop: check-runtime ## Drop vNext database (WARNING: Destructive)
	@echo "$(RED)WARNING: This will drop the vNext_WorkflowDb database!$(NC)"
	@echo "$(YELLOW)Press Ctrl+C to cancel, or wait 5 seconds to continue...$(NC)"
	@sleep 5
	@cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec -T postgres psql -U postgres -c "DROP DATABASE IF EXISTS \"vNext_WorkflowDb\";"
	@echo "$(GREEN)Database dropped!$(NC)"

db-reset: db-drop db-create ## Reset vNext database (drop and recreate)

db-status: check-runtime ## Check database status and list databases
	@echo "$(BLUE)PostgreSQL Database Status:$(NC)"
	@echo "=========================="
	@cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec -T postgres psql -U postgres -c "\l" 2>/dev/null || echo "$(RED)❌ PostgreSQL is not running$(NC)"

db-connect: check-runtime ## Connect to vNext database via psql
	@cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec postgres psql -U postgres -d "vNext_WorkflowDb"

##@ Development
dev: ## Start development environment
	@echo "$(YELLOW)Starting development environment...$(NC)"
	@$(MAKE) setup
	@$(MAKE) up-build
	@$(MAKE) health

shell-orchestration: check-runtime ## Open shell in orchestration container
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec vnext-app sh

shell-execution: check-runtime ## Open shell in execution container
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec vnext-execution-app sh

shell-postgres: check-runtime ## Open PostgreSQL shell
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec postgres psql -U postgres

shell-redis: check-runtime ## Open Redis CLI
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) exec redis redis-cli

##@ Maintenance
clean: check-runtime ## Remove stopped containers and unused networks
	@echo "$(YELLOW)Cleaning up container resources...$(NC)"
	$(CONTAINER_RUNTIME) container prune -f
	$(CONTAINER_RUNTIME) network prune -f
	@echo "$(GREEN)Cleanup completed!$(NC)"

clean-all: check-runtime ## Remove all containers, images, and volumes (WARNING: Destructive)
	@echo "$(RED)WARNING: This will remove ALL containers, images, and volumes!$(NC)"
	@echo "$(YELLOW)Press Ctrl+C to cancel, or wait 10 seconds to continue...$(NC)"
	@sleep 10
	@$(MAKE) down
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) down -v --rmi all
	$(CONTAINER_RUNTIME) system prune -a -f
	@echo "$(GREEN)Complete cleanup finished!$(NC)"

reset: ## Reset environment (stop, clean, and setup)
	@echo "$(YELLOW)Resetting VNext Runtime environment...$(NC)"
	@$(MAKE) down
	@$(MAKE) clean
	@$(MAKE) setup
	@echo "$(GREEN)Environment reset completed!$(NC)"

update: check-runtime ## Pull latest images and restart
	@echo "$(YELLOW)Updating VNext Runtime images...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) pull
	@$(MAKE) restart
	@echo "$(GREEN)Update completed!$(NC)"

##@ Monitoring
ps: check-runtime ## Show running containers
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) ps

top: check-runtime ## Show container resource usage
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) top

stats: check-runtime ## Show container statistics
	$(CONTAINER_RUNTIME) stats $(shell cd $(DOCKER_DIR) && $(COMPOSE_CMD) ps -q)

##@ Custom Components
publish-component: check-env ## Publish component package (waits for vnext-init to be healthy)
	@echo "$(YELLOW)Publishing component package...$(NC)"
	@cd $(DOCKER_DIR) && ./publish-component.sh
	@echo "$(GREEN)Component published!$(NC)"

publish-component-skip-health: check-env ## Publish component package (skip health check)
	@echo "$(YELLOW)Publishing component package (skipping health check)...$(NC)"
	@cd $(DOCKER_DIR) && ./publish-component.sh --skip-health
	@echo "$(GREEN)Component published!$(NC)"

republish-component: check-runtime ## Re-run component publisher container
	@echo "$(YELLOW)Re-publishing component via container...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) rm -f vnext-component-publisher
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) up vnext-component-publisher
	@echo "$(GREEN)Component re-published!$(NC)"

##@ Domain Configuration
change-domain: ## Change domain for all services (usage: make change-domain DOMAIN=mydomain)
	@if [ -z "$(DOMAIN)" ]; then \
		echo "$(RED)❌ DOMAIN parameter is required!$(NC)"; \
		echo "$(YELLOW)Usage: make change-domain DOMAIN=mydomain$(NC)"; \
		exit 1; \
	fi
	@echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"
	@echo "$(BLUE)     VNext Domain Configuration Change$(NC)"
	@echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"
	@echo ""
	@echo "$(YELLOW)📌 New Domain: $(DOMAIN)$(NC)"
	@# Normalize domain for database name (replace non-alphanumeric with underscore, capitalize first letter of each word)
	@NORMALIZED=$$(echo "$(DOMAIN)" | sed 's/[^a-zA-Z0-9]/_/g' | awk '{for(i=1;i<=NF;i++){$$i=toupper(substr($$i,1,1)) tolower(substr($$i,2))}}1' FS='_' OFS='_'); \
	DB_NAME="vNext_$${NORMALIZED}"; \
	echo "$(YELLOW)📌 Database Name: $${DB_NAME}$(NC)"; \
	echo ""; \
	echo "$(PURPLE)Updating environment files...$(NC)"; \
	echo "────────────────────────────────────────────────────────────"; \
	for envfile in $(DOCKER_DIR)/.env $(DOCKER_DIR)/.env.orchestration $(DOCKER_DIR)/.env.execution $(DOCKER_DIR)/.env.inbox $(DOCKER_DIR)/.env.outbox; do \
		if [ -f "$$envfile" ]; then \
			if grep -q "^APP_DOMAIN=" "$$envfile"; then \
				sed -i.bak 's/^APP_DOMAIN=.*/APP_DOMAIN=$(DOMAIN)/' "$$envfile" && rm -f "$$envfile.bak"; \
				echo "$(GREEN)  ✅ Updated APP_DOMAIN in $$envfile$(NC)"; \
			else \
				echo "APP_DOMAIN=$(DOMAIN)" >> "$$envfile"; \
				echo "$(GREEN)  ✅ Added APP_DOMAIN to $$envfile$(NC)"; \
			fi \
		else \
			echo "$(YELLOW)  ⚠️  File not found: $$envfile (skipped)$(NC)"; \
		fi \
	done; \
	echo ""; \
	echo "$(PURPLE)Updating appsettings files (ConnectionStrings:Default)...$(NC)"; \
	echo "────────────────────────────────────────────────────────────"; \
	for appsettings in $(DOCKER_DIR)/appsettings.Development.json $(DOCKER_DIR)/appsettings.WorkerInbox.Development.json $(DOCKER_DIR)/appsettings.WorkerOutbox.Development.json; do \
		if [ -f "$$appsettings" ]; then \
			sed -i.bak "s/Database=vNext_[^;]*/Database=$${DB_NAME}/" "$$appsettings" && rm -f "$$appsettings.bak"; \
			echo "$(GREEN)  ✅ Updated database name in $$appsettings$(NC)"; \
		else \
			echo "$(YELLOW)  ⚠️  File not found: $$appsettings (skipped)$(NC)"; \
		fi \
	done; \
	echo ""; \
	echo "$(PURPLE)Updating PostgreSQL init script...$(NC)"; \
	echo "────────────────────────────────────────────────────────────"; \
	INIT_SQL="$(DOCKER_DIR)/config/postgres/init-db.sql"; \
	if [ -f "$$INIT_SQL" ]; then \
		sed -i.bak "s/vNext_[a-zA-Z0-9_]*/$${DB_NAME}/g" "$$INIT_SQL" && rm -f "$$INIT_SQL.bak"; \
		echo "$(GREEN)  ✅ Updated database name in $$INIT_SQL$(NC)"; \
	else \
		echo "$(RED)  ❌ File not found: $$INIT_SQL$(NC)"; \
	fi; \
	echo ""; \
	echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"; \
	echo "$(GREEN)✅ Domain configuration completed!$(NC)"; \
	echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"; \
	echo ""; \
	echo "$(YELLOW)📋 Summary of Changes:$(NC)"; \
	echo "  • APP_DOMAIN set to: $(DOMAIN)"; \
	echo "  • Database name set to: $${DB_NAME}"; \
	echo ""; \
	echo "$(YELLOW)⚠️  Important: You need to reset your environment for changes to take effect:$(NC)"; \
	echo ""; \
	echo "  $(PURPLE)1. Stop all services:$(NC)"; \
	echo "     make down"; \
	echo ""; \
	echo "  $(PURPLE)2. Reset database (WARNING: This will delete all data!):$(NC)"; \
	echo "     make db-reset"; \
	echo ""; \
	echo "  $(PURPLE)3. Start fresh environment:$(NC)"; \
	echo "     make dev"; \
	echo ""; \
	echo "$(BLUE)════════════════════════════════════════════════════════════$(NC)"

##@ Git Operations
git-init: ## Initialize git repository
	@if [ ! -d .git ]; then \
		echo "$(YELLOW)Initializing git repository...$(NC)"; \
		git init; \
		git add .; \
		git commit -m "Initial commit: VNext Runtime setup"; \
		echo "$(GREEN)Git repository initialized!$(NC)"; \
	else \
		echo "$(YELLOW)Git repository already exists$(NC)"; \
	fi

##@ Information
info: ## Show project information
	@echo "$(BLUE)VNext Runtime Project Information$(NC)"
	@echo "=================================="
	@echo "$(YELLOW)Project:$(NC) VNext Runtime"
	@echo "$(YELLOW)Container Runtime:$(NC) $(CONTAINER_RUNTIME)$(if $(filter yes,$(IS_ORBSTACK)), (OrbStack),)"
	@echo "$(YELLOW)Compose Command:$(NC) $(COMPOSE_CMD)"
	@echo "$(YELLOW)Docker Compose:$(NC) $(DOCKER_COMPOSE_FILE)"
	@echo "$(YELLOW)Network:$(NC) $(NETWORK_NAME)"
	@echo ""
	@echo "$(BLUE)Services:$(NC)"
	@echo "• VNext Orchestration: http://localhost:4201"
	@echo "• VNext Execution: http://localhost:4202"
	@echo ""
	@echo "$(BLUE)Management Interfaces:$(NC)"
	@echo "• Vault: http://localhost:8200 (admin)"
	@echo "• OpenObserve: http://localhost:5080 (root@example.com / Complexpass#@123)"
	@echo ""
	@echo "$(BLUE)Quick Commands:$(NC)"
	@echo "• make dev      - Start development environment"
	@echo "• make logs     - View all service logs"
	@echo "• make health   - Check service health"
	@echo "• make status   - Show service status"

version: ## Show version information
	@echo "$(BLUE)Version Information:$(NC)"
	@echo "==================="
	@echo "$(YELLOW)Make:$(NC) $(shell make --version | head -n 1)"
	@if [ "$(IS_ORBSTACK)" = "yes" ]; then \
		echo "$(YELLOW)Container Runtime:$(NC) OrbStack (using Docker CLI)"; \
		echo "$(YELLOW)OrbStack:$(NC) $(shell orb version 2>/dev/null || echo 'N/A')"; \
	else \
		echo "$(YELLOW)Container Runtime:$(NC) $(CONTAINER_RUNTIME)"; \
	fi
	@echo "$(YELLOW)$(CONTAINER_RUNTIME):$(NC) $(shell $(CONTAINER_RUNTIME) --version 2>/dev/null || echo 'N/A')"
	@if [ "$(COMPOSE_CMD)" = "docker compose" ]; then \
		echo "$(YELLOW)Compose Version:$(NC) $(shell docker compose version 2>/dev/null || echo 'N/A')"; \
	elif [ "$(COMPOSE_CMD)" = "docker-compose" ]; then \
		echo "$(YELLOW)Compose Version:$(NC) $(shell docker-compose --version 2>/dev/null || echo 'N/A')"; \
	elif [ "$(COMPOSE_CMD)" = "podman-compose" ]; then \
		echo "$(YELLOW)Compose Version:$(NC) $(shell podman-compose --version 2>/dev/null || echo 'N/A')"; \
	elif [ "$(COMPOSE_CMD)" = "podman compose" ]; then \
		echo "$(YELLOW)Compose Version:$(NC) $(shell podman compose version 2>/dev/null || echo 'N/A')"; \
	fi

# Prevent make from interpreting file names as targets
.PHONY: help check-runtime setup create-env-files create-network check-env build up start up-build down stop restart status logs logs-orchestration logs-execution logs-init logs-dapr logs-db health dev shell-orchestration shell-execution shell-postgres shell-redis clean clean-all reset update ps top stats publish-component publish-component-skip-health republish-component git-init info version db-create db-drop db-reset db-status db-connect change-domain
