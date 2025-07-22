# BlockFarmEditor.USync

uSync integration package for BlockFarmEditor that provides automatic synchronization of block definitions across Umbraco environments.

## Installation

### Prerequisites

- **Umbraco CMS**: Version 16+ (compatible with .NET 9+)
- **BlockFarmEditor.RCL**: Core BlockFarmEditor package
- **uSync**: Version 16+ for BackOffice integration

### NuGet Package Installation

```bash
dotnet add package BlockFarmEditor.USync
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package BlockFarmEditor.USync
```
### Verification

After installation, you can verify the integration is working by:

1. **Check Composition**: The `BFEuSyncComposer` should automatically register the notification handlers
2. **Export Test**: Run a uSync export and check for the `BlockFarmEditorDefinitions` folder in your uSync directory
3. **Logs**: Monitor Umbraco logs for any uSync-related messages during startup

## Overview

This package extends the BlockFarmEditor with uSync capabilities, allowing block definitions to be automatically exported and imported alongside content types during uSync operations.

### Automatic Registration

The package includes a `BFEuSyncComposer` that automatically registers the following notification handlers:

- `BlockFarmEditorDefinitionExport` - Handles export operations
- `BlockFarmEditorDefinitionImport` - Handles import operations  
- `BlockFarmEditorDefinitionImported` - Handles cache clearing after import completion

No manual registration is required - the composer will automatically wire up the handlers when Umbraco starts.

## Features

- **Automatic Export**: Block definitions are automatically exported when element content types are synced
- **Automatic Import**: Block definitions are imported and synchronized when content types are restored
- **File-based Storage**: Definitions are stored as JSON files in the uSync folder structure
- **Conflict Resolution**: Handles updates to existing definitions during import operations

## How It Works

### Export Process

When uSync exports a content type that is marked as an element type (`IsElement = true`), the `BlockFarmEditorDefinitionExport` handler:

1. Detects element content types during export
2. Retrieves the corresponding block definition from the database
3. Serializes the definition to JSON
4. Saves it to `uSync/Root/BlockFarmEditorDefinitions/{alias}.json`

### Import Process

When uSync imports a content type XML file, the `BlockFarmEditorDefinitionImport` handler:

1. Parses the XML to identify element content types
2. Checks for a corresponding JSON definition file
3. Deserializes the definition and either:
   - Updates an existing definition if one exists
   - Creates a new definition if none exists

## File Structure

After export, your uSync folder will contain:

```
uSync/
├── v16/
│   └── BlockFarmEditorDefinitions/
│       ├── alert.json
│       ├── hero.json
│       └── textBlock.json
```

## Event Handlers

### BlockFarmEditorDefinitionExport

- **Triggers on**: `uSyncItemNotification<ContentType>`
- **Condition**: `notification.Item?.IsElement == true`
- **Action**: Exports block definition to JSON file

### BlockFarmEditorDefinitionImport

- **Triggers on**: `uSyncImportedItemNotification`
- **Condition**: ContentType XML with `<IsElement>true</IsElement>`
- **Action**: Imports/updates block definition from JSON file

## XML Structure Parsing

The import handler correctly parses Umbraco's content type XML structure:

```xml
<ContentType Alias="alert">
  <Info>
    <IsElement>true</IsElement>
    <!-- other info elements -->
  </Info>
  <!-- other content type elements -->
</ContentType>
```

## Dependencies

- **BlockFarmEditor.RCL**: Core services and models
- **uSync.BackOffice**: uSync integration framework
- **Umbraco.Cms.Core**: Umbraco core services

## Usage

### Getting Started

1. **Install the Package**: Follow the installation instructions above
2. **Restart Umbraco**: The handlers will be automatically registered on startup
3. **Use uSync Normally**: Run your regular uSync export/import operations
4. **Block Definitions Sync Automatically**: Element content types will trigger automatic block definition synchronization

### What Happens Automatically

- **During Export**: When uSync exports element content types, corresponding block definitions are exported to JSON files
- **During Import**: When uSync imports element content types, corresponding block definitions are imported/updated from JSON files
- **Cache Management**: Block definition cache is automatically cleared after import completion

### No Configuration Required

The package works out of the box with your existing uSync setup. It uses your configured uSync folders and follows the same export/import patterns as other uSync operations.

## Configuration

The handlers use the configured uSync folders from `ISyncConfigService`. By default, definitions are stored in:
- Export path: `{uSyncFolder}/{uSyncVersion}/BlockFarmEditorDefinitions/`
- File naming: `{contentTypeAlias}.json`

## Error Handling

- Gracefully handles missing definition files
- Skips processing if alias is not found
- Creates new GUIDs for user IDs when system user is unavailable
- Validates JSON deserialization before processing

## Example Definition File

```json
{
  "Id": 1,
  "Key": "12345678-1234-1234-1234-123456789012",
  "ContentTypeAlias": "alert",
  "Type": "partial",
  "ViewPath": "~/Views/Partials/Alert.cshtml",
  "Category": "Content",
  "Enabled": true,
  "CreatedAt": "2025-01-01T00:00:00Z",
  "UpdatedAt": "2025-01-01T00:00:00Z",
  "CreatedBy": "ffffffff-ffff-ffff-ffff-ffffffffffff",
  "UpdatedBy": "ffffffff-ffff-ffff-ffff-ffffffffffff"
}
```

## Integration Notes

- The handlers are automatically registered when the package is installed
- No manual configuration is required for basic operation
- Definitions are only processed for element content types
- Both create and update operations are supported during import

## Troubleshooting

### Common Issues

**Package Not Working After Installation**
- Ensure you've restarted your Umbraco application after installing the package
- Check that both BlockFarmEditor.RCL and uSync are properly installed and functioning
- Verify that your content types are marked as element types (`IsElement = true`)

**Block Definitions Not Exporting**
- Confirm that your content types have the `IsElement` flag set to `true`
- Check that block definitions exist for the content type aliases in your database
- Verify uSync export is working correctly for content types

**Block Definitions Not Importing**
- Ensure the JSON files exist in the `BlockFarmEditorDefinitions` folder
- Check that the JSON structure matches the expected `BlockFarmEditorDefinitionDTO` format
- Verify file permissions allow reading from the uSync folder

**Cache Issues**
- Block definition cache is automatically cleared after imports
- If experiencing cache issues, try manually restarting the application

### Getting Help

If you encounter issues not covered here:
1. Check the Umbraco logs for detailed error messages
2. Ensure all dependencies are up to date
3. Verify your uSync configuration is working for other content types
