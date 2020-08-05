﻿using System;
using System.Collections.Generic;
using System.Linq;
using EmbyStat.Common;
using EmbyStat.Common.Enums;
using EmbyStat.Common.Extensions;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Common.Models.Entities.Helpers;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Converters;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Cards;
using EmbyStat.Services.Models.Charts;
using EmbyStat.Services.Models.Stat;
using Newtonsoft.Json;

namespace EmbyStat.Services.Abstract
{
    public abstract class MediaService
    {
        private readonly IJobRepository _jobRepository;
        internal readonly IPersonService PersonService;

        protected MediaService(IJobRepository jobRepository, IPersonService personService)
        {
            _jobRepository = jobRepository;
            PersonService = personService;
        }

        internal bool StatisticsAreValid(Statistic statistic, IEnumerable<string> collectionIds)
        {
            var lastMediaSync = _jobRepository.GetById(Constants.JobIds.MediaSyncId);

            //We need to add 5 minutes here because the CalculationDateTime is ALWAYS just in front of the EndTimeUtc :( Ugly fix
            return statistic != null
                   && lastMediaSync != null
                   && statistic.CalculationDateTime.AddMinutes(5) > lastMediaSync.EndTimeUtc
                   && collectionIds.AreListEqual(statistic.CollectionIds);
        }

        #region Chart

        internal Chart CalculateGenreChart(IEnumerable<Extra> media)
        {
            var genresData = media
                .SelectMany(x => x.Genres)
                .GroupBy(x => x)
                .Select(x => new { Label = x.Key, Val0 = x.Count() })
                .OrderBy(x => x.Label)
                .ToList();

            return new Chart
            {
                Title = Constants.CountPerGenre,
                DataSets = JsonConvert.SerializeObject(genresData),
                SeriesCount = 1
            };
        }

        internal Chart CalculateRatingChart(IEnumerable<float?> list)
        {
            var ratingDataList = list
                .GroupBy(x => x.RoundToHalf())
                .OrderBy(x => x.Key)
                .ToList();

            for (double i = 0; i < 10; i += 0.5)
            {
                if (!ratingDataList.Any(x => x.Key == i))
                {
                    ratingDataList.Add(new ChartGrouping<double?, float?> { Key = i, Capacity = 0 });
                }
            }

            var ratingData = ratingDataList
                .Select(x => new { Label = x.Key?.ToString() ?? Constants.Unknown, Val0 = x.Count() })
                .OrderBy(x => x.Label)
                .ToList();

            return new Chart
            {
                Title = Constants.CountPerCommunityRating,
                DataSets = JsonConvert.SerializeObject(ratingData),
                SeriesCount = 1
            };
        }

        internal Chart CalculatePremiereYearChart(IEnumerable<DateTimeOffset?> list)
        {
            var yearDataList = list
                .GroupBy(x => x.RoundToFiveYear())
                .Where(x => x.Key != null)
                .OrderBy(x => x.Key)
                .ToList();

            if (yearDataList.Any())
            {
                var lowestYear = yearDataList.Where(x => x.Key.HasValue).Min(x => x.Key);
                var highestYear = yearDataList.Where(x => x.Key.HasValue).Max(x => x.Key);

                for (var i = lowestYear; i < highestYear; i += 5)
                {
                    if (yearDataList.All(x => x.Key != i))
                    {
                        yearDataList.Add(new ChartGrouping<int?, DateTimeOffset?> { Key = i, Capacity = 0 });
                    }
                }
            }

            var yearData = yearDataList
                .Select(x => new { Label = x.Key != null ? $"{x.Key} - {x.Key + 4}" : Constants.Unknown, Val0 = x.Count() })
                .OrderBy(x => x.Label)
                .ToList();

            return new Chart
            {
                Title = Constants.CountPerPremiereYear,
                DataSets = JsonConvert.SerializeObject(yearData),
                SeriesCount = 1
            };
        }

        #endregion

        internal Card<double> CalculateTotalDiskSize(IEnumerable<Video> videos)
        {
            var sum = videos.Sum(x => x.MediaSources.FirstOrDefault()?.SizeInMb ?? 0);
            return new Card<double>
            {
                Value = sum,
                Title = Constants.Common.TotalDiskSize
            };
        }

        #region People

        internal List<TopCard> GetMostFeaturedActorsPerGenre(IReadOnlyList<Extra> media, int count)
        {
            var list = new List<TopCard>();
            foreach (var genre in media.SelectMany(x => x.Genres).Distinct().OrderBy(x => x))
            {
                var selectedMovies = media.Where(x => x.Genres.Any(y => y == genre));
                var people = selectedMovies
                    .SelectMany(x => x.People)
                    .Where(x => x.Type == PersonType.Actor)
                    .GroupBy(x => x.Name, (name, p) => new { Name = name, Count = p.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Name);

                var boe = people
                    .Take(count);
                    var boeTwo = boe.Select(name => PersonService.GetPersonByNameForMovies(name, genre))
                    .ToArray();

                list.Add(boeTwo.ConvertToTopCard(genre, string.Empty, "MovieCount"));
            }

            return list;
        }

        #endregion
    }
}
