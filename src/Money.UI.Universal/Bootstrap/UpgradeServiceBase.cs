﻿using Neptuo;
using Neptuo.Activators;
using Neptuo.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Money.Bootstrap
{
    internal abstract class UpgradeServiceBase : IUpgradeService
    {
        private readonly IFactory<ApplicationDataContainer> storageContainerFactory;
        private readonly int currentVersion;

        public UpgradeServiceBase(IFactory<ApplicationDataContainer> storageContainerFactory, int currentVersion)
        {
            Ensure.NotNull(storageContainerFactory, "storageContainerFactory");
            Ensure.Positive(currentVersion, "currentVersion");
            this.storageContainerFactory = storageContainerFactory;
            this.currentVersion = currentVersion;
        }

        public bool IsRequired()
        {
            return this.currentVersion > GetCurrentVersion();
        }

        public async Task UpgradeAsync(IUpgradeContext context)
        {
            int currentVersion = GetCurrentVersion();
            if (this.currentVersion <= currentVersion)
                return;

            context.TotalSteps(this.currentVersion - currentVersion);
            await Task.Delay(500);

            await UpgradeOverrideAsync(context, currentVersion);

            ApplicationDataContainer migrationContainer = GetMigrationContainer();
            migrationContainer.Values["Version"] = this.currentVersion;
        }

        protected abstract Task UpgradeOverrideAsync(IUpgradeContext context, int currentVersion);

        private ApplicationDataContainer GetMigrationContainer()
        {
            ApplicationDataContainer root = storageContainerFactory.Create();

            ApplicationDataContainer migrationContainer;
            if (!root.Containers.TryGetValue("Migration", out migrationContainer))
                migrationContainer = root.CreateContainer("Migration", ApplicationDataCreateDisposition.Always);

            return migrationContainer;
        }

        private int GetCurrentVersion()
        {
            ApplicationDataContainer migrationContainer = GetMigrationContainer();
            int currentVersion = (int?)migrationContainer.Values["Version"] ?? 0;
            return currentVersion;
        }

    }
}
