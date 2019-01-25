﻿using System;
using System.Threading.Tasks;
using EmbyStat.Common;
using EmbyStat.Common.Hubs;
using EmbyStat.Common.Hubs.Job;
using EmbyStat.Jobs.Jobs.Interfaces;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Interfaces;
using Hangfire;

namespace EmbyStat.Jobs.Jobs.Sync
{
    [DisableConcurrentExecution(60)]
    public class SmallSyncJob : BaseJob, ISmallSyncJob
    {
        private readonly IEmbyService _embyService;

        public SmallSyncJob(IJobHubHelper hubHelper, IJobRepository jobRepository, IConfigurationService configurationService, IEmbyService embyService) : base(hubHelper, jobRepository, configurationService)
        {
            _embyService = embyService;
            Title = jobRepository.GetById(Id).Title;
        }

        public sealed override Guid Id => Constants.JobIds.SmallSyncId;
        public override string JobPrefix => Constants.LogPrefix.SmallEmbySyncJob;
        public override string Title { get; }

        public override async Task RunJob()
        {
            await _embyService.GetAndProcessServerInfo();
            LogInformation("Server info downloaded");
            LogProgress(35);

            await _embyService.GetAndProcessPluginInfo();
            LogInformation("Server plugins downloaded");
            LogProgress(55);

            await _embyService.GetAndProcessEmbyDriveInfo();
            LogInformation("Server drives downloaded");
            LogProgress(65);

            await _embyService.GetAndProcessEmbyUsers();
            LogInformation("Server users downloaded");
        }

        public override void OnFail()
        {
            
        }

        public override void Dispose()
        {
            _embyService.Dispose();
        }
    }
}