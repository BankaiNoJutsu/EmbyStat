﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Common.Models.Show;

namespace EmbyStat.Clients.Tvdb
{
    public interface ITvdbClient
    {
        Task Login(string apiKey, CancellationToken cancellationToken);
        Task<IEnumerable<VirtualEpisode>> GetEpisodes(string seriesId, CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetShowsToUpdate(IEnumerable<string> showIds, DateTime lastUpdateTime, CancellationToken cancellationToken);
    }
}
