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

namespace EmbyStat.Jobs.Jobs.Maintenance
{
    [DisableConcurrentExecution(30)]
    public class PingEmbyJob : BaseJob, IPingEmbyJob
    {
        private readonly IEmbyService _embyService;

        public PingEmbyJob(IJobHubHelper hubHelper, IJobRepository jobRepository, ISettingsService settingsService, 
            IEmbyService embyService) : base(hubHelper, jobRepository, settingsService)
        {
            _embyService = embyService;
            Title = jobRepository.GetById(Id).Title;
        }

        public sealed override Guid Id => Constants.JobIds.PingEmbyId;
        public override string JobPrefix => Constants.LogPrefix.PingEmbyJob;
        public override string Title { get; }

        public override async Task RunJob()
        {
            var result = await _embyService.PingEmbyAsync(Settings.FullEmbyServerAddress, Settings.Emby.AccessToken, new CancellationToken(false));
            LogProgress(50);
            if (result == "Emby Server")
            {
                LogInformation("We found your Emby server");
                _embyService.ResetMissedPings();
            }
            else
            {
                LogInformation("We could not ping your Emby server. Might be because it's turned off or dns is wrong");
                _embyService.IncreaseMissedPings();
            }

            var status = _embyService.GetEmbyStatus();
            await HubHelper.BroadcastEmbyConnectionStatus(status.MissedPings);

        }

        public override void OnFail()
        {
            
        }

        public override void Dispose()
        {
        }
    }
}