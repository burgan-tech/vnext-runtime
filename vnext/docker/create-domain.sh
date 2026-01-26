#!/bin/bash
# Script to create domain-specific configuration files from templates
# Usage: ./create-domain.sh <domain_name> [port_offset]
#
# Templates are read from: templates/
# Output is created in: domains/<domain_name>/

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

DOMAIN_NAME="${1:-core}"
PORT_OFFSET="${2:-0}"

# Calculate ports based on offset
# Base ports: 4201, 4202, 4203, 4204, 3005
# Dapr ports: 42110/42111, 43110/43111, 44110/44111, 45110/45111
VNEXT_APP_PORT=$((4201 + PORT_OFFSET))
VNEXT_EXECUTION_PORT=$((4202 + PORT_OFFSET))
VNEXT_INBOX_PORT=$((4203 + PORT_OFFSET))
VNEXT_OUTBOX_PORT=$((4204 + PORT_OFFSET))
VNEXT_INIT_PORT=$((3005 + PORT_OFFSET))

# Dapr ports (using offset * 100 to avoid conflicts)
DAPR_OFFSET=$((PORT_OFFSET * 100))
DAPR_ORCHESTRATION_HTTP_PORT=$((42110 + DAPR_OFFSET))
DAPR_ORCHESTRATION_GRPC_PORT=$((42111 + DAPR_OFFSET))
DAPR_EXECUTION_HTTP_PORT=$((43110 + DAPR_OFFSET))
DAPR_EXECUTION_GRPC_PORT=$((43111 + DAPR_OFFSET))
DAPR_INBOX_HTTP_PORT=$((44110 + DAPR_OFFSET))
DAPR_INBOX_GRPC_PORT=$((44111 + DAPR_OFFSET))
DAPR_OUTBOX_HTTP_PORT=$((45110 + DAPR_OFFSET))
DAPR_OUTBOX_GRPC_PORT=$((45111 + DAPR_OFFSET))

# Normalize domain name for database (capitalize first letter, replace non-alphanumeric)
NORMALIZED_DOMAIN=$(echo "${DOMAIN_NAME}" | sed 's/[^a-zA-Z0-9]/_/g' | awk '{for(i=1;i<=length;i++){if(i==1){printf toupper(substr($0,i,1))}else{printf substr($0,i,1)}}}')
DB_NAME="vNext_${NORMALIZED_DOMAIN}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEMPLATES_DIR="${SCRIPT_DIR}/templates"
DOMAIN_DIR="${SCRIPT_DIR}/domains/${DOMAIN_NAME}"

echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}     Creating VNext Domain Configuration: ${DOMAIN_NAME}${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}Domain:${NC} ${DOMAIN_NAME}"
echo -e "${YELLOW}Port Offset:${NC} ${PORT_OFFSET}"
echo -e "${YELLOW}Database:${NC} ${DB_NAME}"
echo -e "${YELLOW}Templates:${NC} templates/"
echo -e "${YELLOW}Output:${NC} domains/${DOMAIN_NAME}/"
echo ""
echo -e "${PURPLE}Port Assignments:${NC}"
echo -e "  • VNext App:       ${VNEXT_APP_PORT}"
echo -e "  • VNext Execution: ${VNEXT_EXECUTION_PORT}"
echo -e "  • VNext Inbox:     ${VNEXT_INBOX_PORT}"
echo -e "  • VNext Outbox:    ${VNEXT_OUTBOX_PORT}"
echo -e "  • VNext Init:      ${VNEXT_INIT_PORT}"
echo ""

# Check if templates directory exists
if [ ! -d "${TEMPLATES_DIR}" ]; then
    echo -e "${RED}❌ Templates directory not found: ${TEMPLATES_DIR}${NC}"
    exit 1
fi

# Create domain directory
mkdir -p "${DOMAIN_DIR}"

# Function to process template file
process_template() {
    local template_file=$1
    local output_file=$2
    local template_name=$(basename "$template_file")
    
    if [ -f "${template_file}" ]; then
        echo -e "${YELLOW}Creating ${output_file} from template...${NC}"
        sed -e "s|{{DOMAIN_NAME}}|${DOMAIN_NAME}|g" \
            -e "s|{{PORT_OFFSET}}|${PORT_OFFSET}|g" \
            -e "s|{{DB_NAME}}|${DB_NAME}|g" \
            -e "s|{{VNEXT_APP_PORT}}|${VNEXT_APP_PORT}|g" \
            -e "s|{{VNEXT_EXECUTION_PORT}}|${VNEXT_EXECUTION_PORT}|g" \
            -e "s|{{VNEXT_INBOX_PORT}}|${VNEXT_INBOX_PORT}|g" \
            -e "s|{{VNEXT_OUTBOX_PORT}}|${VNEXT_OUTBOX_PORT}|g" \
            -e "s|{{VNEXT_INIT_PORT}}|${VNEXT_INIT_PORT}|g" \
            -e "s|{{DAPR_ORCHESTRATION_HTTP_PORT}}|${DAPR_ORCHESTRATION_HTTP_PORT}|g" \
            -e "s|{{DAPR_ORCHESTRATION_GRPC_PORT}}|${DAPR_ORCHESTRATION_GRPC_PORT}|g" \
            -e "s|{{DAPR_EXECUTION_HTTP_PORT}}|${DAPR_EXECUTION_HTTP_PORT}|g" \
            -e "s|{{DAPR_EXECUTION_GRPC_PORT}}|${DAPR_EXECUTION_GRPC_PORT}|g" \
            -e "s|{{DAPR_INBOX_HTTP_PORT}}|${DAPR_INBOX_HTTP_PORT}|g" \
            -e "s|{{DAPR_INBOX_GRPC_PORT}}|${DAPR_INBOX_GRPC_PORT}|g" \
            -e "s|{{DAPR_OUTBOX_HTTP_PORT}}|${DAPR_OUTBOX_HTTP_PORT}|g" \
            -e "s|{{DAPR_OUTBOX_GRPC_PORT}}|${DAPR_OUTBOX_GRPC_PORT}|g" \
            -e "s|vNext_[a-zA-Z0-9_]*|${DB_NAME}|g" \
            "${template_file}" > "${output_file}"
    else
        echo -e "${YELLOW}⚠️  Template not found: ${template_name} (skipped)${NC}"
    fi
}

# Process .env templates
process_template "${TEMPLATES_DIR}/.env" "${DOMAIN_DIR}/.env"
process_template "${TEMPLATES_DIR}/.env.orchestration" "${DOMAIN_DIR}/.env.orchestration"
process_template "${TEMPLATES_DIR}/.env.execution" "${DOMAIN_DIR}/.env.execution"
process_template "${TEMPLATES_DIR}/.env.worker-inbox" "${DOMAIN_DIR}/.env.worker-inbox"
process_template "${TEMPLATES_DIR}/.env.worker-outbox" "${DOMAIN_DIR}/.env.worker-outbox"

# Process appsettings templates
process_template "${TEMPLATES_DIR}/appsettings.Development.json" "${DOMAIN_DIR}/appsettings.Development.json"
process_template "${TEMPLATES_DIR}/appsettings.Execution.Development.json" "${DOMAIN_DIR}/appsettings.Execution.Development.json"
process_template "${TEMPLATES_DIR}/appsettings.WorkerInbox.Development.json" "${DOMAIN_DIR}/appsettings.WorkerInbox.Development.json"
process_template "${TEMPLATES_DIR}/appsettings.WorkerOutbox.Development.json" "${DOMAIN_DIR}/appsettings.WorkerOutbox.Development.json"

echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ Domain configuration created successfully!${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}Files created in domains/${DOMAIN_NAME}/:${NC}"
ls -1 "${DOMAIN_DIR}" | sed 's/^/  • /'
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "  1. Create database: make db-create-domain DOMAIN=${DOMAIN_NAME}"
echo -e "  2. Start services:  make up-vnext DOMAIN=${DOMAIN_NAME}"
echo ""
