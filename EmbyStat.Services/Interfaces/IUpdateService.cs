﻿using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Clients.Github.Models;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Common.Models.Settings;

namespace EmbyStat.Services.Interfaces
{
    public interface IUpdateService
    {
        Task<UpdateResult> CheckForUpdate(CancellationToken cancellationToken);
        Task<UpdateResult> CheckForUpdate(UserSettings settings, CancellationToken cancellationToken);
        Task DownloadZip(UpdateResult result);
        Task UpdateServer();
    }
}
