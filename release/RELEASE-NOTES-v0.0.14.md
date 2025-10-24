# vNext Runtime Platform - Release Notes v0.0.14
**Release Date:** October 24, 2025

## Overview
This is an intermediate release focusing on infrastructure improvements and bug fixes. The release addresses critical issues with OpenTelemetry configuration, health check reliability, and container image distribution.

## 🔧 Infrastructure Improvements

### Container Image Distribution
The vNext Init component has been migrated from local Docker builds to centralized image registry distribution.

**Changes:**
- Removed local `Dockerfile.vnext-core-init` 
- Now using pre-built image: `ghcr.io/burgan-tech/vnext/init:${VNEXT_INIT_VERSION}`
- Added `VNEXT_INIT_VERSION=0.0.13` environment variable
- Improved deployment consistency across environments
- Added npm cache volume for better performance

**Benefits:**
- Faster deployment times
- Consistent image versions across environments
- Reduced build complexity
- Better resource utilization

### OpenTelemetry gRPC Protocol Fix
Fixed OpenTelemetry configuration to use proper gRPC protocol instead of HTTP.

**Configuration Changes:**
```bash
# Before (HTTP/Protobuf - causing issues)
Telemetry__Otlp__Endpoint=http://otel-collector:4318
Telemetry__Otlp__Protocol=http/protobuf

# After (gRPC - stable)
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

**Impact:**
- Resolved telemetry data transmission issues
- Improved observability reliability
- Better performance with gRPC protocol
- Consistent with OpenTelemetry best practices

### Health Check Improvements
Enhanced health check reliability and service startup coordination.

**Makefile Enhancements:**
- Added 3-second delay before checking VNext Execution service
- Improved service coordination during startup
- Better error handling in health check commands
- Enhanced user feedback during service verification

**Docker Compose Health Checks:**
- Fixed health check URLs to use container names instead of localhost
- `vnext-app`: `http://vnext-app:5000/health`
- `vnext-execution-app`: `http://vnext-execution-app:5000/health`
- Improved container-to-container communication reliability

## 🐛 Bug Fixes

### Service Communication
- Fixed health check endpoints to use proper container networking
- Resolved intermittent health check failures
- Improved service discovery reliability

### Container Orchestration
- Enhanced service startup sequence
- Better dependency management between services
- Improved container restart behavior

### Telemetry Stability
- Resolved OpenTelemetry connection issues
- Fixed trace data export problems
- Improved monitoring reliability

## 📋 Configuration Updates

### Environment Variables
New and updated environment variables:

```bash
# New Init Image Version
VNEXT_INIT_VERSION=0.0.13

# Updated OpenTelemetry Configuration
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

### Docker Compose
- Added npm cache volume for improved performance
- Updated health check configurations
- Enhanced service networking

## 🚀 Deployment Notes

### Upgrade Instructions
1. Update environment files with new OpenTelemetry configuration
2. Pull latest container images
3. Restart services to apply health check improvements

### Breaking Changes
- None - this is a backward-compatible release

### Compatibility
- Compatible with all existing workflows and configurations
- No API changes
- No database schema changes

## 📊 Performance Improvements

### Container Startup
- Faster init container deployment
- Reduced image pull times
- Better resource utilization

### Monitoring
- More reliable telemetry data collection
- Improved trace data export
- Better observability coverage

## 🔍 Technical Details

### Image Registry Migration
The vnext-init component migration provides:
- Centralized version control
- Automated security scanning
- Consistent deployment artifacts
- Reduced local build dependencies

### gRPC Protocol Benefits
- Lower latency than HTTP/Protobuf
- Better connection management
- Improved error handling
- Native OpenTelemetry support

### Health Check Reliability
- Container-aware networking
- Better startup coordination
- Reduced false positives
- Improved monitoring accuracy

## 📞 Support
For technical support and questions, please contact the vNext Runtime Platform team.

---
**vNext Runtime Platform Team**  
October 24, 2025
