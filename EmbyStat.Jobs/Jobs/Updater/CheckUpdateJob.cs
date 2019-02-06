﻿using System;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Common;
using EmbyStat.Common.Hubs;
using EmbyStat.Common.Hubs.Job;
using EmbyStat.Jobs.Jobs.Interfaces;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Interfaces;
using Hangfire;

namespace EmbyStat.Jobs.Jobs.Updater
{
    [DisableConcurrentExecution(30)]
    public class CheckUpdateJob : BaseJob, ICheckUpdateJob
    {
        private readonly IUpdateService _updateService;
        private readonly IConfigurationService _configurationService;

        public CheckUpdateJob(IJobHubHelper hubHelper, IJobRepository jobRepository, 
            IConfigurationService configurationService, IUpdateService updateService) 
            : base(hubHelper, jobRepository, configurationService)
        {
            _updateService = updateService;
            _configurationService = configurationService;
            Title = jobRepository.GetById(Id).Title;
        }

        public sealed override Guid Id => Constants.JobIds.CheckUpdateId;
        public override string JobPrefix => Constants.LogPrefix.CheckUpdateJob;
        public override string Title { get; }

        public override async Task RunJob()
        {
            LogInformation("Contacting Github now to see if new version is available.");
            var update = await _updateService.CheckForUpdate(Settings, new CancellationToken(false));
            LogProgress(20);
            if (update.IsUpdateAvailable && Settings.AutoUpdate)
            {
                LogInformation($"New version found: v{update.AvailableVersion}");
                LogInformation($"Auto update is enabled so going to update the server now!");
                _configurationService.SetUpdateInProgressSetting(true);
                await HubHelper.BroadcastUpdateState(true);
                Task.WaitAll(_updateService.DownloadZip(update));
                LogProgress(50);
                await _updateService.UpdateServer();
            }
            else if (update.IsUpdateAvailable)
            {
                LogInformation($"New version found: v{update.AvailableVersion}");
                LogInformation("Auto updater is disabled, so going to end task now.");
            }
            else
            {
                LogInformation("No new version available");
            }
        }

        public override async void OnFail()
        {
            _configurationService.SetUpdateInProgressSetting(false);
            await HubHelper.BroadcastUpdateState(false);
        }

        public override void Dispose()
        {

        }
    }
}