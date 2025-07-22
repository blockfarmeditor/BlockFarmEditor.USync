using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using uSync.BackOffice;

namespace BlockFarmEditor.USync;

public class BFEuSyncComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<uSyncExportedItemNotification, BlockFarmEditorDefinitionExport>();

        builder.AddNotificationAsyncHandler<uSyncImportedItemNotification, BlockFarmEditorDefinitionImport>();
    }
}
