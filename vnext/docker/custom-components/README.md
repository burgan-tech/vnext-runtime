# Custom Components Directory

This directory allows you to add custom components to your VNext runtime without modifying the core `@burgan-tech/vnext-core-runtime` package.

## Directory Structure

Create your custom components following this structure:

```
custom-components/
├── Extensions/            # Custom extension definitions
├── Functions/             # Custom function definitions  
├── Schemas/              # Custom JSON schema definitions
├── Tasks/                # Custom task definitions
├── Views/                # Custom view components
└── Workflows/            # Custom workflow definitions
```

## JSON Schema Format

Custom component files should contain **only the data array** that will be merged into the core components:

```json
{
  "data": [
    {
      // ... another custom item
    }
  ]
}
```

**Important**: Custom component files should NOT include the full schema structure (key, version, domain, etc.). They should only contain the `data` array with your custom items.

## How It Works

1. **Core Component Loading**: The system loads core components from `@burgan-tech/vnext-core-runtime`
2. **Data Array Merging**: Custom components' `data` arrays are merged into the corresponding core component's `data` array
3. **File Matching**: Custom files are matched with core files by filename (e.g., `Functions/my-functions.json` merges with any core function file)
4. **Upload**: The merged component (core + custom data) is uploaded to the VNext API

## Usage with Docker

### Using docker-compose (recommended)

Set the `CUSTOM_COMPONENTS_PATH` environment variable to point to your custom components directory:

```bash
export CUSTOM_COMPONENTS_PATH=/path/to/your/custom-components
docker-compose up
```

Or add it to your `.env` file:

```env
CUSTOM_COMPONENTS_PATH=/path/to/your/custom-components
```

### Using the default path

If you don't set `CUSTOM_COMPONENTS_PATH`, it defaults to `./custom-components` relative to the docker-compose.yml file.

## Example

Create a custom function in `custom-components/Functions/my-functions.json`:

```json
{
  "data": [
    {
      
    }
  ]
}
```

This `data` array will be merged with the core function component's `data` array, adding your custom function to the available functions.

## File Naming

- Files can have any name within their respective folders
- All JSON files in a custom folder will be processed and merged into the corresponding core component
- Example: `Functions/business-logic.json`, `Functions/validators.json`, etc. will all be merged into the core Functions component
