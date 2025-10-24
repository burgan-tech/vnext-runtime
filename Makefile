# VNext Runtime Makefile
# This Makefile provides convenient commands for managing the VNext Runtime application

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
NC = \033[0m # No Color

##@ Help
help: ## Display this help message
	@echo "$(BLUE)VNext Runtime Management$(NC)"
	@echo "=========================="
	@awk 'BEGIN {FS = ":.*##"; printf "\nUsage:\n  make \033[36m<target>\033[0m\n"} /^[a-zA-Z_0-9-]+:.*?##/ { printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2 } /^##@/ { printf "\n\033[1m%s\033[0m\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

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
		echo "# Custom Components Path (optional)" >> $(ENV_FILE); \
		echo "CUSTOM_COMPONENTS_PATH=./custom-components" >> $(ENV_FILE); \
		echo "" >> $(ENV_FILE); \
		echo "# Docker Image Versions" >> $(ENV_FILE); \
		echo "VNEXT_ORCHESTRATOR_VERSION=latest" >> $(ENV_FILE); \
		echo "VNEXT_EXECUTION_VERSION=latest" >> $(ENV_FILE); \
		echo "DAPR_RUNTIME_VERSION=latest" >> $(ENV_FILE); \
		echo "DAPR_PLACEMENT_VERSION=latest" >> $(ENV_FILE); \
		echo "DAPR_SCHEDULER_VERSION=latest" >> $(ENV_FILE); \
		echo "REDIS_VERSION=latest" >> $(ENV_FILE); \
		echo "REDIS_INSIGHT_VERSION=latest" >> $(ENV_FILE); \
		echo "POSTGRES_VERSION=latest" >> $(ENV_FILE); \
		echo "PGADMIN_VERSION=latest" >> $(ENV_FILE); \
		echo "VAULT_VERSION=1.13.3" >> $(ENV_FILE); \
		echo "ALPINE_CURL_VERSION=latest" >> $(ENV_FILE); \
		echo "OPENOBSERVE_VERSION=latest" >> $(ENV_FILE); \
		echo "OTEL_COLLECTOR_VERSION=latest" >> $(ENV_FILE); \
		echo "PROMETHEUS_VERSION=latest" >> $(ENV_FILE); \
		echo "GRAFANA_VERSION=latest" >> $(ENV_FILE); \
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

create-network: ## Create Docker network
	@echo "$(YELLOW)Creating Docker network: $(NETWORK_NAME)...$(NC)"
	@docker network inspect $(NETWORK_NAME) >/dev/null 2>&1 || \
	(docker network create $(NETWORK_NAME) && echo "$(GREEN)Network $(NETWORK_NAME) created$(NC)") || \
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

##@ Docker Operations
build: check-env ## Build Docker images
	@echo "$(YELLOW)Building Docker images...$(NC)"
	cd $(DOCKER_DIR) && docker-compose build
	@echo "$(GREEN)Build completed!$(NC)"

up: check-env ## Start all services
	@echo "$(YELLOW)Starting VNext Runtime services...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && docker-compose up -d
	@echo "$(GREEN)Services started!$(NC)"
	@$(MAKE) status

start: up-build ## Start services with build

up-build: check-env ## Start services with build
	@echo "$(YELLOW)Starting VNext Runtime services with build...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && docker-compose up -d --build
	@echo "$(GREEN)Services started!$(NC)"
	@$(MAKE) status

down: ## Stop all services
	@echo "$(YELLOW)Stopping VNext Runtime services...$(NC)"
	cd $(DOCKER_DIR) && docker-compose down
	@echo "$(GREEN)Services stopped!$(NC)"

stop: down ## Alias for 'down'

restart: ## Restart all services
	@echo "$(YELLOW)Restarting VNext Runtime services...$(NC)"
	@$(MAKE) down
	@$(MAKE) up
	@echo "$(GREEN)Services restarted!$(NC)"

##@ Service Management
status: ## Show status of all services
	@echo "$(BLUE)VNext Runtime Services Status:$(NC)"
	@echo "================================="
	cd $(DOCKER_DIR) && docker-compose ps

logs: ## Show logs for all services
	cd $(DOCKER_DIR) && docker-compose logs -f

logs-orchestration: ## Show logs for orchestration service
	cd $(DOCKER_DIR) && docker-compose logs -f vnext-app

logs-execution: ## Show logs for execution service
	cd $(DOCKER_DIR) && docker-compose logs -f vnext-execution-app

logs-init: ## Show logs for core init service
	cd $(DOCKER_DIR) && docker-compose logs -f vnext-core-init

logs-dapr: ## Show logs for DAPR services
	cd $(DOCKER_DIR) && docker-compose logs -f vnext-orchestration-dapr vnext-execution-dapr

logs-db: ## Show logs for database services
	cd $(DOCKER_DIR) && docker-compose logs -f postgres redis

logs-monitoring: ## Show logs for monitoring services
	cd $(DOCKER_DIR) && docker-compose logs -f prometheus grafana

logs-prometheus: ## Show logs for Prometheus service
	cd $(DOCKER_DIR) && docker-compose logs -f prometheus

logs-grafana: ## Show logs for Grafana service
	cd $(DOCKER_DIR) && docker-compose logs -f grafana

health: ## Check health of services
	@echo "$(BLUE)Service Health Check:$(NC)"
	@echo "===================="
	@echo "$(YELLOW)VNext Orchestration:$(NC)"
	@curl -s http://localhost:4201/health || echo "$(RED)❌ Orchestration service not healthy$(NC)"
	@echo ""
	@echo "$(YELLOW)Waiting 3 seconds before checking VNext Execution...$(NC)"
	@sleep 3
	@echo "$(YELLOW)VNext Execution:$(NC)"
	@curl -s http://localhost:4202/health || echo "$(RED)❌ Execution service not healthy$(NC)"
	@echo ""
	@echo "$(YELLOW)Management Interfaces:$(NC)"
	@echo "• Redis Insight: http://localhost:5501"
	@echo "• PgAdmin: http://localhost:5502"
	@echo "• Vault: http://localhost:8200"
	@echo "• OpenObserve: http://localhost:5080"
	@echo "• Prometheus: http://localhost:9090"
	@echo "• Grafana: http://localhost:3000"

##@ Development
dev: ## Start development environment
	@echo "$(YELLOW)Starting development environment...$(NC)"
	@$(MAKE) setup
	@$(MAKE) up-build
	@$(MAKE) health

shell-orchestration: ## Open shell in orchestration container
	cd $(DOCKER_DIR) && docker-compose exec vnext-app sh

shell-execution: ## Open shell in execution container
	cd $(DOCKER_DIR) && docker-compose exec vnext-execution-app sh

shell-postgres: ## Open PostgreSQL shell
	cd $(DOCKER_DIR) && docker-compose exec postgres psql -U postgres

shell-redis: ## Open Redis CLI
	cd $(DOCKER_DIR) && docker-compose exec redis redis-cli

##@ Maintenance
clean: ## Remove stopped containers and unused networks
	@echo "$(YELLOW)Cleaning up Docker resources...$(NC)"
	docker container prune -f
	docker network prune -f
	@echo "$(GREEN)Cleanup completed!$(NC)"

clean-all: ## Remove all containers, images, and volumes (WARNING: Destructive)
	@echo "$(RED)WARNING: This will remove ALL containers, images, and volumes!$(NC)"
	@echo "$(YELLOW)Press Ctrl+C to cancel, or wait 10 seconds to continue...$(NC)"
	@sleep 10
	@$(MAKE) down
	cd $(DOCKER_DIR) && docker-compose down -v --rmi all
	docker system prune -a -f
	@echo "$(GREEN)Complete cleanup finished!$(NC)"

reset: ## Reset environment (stop, clean, and setup)
	@echo "$(YELLOW)Resetting VNext Runtime environment...$(NC)"
	@$(MAKE) down
	@$(MAKE) clean
	@$(MAKE) setup
	@echo "$(GREEN)Environment reset completed!$(NC)"

update: ## Pull latest images and restart
	@echo "$(YELLOW)Updating VNext Runtime images...$(NC)"
	cd $(DOCKER_DIR) && docker-compose pull
	@$(MAKE) restart
	@echo "$(GREEN)Update completed!$(NC)"

##@ Monitoring
ps: ## Show running containers
	cd $(DOCKER_DIR) && docker-compose ps

top: ## Show container resource usage
	cd $(DOCKER_DIR) && docker-compose top

stats: ## Show container statistics
	docker stats $(shell cd $(DOCKER_DIR) && docker-compose ps -q)

monitoring-up: ## Start only monitoring services (Prometheus & Grafana)
	@echo "$(YELLOW)Starting monitoring services...$(NC)"
	@$(MAKE) create-network
	cd $(DOCKER_DIR) && docker-compose up -d prometheus grafana
	@echo "$(GREEN)Monitoring services started!$(NC)"
	@echo "$(BLUE)Access URLs:$(NC)"
	@echo "• Prometheus: http://localhost:9090"
	@echo "• Grafana: http://localhost:3000 (admin/admin)"

monitoring-down: ## Stop monitoring services
	@echo "$(YELLOW)Stopping monitoring services...$(NC)"
	cd $(DOCKER_DIR) && docker-compose stop prometheus grafana
	@echo "$(GREEN)Monitoring services stopped!$(NC)"

monitoring-restart: ## Restart monitoring services
	@echo "$(YELLOW)Restarting monitoring services...$(NC)"
	@$(MAKE) monitoring-down
	@$(MAKE) monitoring-up
	@echo "$(GREEN)Monitoring services restarted!$(NC)"

monitoring-status: ## Show status of monitoring services
	@echo "$(BLUE)Monitoring Services Status:$(NC)"
	@echo "=========================="
	cd $(DOCKER_DIR) && docker-compose ps prometheus grafana

prometheus-config-reload: ## Reload Prometheus configuration
	@echo "$(YELLOW)Reloading Prometheus configuration...$(NC)"
	@curl -X POST http://localhost:9090/-/reload || echo "$(RED)❌ Failed to reload Prometheus config$(NC)"
	@echo "$(GREEN)Prometheus configuration reloaded!$(NC)"

grafana-reset-password: ## Reset Grafana admin password
	@echo "$(YELLOW)Resetting Grafana admin password...$(NC)"
	cd $(DOCKER_DIR) && docker-compose exec grafana grafana-cli admin reset-admin-password admin
	@echo "$(GREEN)Grafana admin password reset to 'admin'$(NC)"

##@ Custom Components
init-custom-components: ## Initialize custom components directory
	@echo "$(YELLOW)Initializing custom components directory...$(NC)"
	@mkdir -p $(DOCKER_DIR)/custom-components/{Extensions,Functions,Schemas,Tasks,Views,Workflows}
	@echo "$(GREEN)Custom components directory created!$(NC)"
	@echo "$(BLUE)Directory structure:$(NC)"
	@tree $(DOCKER_DIR)/custom-components 2>/dev/null || ls -la $(DOCKER_DIR)/custom-components

reload-components: ## Reload custom components
	@echo "$(YELLOW)Reloading custom components...$(NC)"
	cd $(DOCKER_DIR) && docker-compose restart vnext-core-init
	@echo "$(GREEN)Components reloaded!$(NC)"

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
	@echo "$(YELLOW)Docker Compose:$(NC) $(DOCKER_COMPOSE_FILE)"
	@echo "$(YELLOW)Network:$(NC) $(NETWORK_NAME)"
	@echo ""
	@echo "$(BLUE)Services:$(NC)"
	@echo "• VNext Orchestration: http://localhost:4201"
	@echo "$(YELLOW)Waiting 3 seconds before checking VNext Execution...$(NC)"
	@sleep 3
	@echo "• VNext Execution: http://localhost:4202"
	@echo ""
	@echo "$(BLUE)Management Interfaces:$(NC)"
	@echo "• Redis Insight: http://localhost:5501"
	@echo "• PgAdmin: http://localhost:5502 (info@info.com / admin)"
	@echo "• Vault: http://localhost:8200 (admin)"
	@echo "• OpenObserve: http://localhost:5080 (root@example.com / Complexpass#@123)"
	@echo "• Prometheus: http://localhost:9090"
	@echo "• Grafana: http://localhost:3000 (admin / admin)"
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
	@echo "$(YELLOW)Docker:$(NC) $(shell docker --version)"
	@echo "$(YELLOW)Docker Compose:$(NC) $(shell docker-compose --version)"

# Prevent make from interpreting file names as targets
.PHONY: help setup create-env-files create-network check-env build up start up-build down stop restart status logs logs-orchestration logs-execution logs-init logs-dapr logs-db logs-monitoring logs-prometheus logs-grafana health dev shell-orchestration shell-execution shell-postgres shell-redis clean clean-all reset update ps top stats monitoring-up monitoring-down monitoring-restart monitoring-status prometheus-config-reload grafana-reset-password init-custom-components reload-components git-init info version
