#!/bin/bash

# VNext Component Publisher
# This script waits for vnext-init to be healthy and publishes a component package

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="${SCRIPT_DIR}/.env"
VNEXT_INIT_URL="${VNEXT_INIT_URL:-http://localhost:3005}"
HEALTH_ENDPOINT="${VNEXT_INIT_URL}/health"
PUBLISH_ENDPOINT="${VNEXT_INIT_URL}/api/package/runtime/publish"
MAX_RETRIES="${MAX_RETRIES:-60}"
RETRY_INTERVAL="${RETRY_INTERVAL:-5}"

# Function to print colored messages
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Function to load environment variables from .env file
load_env() {
    if [ -f "$ENV_FILE" ]; then
        log_info "Loading environment from $ENV_FILE"
        # Export variables from .env file (ignore comments and empty lines)
        while IFS= read -r line || [ -n "$line" ]; do
            # Skip empty lines and comments
            [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue
            # Remove carriage returns and export the variable
            line=$(echo "$line" | sed 's/\r$//')
            export "$line"
        done < "$ENV_FILE"
    else
        log_warning ".env file not found at $ENV_FILE"
    fi
}

# Function to check if required variables are set
check_required_vars() {
    local missing_vars=()

    if [ -z "$VNEXT_COMPONENT_VERSION" ]; then
        missing_vars+=("VNEXT_COMPONENT_VERSION")
    fi

    if [ -z "$APP_DOMAIN" ]; then
        missing_vars+=("APP_DOMAIN")
    fi

    if [ ${#missing_vars[@]} -ne 0 ]; then
        log_error "Missing required environment variables: ${missing_vars[*]}"
        log_info "Please set these variables in $ENV_FILE or as environment variables"
        exit 1
    fi

    log_success "Required variables loaded:"
    log_info "  VNEXT_COMPONENT_VERSION: $VNEXT_COMPONENT_VERSION"
    log_info "  APP_DOMAIN: $APP_DOMAIN"
}

# Function to wait for vnext-init to be healthy
wait_for_health() {
    log_info "Waiting for vnext-init to be healthy at $HEALTH_ENDPOINT..."
    
    local retry_count=0
    
    while [ $retry_count -lt $MAX_RETRIES ]; do
        if curl -s -f "$HEALTH_ENDPOINT" > /dev/null 2>&1; then
            log_success "vnext-init is healthy!"
            return 0
        fi
        
        retry_count=$((retry_count + 1))
        log_warning "Attempt $retry_count/$MAX_RETRIES - vnext-init not ready yet. Retrying in ${RETRY_INTERVAL}s..."
        sleep $RETRY_INTERVAL
    done
    
    log_error "vnext-init did not become healthy after $MAX_RETRIES attempts"
    exit 1
}

# Function to publish the component
publish_component() {
    log_info "Publishing component..."
    log_info "  Version: $VNEXT_COMPONENT_VERSION"
    log_info "  App Domain: $APP_DOMAIN"
    log_info "  Endpoint: $PUBLISH_ENDPOINT"
    
    local response
    local http_code
    
    # Make the POST request and capture both response body and HTTP status code
    response=$(curl -s -w "\n%{http_code}" -X POST "$PUBLISH_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d "{\"version\": \"$VNEXT_COMPONENT_VERSION\", \"appDomain\": \"$APP_DOMAIN\"}")
    
    # Extract HTTP status code (last line) and response body (everything else)
    http_code=$(echo "$response" | tail -n 1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        log_success "Component published successfully! (HTTP $http_code)"
        if [ -n "$body" ]; then
            log_info "Response: $body"
        fi
        return 0
    else
        log_error "Failed to publish component (HTTP $http_code)"
        if [ -n "$body" ]; then
            log_error "Response: $body"
        fi
        exit 1
    fi
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Waits for vnext-init to be healthy and publishes a component package."
    echo ""
    echo "Options:"
    echo "  -h, --help              Show this help message"
    echo "  -v, --version VERSION   Override VNEXT_COMPONENT_VERSION"
    echo "  -d, --domain DOMAIN     Override APP_DOMAIN"
    echo "  -u, --url URL           Override VNEXT_INIT_URL (default: http://localhost:3005)"
    echo "  -r, --retries N         Maximum number of health check retries (default: 60)"
    echo "  -i, --interval N        Seconds between retries (default: 5)"
    echo "  --skip-health           Skip waiting for health check"
    echo ""
    echo "Environment Variables (can also be set in .env file):"
    echo "  VNEXT_COMPONENT_VERSION   Version of the component to publish"
    echo "  APP_DOMAIN                Application domain"
    echo "  VNEXT_INIT_URL            Base URL for vnext-init service"
    echo "  MAX_RETRIES               Maximum health check retries"
    echo "  RETRY_INTERVAL            Seconds between retries"
}

# Parse command line arguments
SKIP_HEALTH=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_usage
            exit 0
            ;;
        -v|--version)
            VNEXT_COMPONENT_VERSION="$2"
            shift 2
            ;;
        -d|--domain)
            APP_DOMAIN="$2"
            shift 2
            ;;
        -u|--url)
            VNEXT_INIT_URL="$2"
            HEALTH_ENDPOINT="${VNEXT_INIT_URL}/health"
            PUBLISH_ENDPOINT="${VNEXT_INIT_URL}/api/package/runtime/publish"
            shift 2
            ;;
        -r|--retries)
            MAX_RETRIES="$2"
            shift 2
            ;;
        -i|--interval)
            RETRY_INTERVAL="$2"
            shift 2
            ;;
        --skip-health)
            SKIP_HEALTH=true
            shift
            ;;
        *)
            log_error "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Main execution
main() {
    echo ""
    echo -e "${BLUE}╔══════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║     VNext Component Publisher            ║${NC}"
    echo -e "${BLUE}╚══════════════════════════════════════════╝${NC}"
    echo ""
    
    # Load environment variables
    load_env
    
    # Check required variables
    check_required_vars
    
    # Wait for vnext-init to be healthy (unless skipped)
    if [ "$SKIP_HEALTH" = false ]; then
        wait_for_health
    else
        log_warning "Skipping health check as requested"
    fi
    
    # Publish the component
    publish_component
    
    echo ""
    log_success "Component publication completed!"
}

# Run main function
main

