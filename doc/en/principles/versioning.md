# Version Management and Deployment Strategy

This documentation comprehensively explains versioning, package management, and deployment processes in the vNext platform.

## Table of Contents

1. [Version Management](#version-management)
2. [ETag Usage](#etag-usage)
3. [Development Environment](#development-environment)
4. [Package Management](#package-management)
5. [Runtime Deployment](#runtime-deployment)
6. [Version Format](#version-format)
7. [Test and Rollback Strategy](#test-and-rollback-strategy)

---

## Version Management

All business record instances in the vNext platform can be managed in versions. The [semantic versioning](https://semver.org/) approach is adopted for versioning.

### Version Format

The version follows the standard `MAJOR.MINOR.PATCH` format:

| Component | Description | Example |
|-----------|-------------|---------|
| **MAJOR** | Backward-incompatible API changes | `2.0.0` |
| **MINOR** | Backward-compatible new features | `1.1.0` |
| **PATCH** | Backward-compatible bug fixes | `1.0.1` |

### VersionStrategy

Version updates of records are determined in workflow definitions. With the `VersionStrategy` property:

- Version change can be determined at each **transition**
- Version change can be determined for each **state** entry and exit

```json
{
  "key": "approve",
  "target": "approved",
  "versionStrategy": "Minor"
}
```

**Supported Strategies:**
- `Major`: Major version is incremented (1.0.0 → 2.0.0)
- `Minor`: Minor version is incremented (1.0.0 → 1.1.0)
- `Patch`: Patch version is incremented (1.0.0 → 1.0.1)

---

## ETag Usage

In addition to versioning, record changes are also managed with the [ETag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/ETag) approach.

### ETag Properties

- ETag value is always generated as **ULID**
- A new ETag is created with each record change
- Used for concurrent update control
- Can be used for client-side caching

### Usage Example

**Request:**
```http
GET /:domain/workflows/:flow/instances/:instanceId
If-None-Match: "01ARZ3NDEKTSV4RRFFQ69G5FAV"
```

**Response (if changed):**
```http
HTTP/1.1 200 OK
ETag: "01ARZ3NDEKTSV4RRFFQ69G5FAV"
Content-Type: application/json

{ ... }
```

**Response (if not changed):**
```http
HTTP/1.1 304 Not Modified
```

---

## Development Environment

### Assumptions

| Rule | Description |
|------|-------------|
| Version flexibility | Developer can assign any version number in development environment (Major.Minor.Patch) |
| Content update | Content can be updated without version update |
| Multiple versions | If an artifact will serve two different versions on runtime, a copy with incremented **major** is created |
| Package distribution | Distribution to local or remote runtime is done via package |
| Dependency management | Required reference packages during development are managed with npm |

### Test Constraint (vNext Runtime)

:::warning Important
Packages developed in vNext Runtime can **only be tested on the runtime**.
:::

- Developer cannot test without packaging and local distribution
- Test process must occur after deployment to runtime
- For each test, the package must be built, published to GitHub Packages, and deployed to runtime

---

## Package Management

### Package Repository

**GitHub Packages** is used for package distribution.

**Package Naming Format:** `vNext.<Domain Name>`

| Example | Description |
|---------|-------------|
| `vNext.Account` | Account domain package |
| `vNext.Customer` | Customer domain package |
| `vNext.Contract` | Contract domain package |

### Package Dependency Management

- Package dependencies are defined in the `package.json` file
- Dependency resolution is done automatically by npm
- In case of version conflicts, the highest compatible version is selected
- Reference integrity is maintained by pulling package references into the project

### Package Publishing Process

```
1. Package Development (Local Code Writing)
   ↓
2. Update Version in package.json
   ↓
3. Package Build
   ↓
4. Publish to GitHub Packages
   ↓
5. Deploy to Runtime (publish/package endpoint)
   ↓
6. Runtime Package Loading and Validation
   ↓
7. Test on Runtime (Required)
   ↓
8. Test Successful?
   ├─ No → Fix and Continue from Step 2
   └─ Yes → Package Published
```

---

## Runtime Deployment

### Assumptions

- Workflows and their dependencies are distributed to runtime via npm package manager based on `package.json`
- Package distribution is done via `publish` endpoint service that takes package parameter

### Environment Parameters

The following environment parameters must be provided when starting the runtime:

| Parameter | Description |
|-----------|-------------|
| `NPM_REGISTRY_URL` | GitHub Packages registry URL |
| `NPM_AUTH_TOKEN` | GitHub Packages authentication token |

### Distribution Service

Distribution is done via the `publish/package` endpoint:

```http
POST /publish/package
Content-Type: application/json

{
  "package": "vNext.Account",
  "version": "1.17.0"
}
```

---

## Version Format

### Extended Format

An extended version format is used for artifacts distributed in runtime:

**Format:** `MAJOR.MINOR.PATCH-pkg.PKG_VERSION+PKG_NAME`

### Format Components

| Component | Description | Example |
|-----------|-------------|---------|
| `MAJOR.MINOR.PATCH` | Artifact version | `1.0.0` |
| `-pkg.PKG_VERSION` | Package version (affects SemVer ordering) | `-pkg.1.17.0` |
| `+PKG_NAME` | Build metadata / Package name (does not affect ordering) | `+account` |

### Version Examples

| Version | Description |
|---------|-------------|
| `1.0.0-pkg.1.17.0+account` | Account package, core 1.0.0, package 1.17.0 |
| `2.1.3-pkg.2.5.1+customer` | Customer package, core 2.1.3, package 2.5.1 |
| `1.0.0-alpha.1-pkg.1.17.0+account` | Alpha pre-release version |
| `1.0.0-pkg.1.17.0+account+build.123` | With build metadata |

### Version Comparison

According to SemVer rules:
- `1.0.0-pkg.1.18.0 > 1.0.0-pkg.1.17.0` ✓
- `2.0.0-pkg.1.0.0 > 1.0.0-pkg.2.0.0` ✓
- Build metadata (`+`) does not affect ordering

---

## Test and Rollback Strategy

### Test Strategy

| Rule | Description |
|------|-------------|
| Runtime testing | Due to vNext Runtime architecture, all tests are performed on runtime |
| Deployment requirement | Deployment to runtime is required for each package change |
| Test environment | Using a separate runtime instance for test environment is recommended |
| Failed test | If test fails, rollback mechanism can be used to return to previous version |
| Successful test | If test succeeds, package can be deployed to production |

### Rollback Mechanism

- Previous version information is stored by runtime
- In case of error, rollback to previous version is possible
- Rollback is done via version parameter on `publish/package` endpoint

```http
POST /publish/package
Content-Type: application/json

{
  "package": "vNext.Account",
  "version": "1.16.0",
  "rollback": true
}
```

### Package Security Policies

| Policy | Description |
|--------|-------------|
| Signature verification | Package signatures are verified |
| Security scanning | Dependency security vulnerabilities are checked |
| Source control | Packages are only loaded from authorized sources |

---

## Related Documentation

- [📄 Reference Schema](./reference.md) - Inter-component reference management
- [📄 Persistence](./persistance.md) - Data storage and Dual-Write Pattern
- [📄 Workflow Definition](../flow/flow.md) - Workflow definition and development guide
