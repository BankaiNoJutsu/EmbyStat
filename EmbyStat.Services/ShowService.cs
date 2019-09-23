using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbyStat.Common;
using EmbyStat.Common.Enums;
using EmbyStat.Common.Extensions;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Abstract;
using EmbyStat.Services.Converters;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Charts;
using EmbyStat.Services.Models.Show;
using EmbyStat.Services.Models.Stat;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using Newtonsoft.Json;
using LocationType = EmbyStat.Common.Enums.LocationType;

namespace EmbyStat.Services
{
    public class ShowService : MediaService, IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly IPersonService _personService;
        private readonly IStatisticsRepository _statisticsRepository;
        private readonly ISettingsService _settingsService;

        public ShowService(IJobRepository jobRepository, IShowRepository showRepository, ILibraryRepository libraryRepository,
            IPersonService personService, IStatisticsRepository statisticsRepository, ISettingsService settingsService) : base(jobRepository)
        {
            _showRepository = showRepository;
            _libraryRepository = libraryRepository;
            _personService = personService;
            _statisticsRepository = statisticsRepository;
            _settingsService = settingsService;
        }

        public IEnumerable<Library> GetShowLibraries()
        {
            var settings = _settingsService.GetUserSettings();
            return _libraryRepository.GetLibrariesByTypes(settings.ShowLibraryTypes);
        }

        public async Task<ShowStatistics> GetStatistics(List<string> libraryIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.Show, libraryIds);

            ShowStatistics statistics;
            if (StatisticsAreValid(statistic, libraryIds))
            {
                statistics = JsonConvert.DeserializeObject<ShowStatistics>(statistic.JsonResult);
            }
            else
            {
                statistics = await CalculateShowStatistics(libraryIds);
            }

            return statistics;
        }

        public async Task<ShowStatistics> CalculateShowStatistics(List<string> libraryIds)
        {
            var shows = _showRepository.GetAllShows(libraryIds).ToList();
            var statistics = new ShowStatistics
            {
                General = CalculateGeneralStatistics(shows),
                Charts = CalculateCharts(shows),
                People = await CalculatePeopleStatistics(shows)
            };

            var json = JsonConvert.SerializeObject(statistics);
            _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.Show, libraryIds);

            return statistics;
        }

        private ShowGeneral CalculateGeneralStatistics(IReadOnlyList<Show> shows)
        {
            return new ShowGeneral
            {
                ShowCount = TotalShowCount(shows),
                EpisodeCount = TotalEpisodeCount(shows),
                MissingEpisodeCount = TotalMissingEpisodeCount(shows),
                TotalPlayableTime = CalculatePlayableTime(shows),
                HighestRatedShow = CalculateHighestRatedShow(shows),
                LowestRatedShow = CalculateLowestRatedShow(shows),
                OldestPremieredShow = CalculateOldestPremieredShow(shows),
                ShowWithMostEpisodes = CalculateShowWithMostEpisodes(shows),
                LatestAddedShow = CalculateLatestAddedShow(shows),
                NewestPremieredShow = CalculateNewestPremieredShow(shows)
            };
        }

        private ShowCharts CalculateCharts(IReadOnlyList<Show> shows)
        {
            var stats = new ShowCharts();
            stats.BarCharts.Add(CalculateGenreChart(shows));
            stats.BarCharts.Add(CalculateRatingChart(shows.Select(x => x.CommunityRating)));
            stats.BarCharts.Add(CalculatePremiereYearChart(shows.Select(x => x.PremiereDate)));
            stats.BarCharts.Add(CalculateCollectedRateChart(shows));
            stats.PieCharts.Add(CalculateOfficialRatingChart(shows));
            stats.PieCharts.Add(CalculateShowStateChart(shows));

            return stats;
        }

        private async Task<PersonStats> CalculatePeopleStatistics(IReadOnlyList<Show> shows)
        {
            return new PersonStats
            {
                MostFeaturedActorsPerGenre = await GetMostFeaturedActorsPerGenreAsync(shows)
            };
        }

        public List<ShowCollectionRow> GetCollectedRows(List<string> libraryIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.ShowCollectedRows, libraryIds);

            List<ShowCollectionRow> stats;
            if (StatisticsAreValid(statistic, libraryIds))
            {
                stats = JsonConvert.DeserializeObject<List<ShowCollectionRow>>(statistic.JsonResult);
            }
            else
            {
                var shows = _showRepository.GetAllShows(libraryIds);

                stats = shows.Select(CreateShowCollectedRow).ToList();

                var json = JsonConvert.SerializeObject(stats);
                _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.ShowCollectedRows, libraryIds);
            }

            return stats;
        }

        private ShowCollectionRow CreateShowCollectedRow(Show show)
        {
            var episodeCount = show.GetNonSpecialEpisodeCount(false);
            var specialCount = show.GetSpecialEpisodeCount(false);
            var seasonCount = show.GetNonSpecialSeasonCount();

            return new ShowCollectionRow
            {
                Title = show.Name,
                SortName = show.SortName,
                Episodes = episodeCount,
                Seasons = seasonCount,
                Specials = specialCount,
                MissingEpisodes = show.GetMissingEpisodes(),
                PremiereDate = show.PremiereDate,
                Status = show.Status == "Continuing"
            };
        }

        public bool TypeIsPresent()
        {
            return _showRepository.AnyShows();
        }

        private async Task<List<PersonPoster>> GetMostFeaturedActorsPerGenreAsync(IReadOnlyList<Show> shows)
        {
            var list = new List<PersonPoster>();
            var genres = shows.SelectMany(x => x.Genres).Distinct();

            foreach (var genre in genres.OrderBy(x => x))
            {
                var selectedShows = shows.Where(x => x.Genres.Any(y => y == genre));

                var personName = selectedShows
                    .SelectMany(x => x.People)
                    .Where(x => x.Type == PersonType.Actor)
                    .GroupBy(x => x.Name, (name, people) => new { Name = name, Count = people.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Name)
                    .FirstOrDefault();

                if (personName != null)
                {
                    var person = await _personService.GetPersonByNameAsync(personName);
                    if (person != null)
                    {
                        list.Add(PosterHelper.ConvertToPersonPoster(person, genre));
                    }
                }
            }

            return list;
        }

        private Chart CalculateShowStateChart(IReadOnlyList<Show> shows)
        {
            var list = shows
                .GroupBy(x => x.Status)
                .Select(x => new { Name = x.Key, Count = x.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            return new Chart
            {
                Title = Constants.Shows.ShowStatusChart,
                Labels = list.Select(x => x.Name),
                DataSets = new List<IEnumerable<int>> { list.Select(x => x.Count) }

            };
        }

        private Chart CalculateOfficialRatingChart(IReadOnlyList<Show> shows)
        {
            var ratingData = shows
                .Where(x => !string.IsNullOrWhiteSpace(x.OfficialRating))
                .GroupBy(x => x.OfficialRating.ToUpper())
                .Select(x => new { Name = x.Key, Count = x.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            return new Chart
            {
                Title = Constants.CountPerOfficialRating,
                Labels = ratingData.Select(x => x.Name),
                DataSets = new List<IEnumerable<int>> { ratingData.Select(x => x.Count) }
            };
        }

        private Chart CalculateCollectedRateChart(IReadOnlyList<Show> shows)
        {
            var percentageList = new List<double>();
            foreach (var show in shows)
            {
                var episodeCount = show.GetNonSpecialEpisodeCount(false);
                if (episodeCount + show.MissingEpisodesCount == 0)
                {
                    percentageList.Add(0);
                }
                else
                {
                    percentageList.Add((double)episodeCount / (episodeCount + show.MissingEpisodesCount));
                }
            }

            var groupedList = percentageList
                .GroupBy(x => x.RoundToFive())
                .OrderBy(x => x.Key)
                .ToList();

            if (percentageList.Any())
            {
                var j = 0;
                for (var i = 0; i < 20; i++)
                {
                    if (groupedList[j].Key != i * 5)
                    {
                        groupedList.Add(new ChartGrouping<int?, double> { Key = i * 5, Capacity = 0 });
                    }
                    else
                    {
                        j++;
                    }
                }
            }

            var rates = groupedList
                .OrderBy(x => x.Key)
                .Select(x => new { Name = x.Key != 100 ? $"{x.Key}% - {x.Key + 4}%" : $"{x.Key}%", Count = x.Count() })
                .Select(x => new { x.Name, x.Count })
                .ToList();

            return new Chart
            {
                Title = Constants.CountPerCollectedRate,
                Labels = rates.Select(x => x.Name),
                DataSets = new List<IEnumerable<int>> { rates.Select(x => x.Count) }
            };
        }

        private Chart CalculateGenreChart(IReadOnlyList<Show> shows)
        {
            var genresData = shows
                .SelectMany(x => x.Genres)
                .GroupBy(x => x)
                .Select(x => new { Name = x.Key, Count = x.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            return new Chart
            {
                Title = Constants.CountPerGenre,
                Labels = genresData.Select(x => x.Name),
                DataSets = new List<IEnumerable<int>> { genresData.Select(x => x.Count) }
            };
        }

        private ShowPoster CalculateNewestPremieredShow(IReadOnlyList<Show> shows)
        {
            var yougest = shows
                .Where(x => x.PremiereDate.HasValue)
                .OrderByDescending(x => x.PremiereDate)
                .FirstOrDefault();

            if (yougest != null)
            {
                return PosterHelper.ConvertToShowPoster(yougest, Constants.Shows.NewestPremiered);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateLatestAddedShow(IReadOnlyList<Show> shows)
        {
            var yougest = shows
                .Where(x => x.DateCreated.HasValue)
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefault();

            if (yougest != null)
            {
                return PosterHelper.ConvertToShowPoster(yougest, Constants.Shows.LatestAdded);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateShowWithMostEpisodes(IReadOnlyList<Show> shows)
        {
            var total = 0;
            Show resultShow = null;
            foreach (var show in shows)
            {
                var episodes = show.GetNonSpecialEpisodeCount(false);
                if (episodes > total)
                {
                    total = episodes;
                    resultShow = show;
                }
            }

            if (resultShow != null)
            {
                return PosterHelper.ConvertToShowPoster(resultShow, Constants.Shows.MostEpisodes);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateOldestPremieredShow(IReadOnlyList<Show> shows)
        {
            var oldest = shows
                .Where(x => x.PremiereDate.HasValue)
                .OrderBy(x => x.PremiereDate)
                .FirstOrDefault();

            if (oldest != null)
            {
                return PosterHelper.ConvertToShowPoster(oldest, Constants.Shows.OldestPremiered);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateHighestRatedShow(IReadOnlyList<Show> shows)
        {
            var highest = shows
                .Where(x => x.CommunityRating.HasValue)
                .OrderByDescending(x => x.CommunityRating)
                .FirstOrDefault();

            if (highest != null)
            {
                return PosterHelper.ConvertToShowPoster(highest, Constants.Shows.HighestRatedShow);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateLowestRatedShow(IReadOnlyList<Show> shows)
        {
            var lowest = shows
                .Where(x => x.CommunityRating.HasValue)
                .OrderBy(x => x.CommunityRating)
                .FirstOrDefault();

            if (lowest != null)
            {
                return PosterHelper.ConvertToShowPoster(lowest, Constants.Shows.LowestRatedShow);
            }

            return new ShowPoster();
        }

        private Card<int> TotalShowCount(IReadOnlyList<Show> shows)
        {
            return new Card<int>
            {
                Title = Constants.Shows.TotalShows,
                Value = shows.Count
            };
        }

        private Card<int> TotalEpisodeCount(IReadOnlyList<Show> shows)
        {
            return new Card<int>
            {
                Title = Constants.Shows.TotalEpisodes,
                Value = shows.Sum(x => x.GetNonSpecialEpisodeCount(false))
            };
        }

        private Card<int> TotalMissingEpisodeCount(IReadOnlyList<Show> shows)
        {
            var count = shows.Sum(x => x.MissingEpisodesCount);
            return new Card<int>
            {
                Title = Constants.Shows.TotalMissingEpisodes,
                Value = count
            };
        }

        private TimeSpanCard CalculatePlayableTime(IReadOnlyList<Show> shows)
        {
            var playLength = new TimeSpan(shows.Sum(x => x.CumulativeRunTimeTicks ?? 0));
            return new TimeSpanCard
            {
                Title = Constants.Shows.TotalPlayLength,
                Days = playLength.Days,
                Hours = playLength.Hours,
                Minutes = playLength.Minutes
            };
        }
    }
}
