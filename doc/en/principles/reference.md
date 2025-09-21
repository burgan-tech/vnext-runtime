# Reference Schema

Reference relationships can be established between workflow components within the vNext Platform. This concept can be thought of as a `Foreign Key` in a database and provides secure dependency management between components.

## Basic Reference Schema

The following standard schema is used to create references between all components (workflows, tasks, functions, extensions, schemas, views):

```json
{
  "key": "component-unique-key",
  "domain": "domain-name", 
  "version": "version-info",
  "flow": "component-module-type"
}
```

### Schema Fields

- **key**: Unique identifier of the component
- **domain**: Domain information where the component will run (e.g., "core", "account", "payment")
- **version**: Version information of the component (e.g., "1.0.0", "2.1.3")
- **flow**: Which module the component belongs to (e.g., "sys-flows", "sys-tasks", "sys-functions")

## Usage Areas

### 1. Task References

When referencing tasks in workflow states:

```json
{
  "onEntries": [
    {
      "order": 1,
      "task": {
        "key": "validate-client",
        "domain": "core",
        "version": "1.0.0",
        "flow": "sys-tasks"
      }
    }
  ]
}
```

### 2. SubFlow References

When a state runs a sub-workflow:

```json
{
  "subFlow": {
    "type": "S",
    "process": {
      "key": "password-subflow",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-flows"
    }
  }
}
```

### 3. Function References

When referencing custom functions:

```json
{
  "function": {
    "key": "calculate-interest",
    "domain": "banking",
    "version": "2.0.1",
    "flow": "sys-functions"
  }
}
```

## Flow Types

- **sys-flows**: Main workflows
- **sys-tasks**: Task definitions
- **sys-functions**: Custom functions
- **sys-extensions**: System extensions
- **sys-schemas**: Data schemas
- **sys-views**: View definitions

## Short Reference Usage (ref)

For more practical use during development, there is a **"ref"** short usage. During the build process, these references are automatically converted to full reference format.

### Local Definition Reference

When referencing files within the project:

```json
{
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "ref": "Tasks/invalidate-cache.json"
      }
    }
  ]
}
```

### Cross Project Reference

When referencing NPM packages or external definitions:

```json
{
  "onExecutionTasks": [
    {
      "order": 1,
      "task": {
        "ref": "@burgan-tech/vnext-core-reference/core/Tasks/invalidate-cache.json"
      }
    }
  ]
}
```

### Build-Time Conversion

During the build process, `ref` usages are automatically converted to full reference format:

**During development:**
```json
{
  "task": {
    "ref": "Tasks/validate-client.json"
  }
}
```

**After build:**
```json
{
  "task": {
    "key": "validate-client",
    "domain": "core",
    "version": "1.0.0", 
    "flow": "sys-tasks"
  }
}
```

### Ref Usage Advantages

- ✅ **Fast development**: Short and understandable syntax
- ✅ **Automatic resolution**: Converted to full reference during build
- ✅ **IntelliSense support**: File path autocomplete in IDE
- ✅ **Local/external flexibility**: Support for both local and NPM packages

## Reference Resolution

During runtime, references are resolved in the following order:

1. Component is searched by key and domain combination
2. Appropriate version is selected according to version strategy
3. Flow type is validated and component is loaded
4. Dependency graph is created

## Error Management

When reference cannot be resolved:

- **Missing Component**: Component not found
- **Version Mismatch**: Version incompatibility
- **Domain Error**: Domain access error
- **Circular Reference**: Circular dependency

## Best Practices

1. **Explicit versioning**: Use full version for critical references
2. **Domain separation**: Collect related components in the same domain
3. **Circular dependency**: Do not create loops in reference chains
4. **Backward compatibility**: Maintain API compatibility in version changes
