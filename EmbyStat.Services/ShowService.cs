﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbyStat.Common;
using EmbyStat.Common.Extentions;
using EmbyStat.Common.Models;
using EmbyStat.Common.Tasks.Enum;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Abstract;
using EmbyStat.Services.Converters;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Graph;
using EmbyStat.Services.Models.Show;
using EmbyStat.Services.Models.Stat;
using Newtonsoft.Json;

namespace EmbyStat.Services
{
    public class ShowService : MediaService, IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IPersonService _personService;
        private readonly IStatisticsRepository _statisticsRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public ShowService(IShowRepository showRepository, 
            ICollectionRepository collectionRepository, 
            IGenreRepository genreRepository, 
            IPersonService personService, 
            ITaskRepository taskRepository, 
            IStatisticsRepository statisticsRepository,
            IConfigurationRepository configurationRepository)
        : base(taskRepository){
            _showRepository = showRepository;
            _collectionRepository = collectionRepository;
            _genreRepository = genreRepository;
            _personService = personService;
            _statisticsRepository = statisticsRepository;
            _configurationRepository = configurationRepository;
        }

        public IEnumerable<Collection> GetShowCollections()
        {
            var config = _configurationRepository.GetConfiguration();
            return _collectionRepository.GetCollectionByTypes(config.ShowCollectionTypes);
        }

        public ShowStat GetGeneralStats(IEnumerable<string> collectionIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.ShowGeneral);

            ShowStat stats;
            if (NewStatisticsNeeded(statistic, collectionIds))
            {
                stats = JsonConvert.DeserializeObject<ShowStat>(statistic.JsonResult);
            }
            else
            {
                var shows = _showRepository.GetAllShows(collectionIds).ToList();
                stats = new ShowStat
                {
                    ShowCount = TotalShowCount(collectionIds),
                    EpisodeCount = TotalEpisodeCount(collectionIds),
                    MissingEpisodeCount = TotalMissingEpisodeCount(shows),
                    TotalPlayableTime = CalculatePlayableTime(collectionIds),
                    HighestRatedShow = CalculateHighestRatedShow(shows),
                    LowestRatedShow = CalculateLowestRatedShow(shows),
                    OldestPremieredShow = CalculateOldestPremieredShow(shows),
                    ShowWithMostEpisodes = CalculateShowWithMostEpisodes(shows),
                    YoungestAddedShow = CalculateYoungestAddedShow(shows),
                    YoungestPremieredShow = CalculateYoungestPremieredShow(shows)
                };

                var json = JsonConvert.SerializeObject(stats);
                _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.ShowGeneral, collectionIds);
            }

            return stats;
        }

        public ShowGraphs GetGraphs(IEnumerable<string> collectionIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.ShowGraphs);

            ShowGraphs stats;
            if (NewStatisticsNeeded(statistic, collectionIds))
            {
                stats = JsonConvert.DeserializeObject<ShowGraphs>(statistic.JsonResult);
            }
            else
            {
                var shows = _showRepository.GetAllShows(collectionIds, true).ToList();

                stats = new ShowGraphs();
                stats.BarGraphs.Add(CalculateGenreGraph(shows));
                stats.BarGraphs.Add(CalculateRatingGraph(shows.Select(x => x.CommunityRating)));
                stats.BarGraphs.Add(CalculatePremiereYearGraph(shows.Select(x => x.PremiereDate)));
                stats.BarGraphs.Add(CalculateCollectedRateGraph(shows));
                stats.BarGraphs.Add(CalculateOfficialRatingGraph(shows));
                stats.PieGraphs.Add(CalculateShowStateGraph(shows));
                
                var json = JsonConvert.SerializeObject(stats);
                _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.ShowGraphs, collectionIds);
            }

            return stats;
        }

        public PersonStats GetPeopleStats(IEnumerable<string> collectionIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.ShowPeople);

            PersonStats stats;
            if (NewStatisticsNeeded(statistic, collectionIds))
            {
                stats = JsonConvert.DeserializeObject<PersonStats>(statistic.JsonResult);
            }
            else
            {
                stats = new PersonStats
                {
                    TotalActorCount = TotalTypeCount(collectionIds, Constants.Actor, Constants.Common.TotalActors),
                    TotalDirectorCount = TotalTypeCount(collectionIds, Constants.Director, Constants.Common.TotalDirectors),
                    TotalWriterCount = TotalTypeCount(collectionIds, Constants.Writer, Constants.Common.TotalWriters)
                };


                var json = JsonConvert.SerializeObject(stats);
                _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.ShowPeople, collectionIds);
            }

            return stats;
        }

        public List<ShowCollectionRow> GetCollectionRows(IEnumerable<string> collectionIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.ShowCollected);

            List<ShowCollectionRow> stats;
            if (NewStatisticsNeeded(statistic, collectionIds))
            {
                stats = JsonConvert.DeserializeObject<List<ShowCollectionRow>>(statistic.JsonResult);
            }
            else
            {
                stats = new List<ShowCollectionRow>();
                var shows = _showRepository.GetAllShows(collectionIds);

                foreach (var show in shows)
                {
                    var episodeCount = _showRepository.GetEpisodeCountForShow(show.Id);
                    var totalEpisodeCount = _showRepository.GetEpisodeCountForShow(show.Id, true);
                    var specialCount = totalEpisodeCount - episodeCount;
                    var seasonCount = _showRepository.GetSeasonCountForShow(show.Id);

                    stats.Add(new ShowCollectionRow
                    {
                        Title = show.Name,
                        SortName = show.SortName,
                        Episodes = episodeCount,
                        Seasons = seasonCount,
                        Specials = specialCount,
                        MissingEpisodes = show.MissingEpisodesCount,
                        PremiereDate = show.PremiereDate,
                        Status = show.Status == "Continuing"
                    });
                }

                var json = JsonConvert.SerializeObject(stats);
                _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.ShowCollected, collectionIds);
            }

            return stats;
        }

        public bool ShowTypeIsPresent()
        {
            return _showRepository.Any();
        }

        private async Task<List<PersonPoster>> GetMostFeaturedActorsPerGenre(List<string> collectionIds)
        {
            var shows = _showRepository.GetAllShows(collectionIds);
            var genreIds = _showRepository.GetGenres(collectionIds);
            var genres = _genreRepository.GetListByIds(genreIds);

            var list = new List<PersonPoster>();
            foreach (var genre in genres.OrderBy(x => x.Name))
            {
                var selectedShows = shows.Where(x => x.MediaGenres.Any(y => y.GenreId == genre.Id));
                var episodes = _showRepository.GetAllEpisodesForShows(selectedShows.Select(x => x.Id), true);

                var grouping = episodes
                    .SelectMany(x => x.ExtraPersons)
                    .Where(x => x.Type == Constants.Actor)
                    .GroupBy(x => x.PersonId)
                    .Select(group => new { Id = group.Key, Count = group.Count() })
                    .OrderByDescending(x => x.Count);

                var personId = grouping
                    .Select(x => x.Id)
                    .FirstOrDefault();
                //Compleet buggy dit! Er moet gekeken worden naar het aantal episodes ipv shows
                //Misschien ExtraPerson weer toevoegen aan Episode type in sync!
                var person = await _personService.GetPersonById(personId);
                list.Add(PosterHelper.ConvertToPersonPoster(person, genre.Name));
            }

            return list;
        }

        private async Task<PersonPoster> GetMostFeaturedPerson(IEnumerable<string> collectionIds, string type, string title)
        {
            var personId = _showRepository.GetMostFeaturedPerson(collectionIds, type);

            var person = await _personService.GetPersonById(personId);
            return PosterHelper.ConvertToPersonPoster(person, title);
        }

        private Card TotalTypeCount(IEnumerable<string> collectionIds, string type, string title)
        {
            return new Card
            {
                Value = _showRepository.GetTotalPersonByType(collectionIds, type).ToString(),
                Title = title
            };
        }

        private Graph<SimpleGraphValue> CalculateShowStateGraph(IEnumerable<Show> shows)
        {
            var list = shows
                .GroupBy(x => x.Status)
                .Select(x => new SimpleGraphValue { Name = x.Key, Value = x.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            return new Graph<SimpleGraphValue>
            {
                Data = list,
                Title = Constants.Shows.ShowStatusGraph
            };
        }

        private Graph<SimpleGraphValue> CalculateOfficialRatingGraph(IEnumerable<Show> shows)
        {
            var ratingData = shows
                .Where(x => !string.IsNullOrWhiteSpace(x.OfficialRating))
                .GroupBy(x => x.OfficialRating.ToUpper())
                .Select(x => new SimpleGraphValue { Name = x.Key, Value = x.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            return new Graph<SimpleGraphValue>
            {
                Title = Constants.CountPerOfficialRating,
                Data = ratingData
            };
        }

        private Graph<SimpleGraphValue> CalculateCollectedRateGraph(IEnumerable<Show> shows)
        {
            var percentageList = new List<double>();
            foreach (var show in shows)
            {
                var episodeCount = _showRepository.CountEpisodes(show.Id);
                if (episodeCount + show.MissingEpisodesCount == 0)
                {
                    percentageList.Add(0);
                }
                else
                {
                    percentageList.Add((double) episodeCount / (episodeCount + show.MissingEpisodesCount));
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
                        groupedList.Add(new GraphGrouping<int?, double> {Key = i * 5, Capacity = 0});
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
                .Select(x => new SimpleGraphValue { Name = x.Name, Value = x.Count })
                .ToList();

            return new Graph<SimpleGraphValue>
            {
                Title = Constants.CountPerCollectedRate,
                Data = rates
            };
        }

        private Graph<SimpleGraphValue> CalculateGenreGraph(IEnumerable<Show> shows)
        {
            var genres = _genreRepository.GetAll();
            var genresData = shows.SelectMany(x => x.MediaGenres).GroupBy(x => x.GenreId)
                .Select(x => new { Name = genres.Single(y => y.Id == x.Key).Name, Count = x.Count() })
                .Select(x => new SimpleGraphValue { Name = x.Name, Value = x.Count })
                .OrderBy(x => x.Name)
                .ToList();

            return new Graph<SimpleGraphValue>
            {
                Title = Constants.CountPerGenre,
                Data = genresData
            };
        }

        private ShowPoster CalculateYoungestPremieredShow(IEnumerable<Show> shows)
        {
            var yougest = shows
                .Where(x => x.PremiereDate.HasValue)
                .OrderByDescending(x => x.PremiereDate)
                .FirstOrDefault();

            if (yougest != null)
            {
                return PosterHelper.ConvertToShowPoster(yougest, Constants.Shows.YoungestPremiered);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateYoungestAddedShow(IEnumerable<Show> shows)
        {
            var yougest = shows
                .Where(x => x.DateCreated.HasValue)
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefault();

            if (yougest != null)
            {
                return PosterHelper.ConvertToShowPoster(yougest, Constants.Shows.YoungestAdded);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateShowWithMostEpisodes(IEnumerable<Show> shows)
        {
            var total = 0;
            Show resultShow = null;
            foreach (var show in shows)
            {
                var episodes = _showRepository.GetAllEpisodesForShow(show.Id).ToArray();
                if (episodes.Length > total)
                {
                    total = episodes.Length;
                    resultShow = show;
                }
            }
          
            if (resultShow != null)
            {
                return PosterHelper.ConvertToShowPoster(resultShow, Constants.Shows.MostEpisodes);
            }

            return new ShowPoster();
        }

        private ShowPoster CalculateOldestPremieredShow(IEnumerable<Show> shows)
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

        private ShowPoster CalculateHighestRatedShow(IEnumerable<Show> shows)
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

        private ShowPoster CalculateLowestRatedShow(IEnumerable<Show> shows)
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

        private Card TotalShowCount(IEnumerable<string> collectionIds)
        {
            var count = _showRepository.CountShows(collectionIds);
            return new Card
            {
                Title = Constants.Shows.TotalShows,
                Value = count.ToString()
            };
        }

        private Card TotalEpisodeCount(IEnumerable<string> collectionIds)
        {
            var count = _showRepository.CountEpisodes(collectionIds);
            return new Card
            {
                Title = Constants.Shows.TotalEpisodes,
                Value = count.ToString()
            };
        }

        private Card TotalMissingEpisodeCount(IEnumerable<Show> shows)
        {
            var count = shows.Sum(x => x.MissingEpisodesCount);
            return new Card
            {
                Title = Constants.Shows.TotalMissingEpisodes,
                Value = count.ToString()
            };
        }

        private TimeSpanCard CalculatePlayableTime(IEnumerable<string> collectionIds)
        {
            var playLength = new TimeSpan(_showRepository.GetPlayLength(collectionIds));
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
