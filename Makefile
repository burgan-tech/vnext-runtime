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

# Multi-domain support
# DOMAIN: Name of the domain (default: core)
# PORT_OFFSET: Port offset for domain (auto-calculated if not specified)
DOMAIN ?= core
PORT_OFFSET ?= 0

# Domain-specific paths (files stored in domains/<domain_name>/)
DOMAINS_DIR = $(DOCKER_DIR)/domains
DOMAIN_DIR = $(DOMAINS_DIR)/$(DOMAIN)
DOMAIN_ENV_FILE = $(DOMAIN_DIR)/.env

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

create-env-files: ## Create infrastructure environment file and ensure templates/domains directories exist
	@echo "$(YELLOW)Creating environment files...$(NC)"
	@mkdir -p $(DOCKER_DIR)/templates
	@mkdir -p $(DOCKER_DIR)/domains
	@if [ ! -f $(ENV_FILE) ]; then \
		echo "# VNext Infrastructure Environment" > $(ENV_FILE); \
		echo "# This file contains shared infrastructure settings (versions, etc.)" >> $(ENV_FILE); \
		echo "# Domain-specific settings are in domains/<domain_name>/" >> $(ENV_FILE); \
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
	@echo "$(GREEN)✅ Templates directory: $(DOCKER_DIR)/templates$(NC)"
	@echo "$(GREEN)✅ Domains directory: $(DOCKER_DIR)/domains$(NC)"
	@echo ""
	@echo "$(YELLOW)To create a domain, run:$(NC)"
	@echo "  make create-domain DOMAIN=<name> PORT_OFFSET=<offset>"

create-network: check-runtime ## Create container network
	@echo "$(YELLOW)Creating container network: $(NETWORK_NAME)...$(NC)"
	@$(CONTAINER_RUNTIME) network inspect $(NETWORK_NAME) >/dev/null 2>&1 || \
	($(CONTAINER_RUNTIME) network create $(NETWORK_NAME) && echo "$(GREEN)Network $(NETWORK_NAME) created$(NC)") || \
	echo "$(YELLOW)Network $(NETWORK_NAME) already exists$(NC)"

check-env: ## Check if environment files exist
	@echo "$(YELLOW)Checking environment files...$(NC)"
	@if [ ! -f $(ENV_FILE) ]; then \
		echo "$(RED)❌ $(ENV_FILE) not found$(NC)"; \
		echo "$(YELLOW)Run 'make setup' to create it$(NC)"; \
		exit 1; \
	else \
		echo "$(GREEN)✅ $(ENV_FILE) found$(NC)"; \
	fi
	@if [ ! -d "$(DOMAINS_DIR)" ]; then \
		echo "$(YELLOW)⚠️  No domains directory. Run 'make create-domain DOMAIN=<name>' to create a domain$(NC)"; \
	else \
		echo "$(GREEN)✅ Domains directory exists$(NC)"; \
	fi

check-env-infra: ## Check if infrastructure environment file exists
	@if [ ! -f $(ENV_FILE) ]; then \
		echo "$(RED)❌ $(ENV_FILE) not found$(NC)"; \
		echo "$(YELLOW)Run 'make setup' to create it$(NC)"; \
		exit 1; \
	fi

##@ Container Operations
build: check-env check-runtime ## Build container images
	@echo "$(YELLOW)Building container images...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext build
	@echo "$(GREEN)Build completed!$(NC)"

up: check-env check-runtime ## Start all services (infra + vnext)
	@echo "$(YELLOW)Starting VNext Runtime services...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext up -d
	@echo "$(GREEN)Services started!$(NC)"
	@$(MAKE) status

up-infra: check-env-infra check-runtime ## Start only infrastructure services
	@echo "$(YELLOW)Starting infrastructure services...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra up -d
	@echo "$(GREEN)Infrastructure services started!$(NC)"
	@$(MAKE) status-infra

up-vnext: check-env-infra check-runtime ## Start vnext services for a domain (usage: make up-vnext DOMAIN=mydom)
	@if [ ! -d "$(DOMAIN_DIR)" ]; then \
		echo "$(RED)❌ Domain '$(DOMAIN)' not configured!$(NC)"; \
		echo "$(YELLOW)Run 'make create-domain DOMAIN=$(DOMAIN) PORT_OFFSET=<offset>' first$(NC)"; \
		exit 1; \
	fi
	@# Check if infrastructure is running
	@if ! $(CONTAINER_RUNTIME) ps --filter "name=vnext-postgres" --filter "status=running" -q | grep -q .; then \
		echo "$(YELLOW)Infrastructure not running. Starting infrastructure first...$(NC)"; \
		$(MAKE) up-infra; \
	fi
	@echo "$(YELLOW)Starting VNext services for domain: $(DOMAIN)...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && set -a && . ./domains/$(DOMAIN)/.env && set +a && \
		$(COMPOSE_CMD) -p vnext-$(DOMAIN) --env-file ./domains/$(DOMAIN)/.env --profile vnext up -d
	@echo "$(GREEN)VNext services for $(DOMAIN) started!$(NC)"
	@$(MAKE) status-vnext DOMAIN=$(DOMAIN)

start: up-build ## Start services with build

up-build: check-env check-runtime ## Start all services with build
	@echo "$(YELLOW)Starting VNext Runtime services with build...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext up -d --build
	@echo "$(GREEN)Services started!$(NC)"
	@$(MAKE) status

up-infra-build: check-env check-runtime ## Start infrastructure services with build
	@echo "$(YELLOW)Starting infrastructure services with build...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra up -d --build
	@echo "$(GREEN)Infrastructure services started!$(NC)"
	@$(MAKE) status-infra

up-vnext-build: check-env-infra check-runtime ## Start vnext services with build (usage: make up-vnext-build DOMAIN=mydom)
	@if [ ! -d "$(DOMAIN_DIR)" ]; then \
		echo "$(RED)❌ Domain '$(DOMAIN)' not configured!$(NC)"; \
		echo "$(YELLOW)Run 'make create-domain DOMAIN=$(DOMAIN) PORT_OFFSET=<offset>' first$(NC)"; \
		exit 1; \
	fi
	@# Check if infrastructure is running
	@if ! $(CONTAINER_RUNTIME) ps --filter "name=vnext-postgres" --filter "status=running" -q | grep -q .; then \
		echo "$(YELLOW)Infrastructure not running. Starting infrastructure first...$(NC)"; \
		$(MAKE) up-infra; \
	fi
	@echo "$(YELLOW)Starting VNext services with build for domain: $(DOMAIN)...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && set -a && . ./domains/$(DOMAIN)/.env && set +a && \
		$(COMPOSE_CMD) -p vnext-$(DOMAIN) --env-file ./domains/$(DOMAIN)/.env --profile vnext up -d --build
	@echo "$(GREEN)VNext services for $(DOMAIN) started!$(NC)"
	@$(MAKE) status-vnext DOMAIN=$(DOMAIN)

down: check-runtime ## Stop all services
	@echo "$(YELLOW)Stopping VNext Runtime services...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext down
	@echo "$(GREEN)Services stopped!$(NC)"

down-infra: check-runtime ## Stop only infrastructure services
	@echo "$(YELLOW)Stopping infrastructure services...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra down
	@echo "$(GREEN)Infrastructure services stopped!$(NC)"

down-vnext: check-runtime ## Stop vnext services for a domain (usage: make down-vnext DOMAIN=mydom)
	@echo "$(YELLOW)Stopping VNext services for domain: $(DOMAIN)...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) -p vnext-$(DOMAIN) --profile vnext down
	@echo "$(GREEN)VNext services for $(DOMAIN) stopped!$(NC)"

stop: down ## Alias for 'down'

restart: ## Restart all services
	@echo "$(YELLOW)Restarting VNext Runtime services...$(NC)"
	@$(MAKE) down
	@$(MAKE) up
	@echo "$(GREEN)Services restarted!$(NC)"

restart-infra: ## Restart infrastructure services
	@echo "$(YELLOW)Restarting infrastructure services...$(NC)"
	@$(MAKE) down-infra
	@$(MAKE) up-infra
	@echo "$(GREEN)Infrastructure services restarted!$(NC)"

restart-vnext: ## Restart vnext services for a domain (usage: make restart-vnext DOMAIN=mydom)
	@echo "$(YELLOW)Restarting VNext services for domain: $(DOMAIN)...$(NC)"
	@$(MAKE) down-vnext DOMAIN=$(DOMAIN)
	@$(MAKE) up-vnext DOMAIN=$(DOMAIN)
	@echo "$(GREEN)VNext services for $(DOMAIN) restarted!$(NC)"

##@ Service Management
status: check-runtime ## Show status of all services
	@echo "$(BLUE)VNext Runtime Services Status:$(NC)"
	@echo "================================="
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext ps

status-infra: check-runtime ## Show status of infrastructure services
	@echo "$(BLUE)Infrastructure Services Status:$(NC)"
	@echo "================================="
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra ps

status-vnext: check-runtime ## Show status of vnext services for a domain (usage: make status-vnext DOMAIN=mydom)
	@echo "$(BLUE)VNext Services Status for Domain: $(DOMAIN)$(NC)"
	@echo "============================================="
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) -p vnext-$(DOMAIN) --profile vnext ps

logs: check-runtime ## Show logs for all services
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext logs -f

logs-infra: check-runtime ## Show logs for infrastructure services
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra logs -f

logs-vnext: check-runtime ## Show logs for vnext services (usage: make logs-vnext DOMAIN=mydom)
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) -p vnext-$(DOMAIN) --profile vnext logs -f

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

health: ## Check health of services (usage: make health or make health DOMAIN=mydom)
	@echo "$(BLUE)Service Health Check:$(NC)"
	@echo "===================="
	@if [ -d "$(DOMAIN_DIR)" ] && [ -f "$(DOMAIN_DIR)/.env" ]; then \
		. $(DOMAIN_DIR)/.env; \
		echo "$(YELLOW)Domain: $(DOMAIN)$(NC)"; \
		echo ""; \
		echo "$(YELLOW)VNext Orchestration (port $$VNEXT_APP_PORT):$(NC)"; \
		curl -s http://localhost:$$VNEXT_APP_PORT/health || echo "$(RED)❌ Orchestration service not healthy$(NC)"; \
		echo ""; \
		echo "$(YELLOW)VNext Execution (port $$VNEXT_EXECUTION_PORT):$(NC)"; \
		curl -s http://localhost:$$VNEXT_EXECUTION_PORT/health || echo "$(RED)❌ Execution service not healthy$(NC)"; \
		echo ""; \
		echo "$(YELLOW)VNext Inbox (port $$VNEXT_INBOX_PORT):$(NC)"; \
		curl -s http://localhost:$$VNEXT_INBOX_PORT/health || echo "$(RED)❌ Inbox service not healthy$(NC)"; \
		echo ""; \
		echo "$(YELLOW)VNext Outbox (port $$VNEXT_OUTBOX_PORT):$(NC)"; \
		curl -s http://localhost:$$VNEXT_OUTBOX_PORT/health || echo "$(RED)❌ Outbox service not healthy$(NC)"; \
	else \
		echo "$(YELLOW)VNext Orchestration (default port 4201):$(NC)"; \
		curl -s http://localhost:4201/health || echo "$(RED)❌ Orchestration service not healthy$(NC)"; \
		echo ""; \
		echo "$(YELLOW)VNext Execution (default port 4202):$(NC)"; \
		curl -s http://localhost:4202/health || echo "$(RED)❌ Execution service not healthy$(NC)"; \
	fi
	@echo ""
	@echo "$(YELLOW)Infrastructure Services:$(NC)"
	@echo "• Vault: http://localhost:8200"
	@echo "• OpenObserve: http://localhost:5080"

##@ Database Operations (Domain-Specific)
db-create: check-runtime ## Create database for domain (usage: make db-create DOMAIN=core)
	@NORMALIZED=$$(echo "$(DOMAIN)" | sed 's/[^a-zA-Z0-9]/_/g' | awk '{print toupper(substr($$0,1,1)) tolower(substr($$0,2))}'); \
	DB_NAME="vNext_$${NORMALIZED}"; \
	echo "$(YELLOW)Creating database $$DB_NAME for domain: $(DOMAIN)...$(NC)"; \
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra exec -T postgres psql -U postgres -c "SELECT 1 FROM pg_database WHERE datname = '$$DB_NAME'" | grep -q 1 && \
		echo "$(YELLOW)Database $$DB_NAME already exists$(NC)" || \
		($(COMPOSE_CMD) --profile infra exec -T postgres psql -U postgres -c "CREATE DATABASE \"$$DB_NAME\";" && \
		echo "$(GREEN)Database $$DB_NAME created successfully!$(NC)")

db-drop: check-runtime ## Drop database for domain (WARNING: Destructive) (usage: make db-drop DOMAIN=core)
	@NORMALIZED=$$(echo "$(DOMAIN)" | sed 's/[^a-zA-Z0-9]/_/g' | awk '{print toupper(substr($$0,1,1)) tolower(substr($$0,2))}'); \
	DB_NAME="vNext_$${NORMALIZED}"; \
	echo "$(RED)WARNING: This will drop the $$DB_NAME database!$(NC)"; \
	echo "$(YELLOW)Press Ctrl+C to cancel, or wait 5 seconds to continue...$(NC)"; \
	sleep 5; \
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra exec -T postgres psql -U postgres -c "DROP DATABASE IF EXISTS \"$$DB_NAME\";"; \
	echo "$(GREEN)Database $$DB_NAME dropped!$(NC)"

db-reset: ## Reset database for domain (drop and recreate) (usage: make db-reset DOMAIN=core)
	@$(MAKE) db-drop DOMAIN=$(DOMAIN)
	@$(MAKE) db-create DOMAIN=$(DOMAIN)

db-status: check-runtime ## Check database status and list all databases
	@echo "$(BLUE)PostgreSQL Database Status:$(NC)"
	@echo "=========================="
	@cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra exec -T postgres psql -U postgres -c "\l" 2>/dev/null || echo "$(RED)❌ PostgreSQL is not running$(NC)"

db-connect: check-runtime ## Connect to domain database via psql (usage: make db-connect DOMAIN=core)
	@NORMALIZED=$$(echo "$(DOMAIN)" | sed 's/[^a-zA-Z0-9]/_/g' | awk '{print toupper(substr($$0,1,1)) tolower(substr($$0,2))}'); \
	DB_NAME="vNext_$${NORMALIZED}"; \
	echo "$(YELLOW)Connecting to database $$DB_NAME...$(NC)"; \
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra exec postgres psql -U postgres -d "$$DB_NAME"

db-list: check-runtime ## List all vNext databases
	@echo "$(BLUE)VNext Databases:$(NC)"
	@cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra exec -T postgres psql -U postgres -c "SELECT datname FROM pg_database WHERE datname LIKE 'vNext_%';" 2>/dev/null || echo "$(RED)❌ PostgreSQL is not running$(NC)"

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
	@echo "$(YELLOW)Stopping all domain projects...$(NC)"
	@for domain_dir in $(DOMAINS_DIR)/*/; do \
		if [ -d "$$domain_dir" ]; then \
			domain=$$(basename "$$domain_dir"); \
			echo "$(YELLOW)Stopping domain: $$domain$(NC)"; \
			cd $(DOCKER_DIR) && $(COMPOSE_CMD) -p vnext-$$domain --profile vnext down -v 2>/dev/null || true; \
		fi; \
	done
	@echo "$(YELLOW)Stopping infrastructure...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra down -v 2>/dev/null || true
	@echo "$(YELLOW)Removing all vnext containers...$(NC)"
	@$(CONTAINER_RUNTIME) ps -aq --filter "name=vnext-" | xargs -r $(CONTAINER_RUNTIME) rm -f 2>/dev/null || true
	@$(CONTAINER_RUNTIME) ps -aq --filter "name=dapr-" | xargs -r $(CONTAINER_RUNTIME) rm -f 2>/dev/null || true
	@$(CONTAINER_RUNTIME) ps -aq --filter "name=mockoon" | xargs -r $(CONTAINER_RUNTIME) rm -f 2>/dev/null || true
	@echo "$(YELLOW)Pruning system...$(NC)"
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
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext pull
	@$(MAKE) restart
	@echo "$(GREEN)Update completed!$(NC)"

##@ Monitoring
ps: check-runtime ## Show running containers
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext ps

top: check-runtime ## Show container resource usage
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext top

stats: check-runtime ## Show container statistics
	$(CONTAINER_RUNTIME) stats $(shell cd $(DOCKER_DIR) && $(COMPOSE_CMD) --profile infra --profile vnext ps -q)

##@ Custom Components
publish-component: check-env ## Publish component package (waits for vnext-init to be healthy)
	@echo "$(YELLOW)Publishing component package...$(NC)"
	@cd $(DOCKER_DIR) && ./publish-component.sh
	@echo "$(GREEN)Component published!$(NC)"

publish-component-skip-health: check-env ## Publish component package (skip health check)
	@echo "$(YELLOW)Publishing component package (skipping health check)...$(NC)"
	@cd $(DOCKER_DIR) && ./publish-component.sh --skip-health
	@echo "$(GREEN)Component published!$(NC)"

republish-component: check-runtime ## Re-run component publisher (usage: make republish-component DOMAIN=mydom)
	@if [ ! -d "$(DOMAIN_DIR)" ]; then \
		echo "$(RED)❌ Domain '$(DOMAIN)' not configured!$(NC)"; \
		exit 1; \
	fi
	@echo "$(YELLOW)Re-publishing component for domain: $(DOMAIN)...$(NC)"
	cd $(DOCKER_DIR) && $(COMPOSE_CMD) -p vnext-$(DOMAIN) --profile vnext rm -f vnext-component-publisher
	cd $(DOCKER_DIR) && set -a && . ./domains/$(DOMAIN)/.env && set +a && \
		$(COMPOSE_CMD) -p vnext-$(DOMAIN) --env-file ./domains/$(DOMAIN)/.env --profile vnext up vnext-component-publisher
	@echo "$(GREEN)Component re-published for $(DOMAIN)!$(NC)"

##@ Multi-Domain Management
create-domain: ## Create domain configuration (usage: make create-domain DOMAIN=mydom PORT_OFFSET=10)
	@echo "$(BLUE)Creating domain configuration...$(NC)"
	@cd $(DOCKER_DIR) && ./create-domain.sh $(DOMAIN) $(PORT_OFFSET)

list-domains: ## List all configured domains
	@echo "$(BLUE)Configured Domains:$(NC)"
	@echo "==================="
	@if [ -d "$(DOMAINS_DIR)" ] && [ "$$(ls -A $(DOMAINS_DIR) 2>/dev/null)" ]; then \
		for d in $(DOMAINS_DIR)/*/; do \
			if [ -d "$$d" ]; then \
				domain=$$(basename "$$d"); \
				if [ -f "$$d/.env" ]; then \
					port=$$(grep "^VNEXT_APP_PORT=" "$$d/.env" 2>/dev/null | cut -d= -f2); \
					offset=$$(grep "^PORT_OFFSET=" "$$d/.env" 2>/dev/null | cut -d= -f2); \
					echo "  • $$domain (port: $${port:-N/A}, offset: $${offset:-0})"; \
				fi; \
			fi; \
		done; \
	else \
		echo "  No domains configured."; \
		echo "  Run 'make create-domain DOMAIN=<name> PORT_OFFSET=<offset>'"; \
	fi

status-all-domains: check-runtime ## Show status of all running domain services
	@echo "$(BLUE)All Running VNext Services:$(NC)"
	@echo "==========================="
	@$(CONTAINER_RUNTIME) ps --filter "name=vnext-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || echo "No vnext services running"

down-all-vnext: check-runtime ## Stop all vnext domain services (keeps infra running)
	@echo "$(YELLOW)Stopping all VNext domain services...$(NC)"
	@if [ -d "$(DOMAINS_DIR)" ]; then \
		for d in $(DOMAINS_DIR)/*/; do \
			if [ -d "$$d" ]; then \
				domain=$$(basename "$$d"); \
				echo "$(YELLOW)Stopping domain: $$domain$(NC)"; \
				cd $(DOCKER_DIR) && $(COMPOSE_CMD) -p vnext-$$domain --profile vnext down 2>/dev/null || true; \
			fi; \
		done; \
	fi
	@echo "$(GREEN)All VNext domain services stopped!$(NC)"

##@ Legacy Domain Configuration (single domain)
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
	@echo "$(YELLOW)Project:$(NC) VNext Runtime (Multi-Domain Support)"
	@echo "$(YELLOW)Container Runtime:$(NC) $(CONTAINER_RUNTIME)$(if $(filter yes,$(IS_ORBSTACK)), (OrbStack),)"
	@echo "$(YELLOW)Compose Command:$(NC) $(COMPOSE_CMD)"
	@echo "$(YELLOW)Docker Compose:$(NC) $(DOCKER_COMPOSE_FILE)"
	@echo "$(YELLOW)Network:$(NC) $(NETWORK_NAME)"
	@echo ""
	@echo "$(BLUE)Infrastructure Services (shared):$(NC)"
	@echo "• PostgreSQL: localhost:5432"
	@echo "• Redis: localhost:6379"
	@echo "• Vault: http://localhost:8200 (token: admin)"
	@echo "• OpenObserve: http://localhost:5080 (root@example.com / Complexpass#@123)"
	@echo "• Dapr Placement: localhost:50005"
	@echo "• Dapr Scheduler: localhost:50007"
	@echo ""
	@echo "$(BLUE)Multi-Domain Commands:$(NC)"
	@echo "• make create-domain DOMAIN=mydom PORT_OFFSET=10  - Create new domain"
	@echo "• make up-vnext DOMAIN=mydom                      - Start domain services"
	@echo "• make down-vnext DOMAIN=mydom                    - Stop domain services"
	@echo "• make list-domains                               - List configured domains"
	@echo "• make status-all-domains                         - Show all running services"
	@echo ""
	@echo "$(BLUE)Port Allocation (by offset):$(NC)"
	@echo "• Offset 0:  Ports 4201-4204, 3005"
	@echo "• Offset 10: Ports 4211-4214, 3015"
	@echo "• Offset 20: Ports 4221-4224, 3025"
	@echo ""
	@echo "$(BLUE)Quick Start:$(NC)"
	@echo "1. make up-infra                           - Start infrastructure"
	@echo "2. make create-domain DOMAIN=core          - Create 'core' domain (offset 0)"
	@echo "3. make create-domain DOMAIN=sales PORT_OFFSET=10  - Create 'sales' domain"
	@echo "4. make up-vnext DOMAIN=core               - Start 'core' domain"
	@echo "5. make up-vnext DOMAIN=sales              - Start 'sales' domain"

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
.PHONY: help check-runtime setup create-env-files create-network check-env check-env-infra build up up-infra up-vnext start up-build up-infra-build up-vnext-build down down-infra down-vnext stop restart restart-infra restart-vnext status status-infra status-vnext logs logs-infra logs-vnext logs-orchestration logs-execution logs-init logs-dapr logs-db health dev shell-orchestration shell-execution shell-postgres shell-redis clean clean-all reset update ps top stats publish-component publish-component-skip-health republish-component git-init info version db-create db-drop db-reset db-status db-connect db-list change-domain create-domain list-domains status-all-domains down-all-vnext
