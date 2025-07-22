using BlockFarmEditor.RCL.Library.Services;
using BlockFarmEditor.RCL.Models;
using System.Text.Json;
using System.Xml.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using uSync.BackOffice;
using uSync.BackOffice.Configuration;

namespace BlockFarmEditor.USync
{
    public class BlockFarmEditorDefinitionExport(IBlockFarmEditorDefinitionService blockFarmEditorDefinitionService, ISyncConfigService syncConfigService, IUmbracoDatabaseFactory umbracoDatabaseFactory) : INotificationAsyncHandler<uSyncExportedItemNotification>
    {
        public async Task HandleAsync(uSyncExportedItemNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Item is XElement xElement && xElement.Name.LocalName.Equals("ContentType", StringComparison.InvariantCultureIgnoreCase))
            {
                var isElement = xElement
                    .Element("Info")?
                    .Element("IsElement")?
                    .Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                if (isElement)
                {
                    var alias = xElement.Attribute("Alias")?.Value;
                    if (alias == null) return;

                    using var umbracoDatabase = umbracoDatabaseFactory.CreateDatabase();

                    var definition = await blockFarmEditorDefinitionService.GetByAliasAsync(umbracoDatabase, alias);
                    if (definition != null)
                    {
                        var folder = syncConfigService.GetFolders().LastOrDefault("uSync/Root");

                        var path = Path.Combine(folder, "BlockFarmEditorDefinitions");
                        var filePath = Path.Combine(path, $"{alias}.json");
                        var json = JsonSerializer.Serialize(definition);
                        Directory.CreateDirectory(path);
                        await System.IO.File.WriteAllTextAsync(filePath, json, cancellationToken);
                    }
                }
            }
        }
    }
    public class BlockFarmEditorDefinitionImport(IBlockFarmEditorDefinitionService blockFarmEditorDefinitionService, ISyncConfigService syncConfigService, IUserService userService, IUmbracoDatabaseFactory umbracoDatabaseFactory) : INotificationAsyncHandler<uSyncImportedItemNotification>
    {
        public async Task HandleAsync(uSyncImportedItemNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Item is XElement xElement && xElement.Name.LocalName.Equals("ContentType", StringComparison.InvariantCultureIgnoreCase))
            {
                var isElement = xElement
                    .Element("Info")?
                    .Element("IsElement")?
                    .Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

                if (isElement)
                {
                    var alias = xElement.Attribute("Alias")?.Value;
                    if (alias == null) return;
                    var folder = syncConfigService.GetFolders().LastOrDefault("uSync/Root");
                    var path = Path.Combine(folder, "BlockFarmEditorDefinitions", $"{alias}.json");
                    if (Path.Exists(path))
                    {
                        using var umbracoDatabase = umbracoDatabaseFactory.CreateDatabase();

                        var contents = await System.IO.File.ReadAllTextAsync(path, cancellationToken);
                        var dto = JsonSerializer.Deserialize<BlockFarmEditorDefinitionDTO>(contents);
                        if (dto == null) return;
                        var definition = await blockFarmEditorDefinitionService.GetByAliasAsync(umbracoDatabase, alias);
                        if (definition != null)
                        {
                            await blockFarmEditorDefinitionService.UpdateAsync(umbracoDatabase, definition.Id, dto.Type, dto.ViewPath, dto.Category, dto.Enabled, dto.UpdatedBy);
                        }
                        else
                        {
                            var user = userService.GetUserById(-1);
                            await blockFarmEditorDefinitionService.CreateAsync(umbracoDatabase, dto, user?.Key ?? Guid.NewGuid());
                        }
                    }
                }
            }
        }
    }

    public class BlockFarmEditorDefinitionImported(IBlockDefinitionService blockDefinitionService) : INotificationAsyncHandler<uSyncImportCompletedNotification>
    {
        public Task HandleAsync(uSyncImportCompletedNotification notification, CancellationToken cancellationToken)
        {
            blockDefinitionService.ClearCache();
            return Task.CompletedTask;
        }
    }
}
