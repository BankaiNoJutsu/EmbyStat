﻿using System;
using System.Threading.Tasks;
using EmbyStat.Common;
using EmbyStat.Common.Hubs;
using EmbyStat.Jobs.Jobs.Interfaces;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Interfaces;
using Hangfire;

namespace EmbyStat.Jobs.Jobs.Maintenance
{
    [DisableConcurrentExecution(5 * 60)]
    public class DatabaseCleanupJob : BaseJob, IDatabaseCleanupJob
    {
        private readonly IStatisticsRepository _statisticsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IGenreRepository _genreRepository;

        public DatabaseCleanupJob(IJobHubHelper hubHelper, IJobRepository jobRepository, IConfigurationService configurationService,
            IStatisticsRepository statisticsRepository, IPersonRepository personRepository, IGenreRepository genreRepository) 
            : base(hubHelper, jobRepository, configurationService)
        {
            _statisticsRepository = statisticsRepository;
            _personRepository = personRepository;
            _genreRepository = genreRepository;
            Title = jobRepository.GetById(Id).Title;
        }

        public sealed override Guid Id => Constants.JobIds.DatabaseCleanupId;
        public override string JobPrefix => Constants.LogPrefix.DatabaseCleanupJob;
        public override string Title { get; }

        public override async Task RunJob()
        {
            await _statisticsRepository.CleanupStatistics();
            LogProgress(33);
            LogInformation("Removed old statistic results.");

            await _personRepository.CleanupPersons();
            LogProgress(66);
            LogInformation("Removed unused people.");

            await _genreRepository.CleanupGenres();
            LogInformation("Removed unused genres.");
        }

        public override void Dispose()
        {
        }
    }
}