#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const Ajv7 = require('ajv');
const Ajv2019 = require('ajv/dist/2019');
const addFormats = require('ajv-formats');

// ANSI color codes for terminal output
const colors = {
  reset: '\x1b[0m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m',
  bright: '\x1b[1m',
  dim: '\x1b[2m'
};

// Helper function to colorize text
function colorize(text, color) {
  return `${colors[color]}${text}${colors.reset}`;
}

// Helper function to create clickable file links (VS Code terminal format)
function createFileLink(filePath, line = null, column = null) {
  const absolutePath = path.resolve(filePath);
  let link = `file://${absolutePath}`;
  if (line !== null) {
    link += `:${line}`;
    if (column !== null) {
      link += `:${column}`;
    }
  }
  return link;
}

// Helper function to parse line/column from error messages
function parseErrorLocation(errorMessage) {
  // Match patterns like "line 94 column 107" or "at position 3200 (line 94 column 107)"
  const lineMatch = errorMessage.match(/line\s+(\d+)/i);
  const columnMatch = errorMessage.match(/column\s+(\d+)/i);
  
  if (lineMatch) {
    return {
      line: parseInt(lineMatch[1], 10),
      column: columnMatch ? parseInt(columnMatch[1], 10) : null
    };
  }
  return null;
}

// Helper function to find line number for a JSON path in a file
function findLineNumberForPath(filePath, jsonPath) {
  try {
    const fileContent = fs.readFileSync(filePath, 'utf8');
    const lines = fileContent.split('\n');
    
    // Parse the JSON path (e.g., "/attributes/states/0/transitions/1")
    const pathParts = jsonPath.split('/').filter(part => part.length > 0);
    
    if (pathParts.length === 0) {
      return null;
    }
    
    // Parse JSON to get actual structure
    let jsonData;
    try {
      jsonData = JSON.parse(fileContent);
    } catch (e) {
      // If JSON parsing fails, fall back to text search
      return findLineNumberByTextSearch(lines, pathParts);
    }
    
    // Navigate through the JSON structure to find the path
    let current = jsonData;
    for (let i = 0; i < pathParts.length; i++) {
      const part = pathParts[i];
      
      // Check if part is an array index
      if (/^\d+$/.test(part)) {
        const index = parseInt(part, 10);
        if (Array.isArray(current) && index < current.length) {
          current = current[index];
        } else {
          return null;
        }
      } else if (current && typeof current === 'object' && part in current) {
        current = current[part];
      } else {
        // Path doesn't exist, try text search for the last part
        return findLineNumberByTextSearch(lines, pathParts.slice(i));
      }
    }
    
    // Now find the line number where this value appears
    return findValueLineNumber(lines, pathParts, jsonData);
  } catch (error) {
    // Fallback to text search
    try {
      const fileContent = fs.readFileSync(filePath, 'utf8');
      const lines = fileContent.split('\n');
      const pathParts = jsonPath.split('/').filter(part => part.length > 0);
      return findLineNumberByTextSearch(lines, pathParts);
    } catch (e) {
      return null;
    }
  }
}

// Helper to find line number by searching for property names in text
function findLineNumberByTextSearch(lines, pathParts) {
  if (pathParts.length === 0) return null;
  
  const targetProperty = pathParts[pathParts.length - 1];
  
  // For array indices, search for the parent property
  if (/^\d+$/.test(targetProperty)) {
    if (pathParts.length > 1) {
      const parentProperty = pathParts[pathParts.length - 2];
      // Search for parent property and count array elements
      let foundParent = false;
      let arrayDepth = 0;
      let elementIndex = 0;
      const targetIndex = parseInt(targetProperty, 10);
      
      for (let lineNum = 0; lineNum < lines.length; lineNum++) {
        const line = lines[lineNum];
        
        if (line.includes(`"${parentProperty}"`)) {
          foundParent = true;
        }
        
        if (foundParent) {
          if (line.includes('[')) {
            arrayDepth++;
          }
          if (line.includes(']')) {
            arrayDepth--;
            if (arrayDepth === 0 && elementIndex === targetIndex) {
              return lineNum + 1;
            }
            if (arrayDepth === 0) {
              elementIndex++;
            }
          }
          if (arrayDepth > 0 && line.includes('{') && elementIndex === targetIndex) {
            return lineNum + 1;
          }
        }
      }
    }
    return null;
  }
  
  // For regular properties, search for the property name
  for (let lineNum = 0; lineNum < lines.length; lineNum++) {
    const line = lines[lineNum];
    // Look for "propertyName": pattern
    if (line.match(new RegExp(`"${targetProperty.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"\\s*:`))) {
      return lineNum + 1;
    }
  }
  
  return null;
}

// Helper to find line number of a value in JSON structure
function findValueLineNumber(lines, pathParts, jsonData) {
  // Navigate to the value
  let current = jsonData;
  for (const part of pathParts) {
    if (/^\d+$/.test(part)) {
      current = current[parseInt(part, 10)];
    } else {
      current = current[part];
    }
  }
  
  // Search for the property name in the file
  const targetProperty = pathParts[pathParts.length - 1];
  
  // For array indices, find the array element
  if (/^\d+$/.test(targetProperty)) {
    const parentProperty = pathParts[pathParts.length - 2];
    return findArrayElementLine(lines, parentProperty, parseInt(targetProperty, 10));
  }
  
  // For regular properties, find the property line
  for (let lineNum = 0; lineNum < lines.length; lineNum++) {
    if (lines[lineNum].match(new RegExp(`"${targetProperty.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"\\s*:`))) {
      return lineNum + 1;
    }
  }
  
  return null;
}

// Helper to find line number of an array element
function findArrayElementLine(lines, arrayProperty, elementIndex) {
  let foundArray = false;
  let bracketDepth = 0;
  let currentIndex = 0;
  
  for (let lineNum = 0; lineNum < lines.length; lineNum++) {
    const line = lines[lineNum];
    
    // Find the array property
    if (!foundArray && line.includes(`"${arrayProperty}"`)) {
      foundArray = true;
      continue;
    }
    
    if (foundArray) {
      // Count brackets to track array depth
      for (const char of line) {
        if (char === '[') {
          bracketDepth++;
          if (bracketDepth === 1) {
            // Start of array
            continue;
          }
        } else if (char === ']') {
          bracketDepth--;
          if (bracketDepth === 0) {
            // End of array
            break;
          }
        } else if (bracketDepth === 1 && char === '{') {
          // Found an object in the array
          if (currentIndex === elementIndex) {
            return lineNum + 1;
          }
          currentIndex++;
        }
      }
      
      // Check if we're at the target index
      if (bracketDepth === 1 && currentIndex === elementIndex && line.trim().startsWith('{')) {
        return lineNum + 1;
      }
    }
  }
  
  return null;
}

// Helper function to find line number for error in JSON file
function findErrorLineNumber(filePath, err) {
  try {
    const fileContent = fs.readFileSync(filePath, 'utf8');
    const lines = fileContent.split('\n');
    
    // Get the error path
    const errPath = err.instancePath || err.dataPath || '';
    
    if (!errPath) {
      // For root-level errors (like additionalProperty), search for the property
      if (err.params && err.params.additionalProperty) {
        const prop = err.params.additionalProperty;
        for (let i = 0; i < lines.length; i++) {
          if (lines[i].match(new RegExp(`"${prop.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}"\\s*:`))) {
            return i + 1;
          }
        }
      }
      return null;
    }
    
    // Parse JSON path and find the line where the property appears
    const pathParts = errPath.split('/').filter(part => part.length > 0);
    
    if (pathParts.length === 0) return null;
    
    // For nested paths, find the line where the target property/object appears
    // Navigate through the path to find the actual line in the file
    let currentDepth = 0;
    let pathIndex = 0;
    let inString = false;
    let escapeNext = false;
    let currentKey = '';
    let bracketDepth = 0;
    let braceDepth = 0;
    let arrayIndex = 0;
    let foundPath = false;
    
    for (let lineNum = 0; lineNum < lines.length; lineNum++) {
      const line = lines[lineNum];
      
      for (let i = 0; i < line.length; i++) {
        const char = line[i];
        
        if (escapeNext) {
          escapeNext = false;
          continue;
        }
        
        if (char === '\\') {
          escapeNext = true;
          continue;
        }
        
        if (char === '"' && !escapeNext) {
          inString = !inString;
          if (!inString && currentKey) {
            // Check if this key matches our path
            if (pathIndex < pathParts.length && currentKey === pathParts[pathIndex]) {
              pathIndex++;
              if (pathIndex === pathParts.length) {
                // Found the target path
                return lineNum + 1;
              }
            }
            currentKey = '';
          }
          continue;
        }
        
        if (!inString) {
          if (char === '{') {
            braceDepth++;
          } else if (char === '}') {
            braceDepth--;
            if (braceDepth < currentDepth) {
              // Reset path tracking when exiting a level
              if (pathIndex > 0 && braceDepth < pathIndex) {
                pathIndex = Math.max(0, pathIndex - 1);
              }
            }
          } else if (char === '[') {
            bracketDepth++;
            // Check if we're at an array index in the path
            if (pathIndex < pathParts.length && /^\d+$/.test(pathParts[pathIndex])) {
              const targetIndex = parseInt(pathParts[pathIndex], 10);
              if (arrayIndex === targetIndex && bracketDepth === 1) {
                pathIndex++;
                if (pathIndex === pathParts.length) {
                  return lineNum + 1;
                }
              }
            }
          } else if (char === ']') {
            bracketDepth--;
            if (bracketDepth === 0) {
              arrayIndex++;
            }
          } else if (char === ':' && currentKey) {
            currentKey = '';
          } else if (char.match(/[a-zA-Z0-9_]/) && !inString) {
            if (i === 0 || line[i-1] === '"' || (i > 0 && line[i-1].match(/[^a-zA-Z0-9_]/))) {
              currentKey += char;
            }
          }
        } else {
          if (char.match(/[a-zA-Z0-9_]/)) {
            currentKey += char;
          }
        }
      }
    }
    
    // Fallback: search for the last property in the path
    const lastProperty = pathParts[pathParts.length - 1];
    if (!/^\d+$/.test(lastProperty)) {
      for (let i = 0; i < lines.length; i++) {
        if (lines[i].includes(`"${lastProperty}"`)) {
          return i + 1;
        }
      }
    }
    
    return null;
  } catch (error) {
    return null;
  }
}

// Helper function to format error message with better handling of additionalProperty
function formatErrorMessage(err, filePath = null) {
  let message = err.message;
  let lineNumber = null;
  
  // Find line number in the JSON file where the error occurs
  if (filePath) {
    lineNumber = findErrorLineNumber(filePath, err);
  }
  
  // Handle "must NOT have additional properties" with better formatting
  if (message.includes('must NOT have additional properties') && err.params && err.params.additionalProperty) {
    const prop = err.params.additionalProperty;
    message = `must NOT have additional property ${colorize(`"${prop}"`, 'yellow')}`;
  }
  
  // Handle "must have required property" with better formatting
  if (message.includes('must have required property') && err.params && err.params.missingProperty) {
    const prop = err.params.missingProperty;
    message = `must have required property ${colorize(`"${prop}"`, 'yellow')}`;
  }
  
  // Add line number if found
  if (lineNumber !== null) {
    message += ` ${colorize(`(line ${lineNumber})`, 'dim')}`;
  }
  
  return message;
}

// Validation script for vnext-template package
console.log('🔍 Running vnext-template validation...');

let validationsPassed = 0;
let validationsFailed = 0;

// Track schema validation statistics
let schemaValidationStats = {
  filesValidated: 0,
  filesPassed: 0,
  filesFailed: 0,
  enabled: false,
  passedFiles: [],
  failedFiles: []
};

function validate(description, validationFunction) {
  try {
    console.log(`\n🔍 Validating: ${description}`);
    const result = validationFunction();
    if (result !== false) {
      console.log('✅ VALID');
      validationsPassed++;
    } else {
      console.log('❌ INVALID');
      validationsFailed++;
    }
  } catch (error) {
    console.log(`❌ INVALID: ${error.message}`);
    validationsFailed++;
  }
}

// Validation 1: Package.json structure and content
validate('Package.json structure and content', () => {
  const packageJson = JSON.parse(fs.readFileSync('./package.json', 'utf8'));
  
  // Required fields (publishConfig is optional during publishing process)
  const requiredFields = ['name', 'version', 'description', 'main', 'author', 'license', 'repository'];
  for (const field of requiredFields) {
    if (!packageJson[field]) {
      throw new Error(`Missing required field: ${field}`);
    }
  }
  
  // PublishConfig is optional during publishing (workflow may remove it temporarily)
  if (packageJson.publishConfig) {
    console.log(`  ✓ PublishConfig present`);
  } else {
    console.log(`  ⚠ PublishConfig not present (may be removed during publishing)`);
  }
  
  // Scope validation
  if (!packageJson.name.startsWith('@burgan-tech/')) {
    throw new Error('Package name must use @burgan-tech scope');
  }
  
  // Version validation
  if (!/^\d+\.\d+\.\d+/.test(packageJson.version)) {
    throw new Error('Version must follow semantic versioning');
  }
  
  // Main file validation
  if (!fs.existsSync(packageJson.main)) {
    throw new Error(`Main file ${packageJson.main} does not exist`);
  }
  
  // PublishConfig validation (if present)
  if (packageJson.publishConfig) {
    if (!packageJson.publishConfig.registry || !packageJson.publishConfig.access) {
      throw new Error('publishConfig must include registry and access');
    }
    console.log(`  ✓ PublishConfig is valid`);
  }
  
  console.log(`  ✓ Package name: ${packageJson.name}`);
  console.log(`  ✓ Version: ${packageJson.version}`);
  console.log(`  ✓ Main file: ${packageJson.main}`);
  
  return true;
});

// Validation 2: Main entry point functionality
validate('Main entry point functionality', () => {
  const mainFile = JSON.parse(fs.readFileSync('./package.json', 'utf8')).main;
  
  // Check if file exists and is readable
  if (!fs.existsSync(mainFile)) {
    throw new Error(`Main file ${mainFile} does not exist`);
  }
  
  // Try to require the module
  let vnextTemplate;
  try {
    vnextTemplate = require('./' + mainFile);
  } catch (error) {
    throw new Error(`Failed to require main file: ${error.message}`);
  }
  
  // Check required exports
  const requiredExports = [
    'getDomainConfig',
    'getSchemas',
    'getWorkflows',
    'getTasks',
    'getViews',
    'getFunctions',
    'getExtensions',
    'getAvailableTypes',
    'getDomainName'
  ];
  
  for (const exportName of requiredExports) {
    if (typeof vnextTemplate[exportName] !== 'function') {
      throw new Error(`Missing or invalid export: ${exportName}`);
    }
  }
  
  console.log(`  ✓ All required exports present`);
  return true;
});

// Validation 3: vnext.config.json validation
validate('vnext.config.json validation', () => {
  if (fs.existsSync('./vnext.config.json')) {
    const config = JSON.parse(fs.readFileSync('./vnext.config.json', 'utf8'));
    
    if (typeof config !== 'object' || config === null) {
      throw new Error('vnext.config.json must contain a valid JSON object');
    }
    
    console.log(`  ✓ vnext.config.json is valid JSON`);
  } else {
    console.log(`  ✓ vnext.config.json not present (optional)`);
  }
  
  return true;
});

// Validation 4: Domain directory structure
validate('Domain directory structure', () => {
  const vnextTemplate = require('./index.js');
  const domainName = vnextTemplate.getDomainName();
  const pathsConfig = vnextTemplate.getPathsConfig();
  
  if (!domainName) {
    console.log(`  ⚠ No domain directory found (template will be empty)`);
    return true;
  }
  
  if (!fs.existsSync(domainName)) {
    throw new Error(`Domain directory $core does not exist`);
  }
  
  // Check for vnext structure directories using paths config
  const vnextDirs = [
    pathsConfig.schemas,
    pathsConfig.workflows,
    pathsConfig.tasks,
    pathsConfig.views,
    pathsConfig.functions,
    pathsConfig.extensions
  ];
  const existingDirs = vnextDirs.filter(dir => fs.existsSync(path.join(domainName, dir)));
  
  if (existingDirs.length === 0) {
    throw new Error('No vnext structure directories found in domain directory');
  }
  
  console.log(`  ✓ Domain directory: $core`);
  console.log(`  ✓ Found directories: ${existingDirs.join(', ')}`);
  
  return true;
});

// Validation 5: JSON files syntax validation
validate('JSON files syntax validation', () => {
  const vnextTemplate = require('./index.js');
  const domainName = vnextTemplate.getDomainName();
  
  let jsonFileCount = 0;
  
  if (domainName && fs.existsSync(domainName)) {
    const validateJsonInDir = (dirPath) => {
      const entries = fs.readdirSync(dirPath, { withFileTypes: true });
      
      for (const entry of entries) {
        // Skip .meta directory
        if (entry.name === '.meta') {
          continue;
        }
        
        const fullPath = path.join(dirPath, entry.name);
        
        if (entry.isDirectory()) {
          validateJsonInDir(fullPath);
        } else if (entry.isFile() && entry.name.endsWith('.json')) {
          jsonFileCount++;
          try {
            JSON.parse(fs.readFileSync(fullPath, 'utf8'));
          } catch (error) {
            throw new Error(`Invalid JSON in ${fullPath}: ${error.message}`);
          }
        }
      }
    };
    
    validateJsonInDir(domainName);
  }
  
  // Also validate root JSON files
  const rootJsonFiles = ['package.json', 'vnext.config.json'].filter(file => fs.existsSync(file));
  for (const file of rootJsonFiles) {
    jsonFileCount++;
    try {
      JSON.parse(fs.readFileSync(file, 'utf8'));
    } catch (error) {
      throw new Error(`Invalid JSON in ${file}: ${error.message}`);
    }
  }
  
  console.log(`  ✓ Validated ${jsonFileCount} JSON files`);
  return true;
});

// Validation 5b: JSON files schema validation using @burgan-tech/vnext-schema
validate('JSON files schema validation using @burgan-tech/vnext-schema', () => {
  let vnextSchema;
  try {
    vnextSchema = require('@burgan-tech/vnext-schema');
    schemaValidationStats.enabled = true;
  } catch (error) {
    console.log(`  ⚠ @burgan-tech/vnext-schema package not available: ${error.message}`);
    console.log(`  ⚠ Skipping schema validation (syntax validation still performed)`);
    return true; // Don't fail if package is not available
  }

  const vnextTemplate = require('./index.js');
  const domainName = vnextTemplate.getDomainName();
  const pathsConfig = vnextTemplate.getPathsConfig();
  
  if (!domainName || !fs.existsSync(domainName)) {
    console.log(`  ⚠ No domain directory found, skipping schema validation`);
    return true;
  }

  // Initialize AJV instances for draft-07 and draft 2019-09
  const ajv7 = new Ajv7({ strict: false, allErrors: true, verbose: true });
  addFormats(ajv7);

  const ajv2019 = new Ajv2019({ strict: false, allErrors: true, verbose: true });
  addFormats(ajv2019);

  // Pick the correct AJV instance based on the schema's $schema field
  function getAjvInstance(schema) {
    const draft = schema?.$schema || '';
    return draft.includes('2019-09') ? ajv2019 : ajv7;
  }

  // Map directory names to schema types dynamically from paths config
  const directoryToSchemaType = {
    [pathsConfig.schemas]: 'schema',
    [pathsConfig.workflows]: 'workflow',
    [pathsConfig.tasks]: 'task',
    [pathsConfig.views]: 'view',
    [pathsConfig.functions]: 'function',
    [pathsConfig.extensions]: 'extension'
  };

  // Get available schema types
  const availableTypes = vnextSchema.getAvailableTypes ? vnextSchema.getAvailableTypes() : [];
  
  // Compile validators for each schema type
  const validators = {};
  for (const [dirName, schemaType] of Object.entries(directoryToSchemaType)) {
    if (availableTypes.includes(schemaType)) {
      try {
        const schema = vnextSchema.getSchema ? vnextSchema.getSchema(schemaType) : null;
        if (schema) {
          validators[dirName] = {
            validator: getAjvInstance(schema).compile(schema),
            type: schemaType
          };
        }
      } catch (error) {
        console.log(`  ⚠ Warning: Could not compile validator for ${schemaType}: ${error.message}`);
      }
    }
  }

  if (Object.keys(validators).length === 0) {
    console.log(`  ⚠ No validators available, skipping schema validation`);
    return true;
  }

  let validatedCount = 0;
  let errorCount = 0;
  const errors = [];
  const passedFiles = [];

  // Validate JSON files against schemas
  const validateJsonAgainstSchema = (dirPath, domainPath) => {
    const entries = fs.readdirSync(dirPath, { withFileTypes: true });
    
    for (const entry of entries) {
      // Skip .meta directory
      if (entry.name === '.meta') {
        continue;
      }
      
      const fullPath = path.join(dirPath, entry.name);
      
      if (entry.isDirectory()) {
        validateJsonAgainstSchema(fullPath, domainPath);
      } else if (entry.isFile() && entry.name.endsWith('.json')) {
        // Determine which schema type to use based on directory path
        // Check if any of the parent directories match our schema directories
        let validator = null;
        let schemaType = null;
        
        const relativePath = path.relative(domainPath, path.dirname(fullPath));
        const pathParts = relativePath.split(path.sep);
        
        // Find the first matching directory in the path
        for (const part of pathParts) {
          if (validators[part]) {
            validator = validators[part];
            schemaType = validator.type;
            break;
          }
        }
        
        if (validator) {
          validatedCount++;
          try {
            const jsonContent = JSON.parse(fs.readFileSync(fullPath, 'utf8'));
            const valid = validator.validator(jsonContent);
            
            if (valid) {
              // Track passed files
              passedFiles.push({
                file: fullPath,
                type: schemaType
              });
            } else {
              errorCount++;
              const validationErrors = validator.validator.errors || [];
              const errorMessages = validationErrors.map(err => {
                const errPath = err.instancePath || err.dataPath || '';
                const pathColor = errPath ? 'cyan' : 'dim';
                const messageColor = 'red';
                const paramsColor = 'yellow';
                
                // Format error message with better handling and line numbers
                const formattedMessage = formatErrorMessage(err, fullPath);
                
                let output = '';
                if (errPath) {
                  output += colorize(errPath, pathColor) + ': ';
                }
                output += colorize(formattedMessage, messageColor);
                
                // Show params if they exist and aren't already included in the message
                if (err.params && !err.params.additionalProperty && !err.params.missingProperty) {
                  output += ' ' + colorize('(' + JSON.stringify(err.params) + ')', paramsColor);
                }
                
                return `  ${output}`;
              }).join('\n');
              
              errors.push({
                file: fullPath,
                type: schemaType,
                message: `Schema validation failed for ${schemaType}:\n${errorMessages}`
              });
            }
          } catch (error) {
            errorCount++;
            // Parse line/column from JSON parse errors
            const location = parseErrorLocation(error.message);
            
            errors.push({
              file: fullPath,
              type: schemaType || 'unknown',
              message: `Error validating file: ${error.message}`,
              location: location
            });
          }
        }
      }
    }
  };
  
  validateJsonAgainstSchema(domainName, domainName);
  
  // Update schema validation statistics
  schemaValidationStats.filesValidated = validatedCount;
  schemaValidationStats.filesPassed = passedFiles.length;
  schemaValidationStats.filesFailed = errorCount;
  schemaValidationStats.passedFiles = passedFiles;
  schemaValidationStats.failedFiles = errors;
  
  // Display failed files if any
  if (errorCount > 0) {
    console.log(colorize(`  ❌ Schema validation failed for ${errorCount} file(s):`, 'red'));
    errors.forEach(err => {
      console.log(`\n    ${colorize('File:', 'bright')} ${err.file}`);
      if (err.location) {
        console.log(`    ${colorize('Location:', 'bright')} line ${colorize(err.location.line, 'yellow')}${err.location.column ? `, column ${colorize(err.location.column, 'yellow')}` : ''}`);
      }
      console.log(`    ${colorize('Type:', 'bright')} ${colorize(err.type, 'magenta')}`);
      console.log(`    ${err.message}`);
    });
  }
  
  // Display passed files similar to failed files (always show if any passed)
  if (passedFiles.length > 0) {
    if (errorCount > 0) {
      console.log(''); // Add spacing between failed and passed sections
    }
    console.log(colorize(`  ✓ Schema validation passed for ${passedFiles.length} file(s):`, 'green'));
    passedFiles.forEach(passed => {
      console.log(`\n    ${colorize('File:', 'bright')} ${passed.file}`);
      console.log(`    ${colorize('Type:', 'bright')} ${colorize(passed.type, 'magenta')}`);
      console.log(`    ${colorize('✓ Valid', 'green')}`);
    });
  } else if (validatedCount === 0) {
    console.log(`  ⚠ No files found to validate against schemas`);
  }
  
  // Throw error if there are validation failures
  if (errorCount > 0) {
    throw new Error(`Schema validation failed for ${errorCount} file(s)`);
  }
  
  return true;
});

// Validation 6: Module functionality test
validate('Module functionality test', () => {
  const vnextTemplate = require('./index.js');
  
  // Test basic functionality
  const pathsConfig = vnextTemplate.getPathsConfig();
  const availableTypes = vnextTemplate.getAvailableTypes();
  
  if (!Array.isArray(availableTypes)) {
    throw new Error('getAvailableTypes must return an array');
  }
  
  // Verify availableTypes contains expected directory names from paths config
  const expectedDirs = [
    pathsConfig.schemas,
    pathsConfig.workflows,
    pathsConfig.tasks,
    pathsConfig.views,
    pathsConfig.functions,
    pathsConfig.extensions
  ];
  
  for (const dir of expectedDirs) {
    if (!availableTypes.includes(dir)) {
      throw new Error(`Missing expected directory in availableTypes: ${dir}`);
    }
  }
  
  // Test component getters
  const componentGetters = [
    'getSchemas',
    'getWorkflows', 
    'getTasks',
    'getViews',
    'getFunctions',
    'getExtensions'
  ];
  
  for (const getter of componentGetters) {
    const result = vnextTemplate[getter]();
    if (typeof result !== 'object' || result === null) {
      throw new Error(`${getter} must return an object`);
    }
  }
  
  console.log(`  ✓ All component getters working`);
  console.log(`  ✓ Available types: ${availableTypes.join(', ')}`);
  console.log(`  ✓ Paths config loaded from vnext.config.json`);
  
  return true;
});

// Validation 7: Files array in package.json
validate('Files array in package.json', () => {
  const packageJson = JSON.parse(fs.readFileSync('./package.json', 'utf8'));
  
  if (!packageJson.files || !Array.isArray(packageJson.files)) {
    throw new Error('package.json must include a files array');
  }
  
  // Check that critical files are included
  const criticalFiles = ['index.js', 'package.json'];
  for (const file of criticalFiles) {
    if (!packageJson.files.includes(file)) {
      throw new Error(`Critical file ${file} not included in files array`);
    }
  }
  
  // Verify files exist (except for templated paths)
  for (const file of packageJson.files) {
    if (!file.includes('{') && !fs.existsSync(file)) {
      console.log(`  ⚠ Warning: File ${file} listed in files array but does not exist`);
    }
  }
  
  console.log(`  ✓ Files array contains ${packageJson.files.length} entries`);
  return true;
});

// Validation 8: Semantic versioning compliance
validate('Semantic versioning compliance', () => {
  const packageJson = JSON.parse(fs.readFileSync('./package.json', 'utf8'));
  const version = packageJson.version;
  
  const semverRegex = /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$/;
  
  if (!semverRegex.test(version)) {
    throw new Error(`Version ${version} does not follow semantic versioning`);
  }
  
  console.log(`  ✓ Version ${version} follows semantic versioning`);
  return true;
});

// Print validation results
console.log('\n' + '═'.repeat(60));
console.log('📊 Validation Results:');
console.log('═'.repeat(60));
console.log(`   ✅ Passed: ${colorize(validationsPassed, 'green')}`);
console.log(`   ❌ Failed: ${colorize(validationsFailed, 'red')}`);
console.log(`   📈 Total:  ${validationsPassed + validationsFailed}`);

// Print schema validation statistics if enabled
if (schemaValidationStats.enabled) {
  console.log('\n' + '─'.repeat(60));
  console.log('📋 Schema Validation Statistics:');
  console.log('─'.repeat(60));
  console.log(`   Files validated: ${colorize(schemaValidationStats.filesValidated, 'cyan')}`);
  console.log(`   ${colorize('✓ Passed:', 'green')} ${colorize(schemaValidationStats.filesPassed, 'green')}`);
  console.log(`   ${colorize('✗ Failed:', 'red')} ${colorize(schemaValidationStats.filesFailed, 'red')}`);
  
  // Print failed files summary if any
  if (schemaValidationStats.failedFiles.length > 0) {
    console.log('\n' + '─'.repeat(60));
    console.log(colorize('❌ Failed Files Summary:', 'red'));
    console.log('─'.repeat(60));
    
    schemaValidationStats.failedFiles.forEach((err, index) => {
      const fileName = path.basename(err.file);
      const relativePath = err.file;
      
      if (index > 0) {
        console.log(''); // Line break between files
      }
      
      console.log(`   ${colorize((index + 1) + '.', 'bright')} ${colorize(fileName, 'yellow')}`);
      console.log(`      Path: ${colorize(relativePath, 'dim')}`);
      console.log(`      Type: ${colorize(err.type, 'magenta')}`);
      
      if (err.location) {
        console.log(`      Line: ${colorize(err.location.line, 'cyan')}${err.location.column ? `, Column: ${colorize(err.location.column, 'cyan')}` : ''}`);
      }
    });
    
    console.log('─'.repeat(60));
  }
}

console.log('═'.repeat(60));

if (validationsFailed > 0) {
  console.log('\n❌ Validation failed! Please fix the issues above.');
  process.exit(1);
} else {
  console.log('\n🎉 All validations passed! Package is ready for publishing.');
  process.exit(0);
}
