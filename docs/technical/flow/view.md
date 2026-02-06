# View Management

Views are UI component definitions that represent the visual interface for workflow states and transitions. They provide a declarative way to define what users see at different stages of a workflow.

## Table of Contents

1. [View Definition](#view-definition)
2. [View Properties](#view-properties)
3. [View Reference Schema](#view-reference-schema)
4. [Display Strategy](#display-strategy)
5. [Display Modes](#display-modes)
6. [Platform Overrides](#platform-overrides)
7. [Usage Examples](#usage-examples)
8. [Best Practices](#best-practices)

## View Definition

A View is a domain entity that contains the UI definition for displaying workflow states or transitions. Views are versioned and can be referenced by workflows.

### Class Definition

```csharp
public sealed class View : IDomainEntity, IViewReference, IReferenceSetter
{
    public string Key { get; private set; }
    public string Flow { get; init; }
    public string Domain { get; private set; }
    public string Version { get; private set; }
    public ViewType Type { get; private set; }
    public string Content { get; private set; }
    public string Display { get; private set; }
    public LanguageLabel[]? Labels { get; private set; }
    public PlatformOverrides? PlatformOverrides { get; private set; }
}
```

## View Properties

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string` | Unique identifier for the view |
| `Flow` | `string` | Flow stream information (default: `sys-views`) |
| `Domain` | `string` | Domain to which the view belongs |
| `Version` | `string` | Version information (semantic versioning) |
| `Type` | `ViewType` | Type of view content (e.g., JSON, HTML, etc.) |
| `Content` | `string` | The actual view content/definition |
| `Display` | `string` | Display mode for rendering the view |
| `Labels` | `LanguageLabel[]` | Multi-language labels for the view |
| `PlatformOverrides` | `PlatformOverrides` | Platform-specific view overrides |

### Content Property

The `Content` property contains the actual UI definition. This can be:
- JSON schema for form definitions
- HTML templates
- Component references
- Custom UI definitions

### Display Property

Specifies how the view should be rendered. See [Display Modes](#display-modes) for available options.

### Labels Property

Multi-language support for view titles and descriptions:

```json
{
  "labels": [
    {
      "language": "en-US",
      "label": "Account Type Selection"
    },
    {
      "language": "tr-TR",
      "label": "Hesap Tipi Seçimi"
    }
  ]
}
```

## View Reference Schema

When referencing a view from a workflow state or transition, use the following schema:

### Full View Reference

```json
{
  "view": {
    "view": {
      "key": "account-type-selection-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true,
    "extensions": ["user-profile-ext", "account-limits-ext"]
  }
}
```

### Reference Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `view` | `IViewReference` | Yes | Reference to the view definition |
| `loadData` | `boolean` | No | Whether the view needs instance data (default: false) |
| `extensions` | `string[]` | No | Additional data extension keys for view enrichment |

#### loadData Flag

- **true**: The view requires access to workflow instance data. The platform will include instance data when rendering the view.
- **false**: The view is self-contained and doesn't need instance data.

#### extensions Array

Specifies additional data sources to enrich the view:
- Extension keys reference external data providers
- Extensions are loaded and merged with instance data
- Useful for supplementary information (user profiles, reference data, etc.)

## Display Strategy

The platform follows specific strategies for determining which view to render:

### State View Rendering

**Rule**: A state view is rendered when a state has multiple transitions.

```json
{
  "key": "account-selection",
  "stateType": 1,
  "view": {
    "view": {
      "key": "account-type-selection-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true
  },
  "transitions": [
    {
      "key": "select-savings",
      "target": "savings-account-details"
    },
    {
      "key": "select-checking",
      "target": "checking-account-details"
    }
  ]
}
```

In this example, the state has multiple transitions, so the `account-type-selection-view` is rendered to allow the user to choose.

### Transition View Rendering

**Rule**: Transition views are handled by the client and checked before transition submission.

Transition views are typically used for:
- Confirmation dialogs
- Additional data input
- Warning messages
- Terms and conditions acceptance

```json
{
  "key": "confirm-account-creation",
  "source": "account-details",
  "target": "account-created",
  "view": {
    "view": {
      "key": "account-confirmation-popup-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true
  }
}
```

**Client Behavior**:
1. Before submitting the transition, the client queries if a view exists
2. If a view exists, render it to the user
3. Wait for user confirmation/input
4. Submit the transition with any additional data

### Wizard State View Rendering

**Rule**: In wizard-type states, the transition view is displayed.

Wizard states accept only **one transition** and guide users through a step-by-step process:

```json
{
  "key": "wizard-step-1",
  "stateType": 1,
  "attributes": {
    "wizard": true
  },
  "transitions": [
    {
      "key": "next-step",
      "target": "wizard-step-2",
      "view": {
        "view": {
          "key": "wizard-step-1-input-view",
          "domain": "core",
          "version": "1.0.0",
          "flow": "sys-views"
        },
        "loadData": true
      }
    }
  ]
}
```

In wizard mode:
- The transition view is rendered immediately
- User provides required input
- Transition automatically proceeds upon valid submission

## Display Modes

The `display` property determines how the view is presented to the user. Available modes:

### 1. full-page

Renders the view as a full-page component, taking up the entire screen.

**Use Cases:**
- Main workflow screens
- Complex forms
- Dashboard views

```json
{
  "display": "full-page"
}
```

### 2. popup

Renders the view as a modal/popup dialog overlaying the current screen.

**Use Cases:**
- Confirmation dialogs
- Alert messages
- Short forms

```json
{
  "display": "popup"
}
```

### 3. bottom-sheet

Renders the view as a bottom sheet sliding up from the bottom of the screen.

**Use Cases:**
- Mobile-friendly selections
- Quick actions
- Filter options

```json
{
  "display": "bottom-sheet"
}
```

### 4. top-sheet

Renders the view as a top sheet sliding down from the top of the screen.

**Use Cases:**
- Notifications
- Success messages
- Quick information display

```json
{
  "display": "top-sheet"
}
```

### 5. drawer

Renders the view as a side drawer/menu sliding in from the side.

**Use Cases:**
- Navigation menus
- Settings panels
- Side filters

```json
{
  "display": "drawer"
}
```

### 6. inline

Renders the view inline within the current page content.

**Use Cases:**
- Embedded forms
- Inline editors
- Contextual information

```json
{
  "display": "inline"
}
```

## Platform Overrides

Platform overrides allow you to provide different view content for different platforms (web, iOS, Android).

### Definition

```csharp
public class PlatformOverrides
{
    public PlatformOverride? Android { get; private set; }
    public PlatformOverride? Ios { get; private set; }
    public PlatformOverride? Web { get; private set; }
}

public class PlatformOverride : ValueObject
{
    public string Content { get; private set; }
    public string Display { get; private set; }
    public ViewType? Type { get; private set; } = ViewType.Json;
}

public static class PlatformConst
{
    public const string Web = "web";
    public const string Ios = "ios";
    public const string Android = "android";
}
```

### Supported Platforms

The system supports three platform types:
- **web**: Web browsers
- **ios**: iOS mobile devices
- **android**: Android mobile devices

### Usage

The system automatically handles platform-specific content selection based on the `platform` query parameter:

**Request with Platform:**
```http
GET /core/workflows/account-opening/instances/123/functions/view?platform=ios
```

**View Definition with Override:**
```json
{
  "key": "account-selection-view",
  "content": "{...default content...}",
  "display": "full-page",
  "type": "json",
  "platformOverrides": {
    "ios": {
      "content": "{...iOS-optimized content...}",
      "display": "bottom-sheet",
      "type": "json"
    },
    "android": {
      "content": "{...Android-optimized content...}",
      "display": "bottom-sheet",
      "type": "json"
    },
    "web": {
      "content": "{...web-optimized content...}",
      "display": "full-page",
      "type": "json"
    }
  }
}
```

**System Behavior:**
- The system automatically determines which content to return based on the platform parameter
- If a platform override exists for the requested platform → returns the override content
- If no override exists → returns the original view content
- Client doesn't need to handle platform selection logic

## Usage Examples

### Example 1: Simple State View

```json
{
  "key": "account-type-selection-view",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-views",
  "type": "json",
  "content": "{\"type\":\"form\",\"fields\":[{\"name\":\"accountType\",\"type\":\"select\",\"options\":[\"savings\",\"checking\"]}]}",
  "display": "full-page",
  "labels": [
    {
      "language": "en-US",
      "label": "Select Account Type"
    }
  ]
}
```

### Example 2: Confirmation Popup

```json
{
  "key": "final-confirmation-popup-view",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-views",
  "type": "json",
  "content": "{\"type\":\"confirmation\",\"message\":\"Are you sure you want to proceed?\",\"actions\":[\"confirm\",\"cancel\"]}",
  "display": "popup",
  "labels": [
    {
      "language": "en-US",
      "label": "Confirm Action"
    }
  ]
}
```

### Example 3: Platform-Specific View

```json
{
  "key": "product-catalog-view",
  "domain": "core",
  "version": "1.0.0",
  "flow": "sys-views",
  "type": "json",
  "content": "{\"layout\":\"grid\",\"columns\":4}",
  "display": "full-page",
  "platformOverrides": {
    "ios": {
      "content": "{\"layout\":\"list\",\"columns\":1}",
      "display": "full-page",
      "type": "json"
    },
    "android": {
      "content": "{\"layout\":\"list\",\"columns\":1}",
      "display": "full-page",
      "type": "json"
    },
    "web": {
      "content": "{\"layout\":\"grid\",\"columns\":4}",
      "display": "full-page",
      "type": "json"
    }
  },
  "labels": [
    {
      "language": "en-US",
      "label": "Product Catalog"
    }
  ]
}
```

### Example 4: View with Extensions

```json
{
  "view": {
    "view": {
      "key": "user-dashboard-view",
      "domain": "core",
      "version": "1.0.0",
      "flow": "sys-views"
    },
    "loadData": true,
    "extensions": [
      "user-profile-extension",
      "recent-transactions-extension",
      "account-summary-extension"
    ]
  }
}
```

In this example:
- `loadData: true` ensures instance data is included
- Extensions provide additional data:
  - User profile information
  - Recent transaction history
  - Account summary details

## Best Practices

### 1. Keep Content Portable

Design view content to be platform-agnostic when possible. Use platform overrides only when necessary for optimal user experience on specific platforms (web, iOS, Android).

### 2. Use Appropriate Display Modes

Choose display modes based on:
- Amount of content
- User interaction required
- Platform conventions (mobile vs. web)
- Workflow context (primary vs. supplementary actions)

### 3. Leverage Extensions Wisely

Use extensions for:
- Data that changes independently of workflow
- Reference data (lookups, configurations)
- User-specific information
- Real-time data enrichment

Avoid extensions for:
- Core workflow data (use instance data instead)
- Data that must be versioned with the workflow

### 4. Implement Multi-Language Support

Always provide labels in all supported languages:

```json
{
  "labels": [
    {
      "language": "en-US",
      "label": "English Label"
    },
    {
      "language": "tr-TR",
      "label": "Türkçe Etiket"
    },
    {
      "language": "es-ES",
      "label": "Etiqueta en Español"
    }
  ]
}
```

### 5. Version Views Appropriately

- Use semantic versioning
- Create new versions for breaking changes
- Maintain backward compatibility when possible
- Document changes in version notes

### 6. Optimize for Mobile Platforms

When creating platform overrides for iOS and Android:
- Use bottom-sheet for selections and quick actions
- Simplify layouts (reduce columns, larger touch targets)
- Minimize text input requirements
- Consider thumb-reachable zones
- Test on both iOS and Android devices
- Respect platform-specific design guidelines (Material Design for Android, Human Interface Guidelines for iOS)

### 7. Test Display Strategies

Test your view rendering with:
- Single transition states
- Multiple transition states
- Wizard flows
- Different display modes
- Various screen sizes

### 8. Cache View Definitions

Views are cached using the pattern:
```
View:{Domain}:{Flow}:{Key}:{Version}
```

This enables fast retrieval and reduces database load.

## Related Documentation

- [State Management](./state.md) - Understanding workflow states
- [Transition Management](./transition.md) - Working with transitions
- [Function APIs](./function.md) - View function endpoint details
- [Interface Documentation](./interface.md) - Mapping interfaces

