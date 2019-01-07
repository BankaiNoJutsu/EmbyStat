﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Services.Models.Movie;
using EmbyStat.Services.Models.Stat;

namespace EmbyStat.Services.Interfaces
{
    public interface IMovieService
    {
        IEnumerable<Collection> GetMovieCollections();
        MovieStats GetGeneralStatsForCollections(List<Guid> collectionIds);
        Task<PersonStats> GetPeopleStatsForCollections(List<Guid> collectionsIds);
        MovieGraphs GetGraphs(List<Guid> collectionIds);
        SuspiciousTables GetSuspiciousMovies(List<Guid> collectionIds);
        bool MovieTypeIsPresent();
    }
}
