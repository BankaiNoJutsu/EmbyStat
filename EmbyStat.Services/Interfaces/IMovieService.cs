﻿using System.Collections.Generic;
using System.Threading.Tasks;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Services.Models.Movie;

namespace EmbyStat.Services.Interfaces
{
    public interface IMovieService
    {
        IEnumerable<Library> GetMovieLibraries();
        Task<MovieStatistics> GetStatisticsAsync(List<string> libraryIds);
        Task<MovieStatistics> CalculateMovieStatistics(List<string> libraryIds);
        bool TypeIsPresent();
    }
}
