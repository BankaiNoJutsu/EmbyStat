﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbyStat.Common;
using EmbyStat.Common.Converters;
using EmbyStat.Common.Enums;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Abstract;
using EmbyStat.Services.Converters;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Charts;
using EmbyStat.Services.Models.Movie;
using EmbyStat.Services.Models.Stat;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using Newtonsoft.Json;

namespace EmbyStat.Services
{
    public class MovieService : MediaService, IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly IPersonService _personService;
        private readonly ISettingsService _settingsService;
        private readonly IStatisticsRepository _statisticsRepository;

        public MovieService(IMovieRepository movieRepository, ILibraryRepository libraryRepository,
            IPersonService personService, ISettingsService settingsService,
            IStatisticsRepository statisticsRepository, IJobRepository jobRepository) : base(jobRepository)
        {
            _movieRepository = movieRepository;
            _libraryRepository = libraryRepository;
            _personService = personService;
            _settingsService = settingsService;
            _statisticsRepository = statisticsRepository;
        }

        public IEnumerable<Library> GetMovieLibraries()
        {
            var settings = _settingsService.GetUserSettings();
            return _libraryRepository.GetLibrariesByTypes(settings.MovieLibraryTypes);
        }

        public async Task<MovieStatistics> GetMovieStatisticsAsync(List<string> libraryIds)
        {
            var statistic = _statisticsRepository.GetLastResultByType(StatisticType.Movie, libraryIds);

            MovieStatistics statistics;
            if (StatisticsAreValid(statistic, libraryIds))
            {
                statistics = JsonConvert.DeserializeObject<MovieStatistics>(statistic.JsonResult);

                if (!_settingsService.GetUserSettings().ToShortMovieEnabled && statistics.Suspicious.Shorts.Any())
                {
                    statistics.Suspicious.Shorts = new List<ShortMovie>();
                }
            }
            else
            {
                statistics = await CalculateMovieStatistics(libraryIds);
            }

            return statistics;
        }

        public async Task<MovieStatistics> CalculateMovieStatistics(List<string> libraryIds)
        {
            var movies = _movieRepository.GetAll(libraryIds).ToList();

            var statistics = new MovieStatistics
            {
                General = CalculateGeneralStatistics(movies),
                Charts = CalculateCharts(movies),
                People = await CalculatePeopleStatistics(movies),
                Suspicious = CalculateSuspiciousMovies(movies)
            };

            var json = JsonConvert.SerializeObject(statistics);
            _statisticsRepository.AddStatistic(json, DateTime.UtcNow, StatisticType.Movie, libraryIds);

            return statistics;
        }

        private MovieGeneral CalculateGeneralStatistics(IReadOnlyCollection<Movie> movies)
        {
            return new MovieGeneral
            {
                MovieCount = TotalMovieCount(movies),
                GenreCount = TotalMovieGenres(movies),
                TotalPlayableTime = TotalPlayLength(movies),
                HighestRatedMovie = HighestRatedMovie(movies),
                LowestRatedMovie = LowestRatedMovie(movies),
                OldestPremieredMovie = OldestPremieredMovie(movies),
                NewestPremieredMovie = NewestPremieredMovie(movies),
                ShortestMovie = ShortestMovie(movies),
                LongestMovie = LongestMovie(movies),
                LatestAddedMovie = LatestAddedMovie(movies),
                TotalDiskSize = CalculateTotalDiskSize(movies)
            };
        }

        public async Task<PersonStats> CalculatePeopleStatistics(IReadOnlyCollection<Movie> movies)
        {
            return new PersonStats
            {
                TotalActorCount = TotalTypeCount(movies, PersonType.Actor, Constants.Common.TotalActors),
                TotalDirectorCount = TotalTypeCount(movies, PersonType.Director, Constants.Common.TotalDirectors),
                TotalWriterCount = TotalTypeCount(movies, PersonType.Writer, Constants.Common.TotalWriters),
                MostFeaturedActor = await GetMostFeaturedPersonAsync(movies, PersonType.Actor, Constants.Common.MostFeaturedActor),
                MostFeaturedDirector = await GetMostFeaturedPersonAsync(movies, PersonType.Director, Constants.Common.MostFeaturedDirector),
                MostFeaturedWriter = await GetMostFeaturedPersonAsync(movies, PersonType.Writer, Constants.Common.MostFeaturedWriter),
                MostFeaturedActorsPerGenre = await GetMostFeaturedActorsPerGenreAsync(movies)
            };
        }

        private MovieCharts CalculateCharts(IReadOnlyCollection<Movie> movies)
        {
            var stats = new MovieCharts();
            stats.BarCharts.Add(CalculateGenreChart(movies));
            stats.BarCharts.Add(CalculateRatingChart(movies.Select(x => x.CommunityRating)));
            stats.BarCharts.Add(CalculatePremiereYearChart(movies.Select(x => x.PremiereDate)));
            stats.BarCharts.Add(CalculateOfficialRatingChart(movies));

            return stats;
        }

        public SuspiciousTables CalculateSuspiciousMovies(IReadOnlyCollection<Movie> movies)
        {
            return new SuspiciousTables
            {
                Duplicates = GetDuplicates(movies),
                Shorts = GetShortMovies(movies),
                NoImdb = GetMoviesWithoutImdbLink(movies),
                NoPrimary = GetMoviesWithoutPrimaryImage(movies)
            };
        }

        public bool TypeIsPresent()
        {
            return _movieRepository.Any();
        }

        private IEnumerable<ShortMovie> GetShortMovies(IEnumerable<Movie> movies)
        {
            var settings = _settingsService.GetUserSettings();
            if (!settings.ToShortMovieEnabled)
            {
                return new List<ShortMovie>(0);
            }

            var shortMovies = movies
                .Where(x => x.RunTimeTicks != null)
                .Where(x => new TimeSpan(x.RunTimeTicks ?? 0).TotalMinutes < settings.ToShortMovie)
                .OrderBy(x => x.SortName);
            return shortMovies.Select((t, i) => new ShortMovie
            {
                Number = i++,
                Duration = Math.Floor(new TimeSpan(t.RunTimeTicks ?? 0).TotalMinutes),
                Title = t.Name,
                MediaId = t.Id
            }).ToList();
        }

        private IEnumerable<SuspiciousMovie> GetMoviesWithoutImdbLink(IEnumerable<Movie> movies)
        {
            var noImdbMovies = movies
                .Where(x => string.IsNullOrWhiteSpace(x.IMDB))
                .OrderBy(x => x.SortName)
                .Select((t, i) => new SuspiciousMovie
                {
                    Number = i++,
                    Title = t.Name,
                    MediaId = t.Id
                });

            return noImdbMovies;
        }

        private IEnumerable<SuspiciousMovie> GetMoviesWithoutPrimaryImage(IEnumerable<Movie> movies)
        {
            var noPrimaryImageMovies = movies
                .Where(x => string.IsNullOrWhiteSpace(x.Primary))
                .OrderBy(x => x.SortName)
                .Select((t, i) => new SuspiciousMovie
                {
                    Number = i++,
                    Title = t.Name,
                    MediaId = t.Id
                })
                .ToList();

            return noPrimaryImageMovies;
        }

        private IEnumerable<MovieDuplicate> GetDuplicates(IReadOnlyCollection<Movie> movies)
        {
            var list = new List<MovieDuplicate>();

            var duplicatesByImdb = movies.Where(x => !string.IsNullOrWhiteSpace(x.IMDB)).GroupBy(x => x.IMDB).Select(x => new { x.Key, Count = x.Count() }).Where(x => x.Count > 1).ToList();
            for (var i = 0; i < duplicatesByImdb.Count; i++)
            {
                var duplicateMovies = movies.Where(x => x.IMDB == duplicatesByImdb[i].Key).OrderBy(x => x.Id).ToList();
                var itemOne = duplicateMovies[0];
                var itemTwo = duplicateMovies[1];

                if (itemOne.Video3DFormat != itemTwo.Video3DFormat)
                {
                    continue;
                }

                list.Add(new MovieDuplicate
                {
                    Number = i,
                    Title = itemOne.Name,
                    Reason = Constants.ByImdb,
                    ItemOne = new MovieDuplicateItem { DateCreated = itemOne.DateCreated, Id = itemOne.Id, Quality = string.Join(",", itemOne.VideoStreams.Select(x => QualityConverter.ConvertToQualityString(x.Width))) },
                    ItemTwo = new MovieDuplicateItem { DateCreated = itemTwo.DateCreated, Id = itemTwo.Id, Quality = string.Join(",", itemTwo.VideoStreams.Select(x => QualityConverter.ConvertToQualityString(x.Width))) }
                });
            }

            return list.OrderBy(x => x.Title).ToList();
        }

        #region StatCreators

        private Card<int> TotalMovieCount(IEnumerable<Movie> movies)
        {
            return new Card<int>
            {
                Title = Constants.Movies.TotalMovies,
                Value = movies.Count()
            };
        }

        private Card<int> TotalMovieGenres(IEnumerable<Movie> movies)
        {
            return new Card<int>
            {
                Title = Constants.Movies.TotalGenres,
                Value = movies.SelectMany(x => x.Genres)
                              .Distinct()
                              .Count()
            };
        }

        private MoviePoster HighestRatedMovie(IEnumerable<Movie> movies)
        {
            var movie = movies.Where(x => x.CommunityRating != null)
                              .OrderByDescending(x => x.CommunityRating).ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.HighestRated);
            }

            return new MoviePoster();
        }

        private MoviePoster LowestRatedMovie(IEnumerable<Movie> movies)
        {
            var movie = movies.Where(x => x.CommunityRating != null)
                              .OrderBy(x => x.CommunityRating)
                              .ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.LowestRated);
            }

            return new MoviePoster();
        }

        private MoviePoster OldestPremieredMovie(IEnumerable<Movie> movies)
        {
            var movie = movies.Where(x => x.PremiereDate != null)
                              .OrderBy(x => x.PremiereDate)
                              .ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.OldestPremiered);
            }

            return new MoviePoster();
        }

        private MoviePoster NewestPremieredMovie(IEnumerable<Movie> movies)
        {
            var movie = movies.Where(x => x.PremiereDate != null)
                              .OrderByDescending(x => x.PremiereDate)
                              .ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.NewestPremiered);
            }

            return new MoviePoster();
        }

        private MoviePoster ShortestMovie(IEnumerable<Movie> movies)
        {
            var settings = _settingsService.GetUserSettings();
            var movie = movies.Where(x => x.RunTimeTicks != null && x.RunTimeTicks >= TimeSpan.FromMinutes(settings.ToShortMovie).Ticks)
                              .OrderBy(x => x.RunTimeTicks)
                              .ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.Shortest);
            }

            return new MoviePoster();
        }

        private MoviePoster LongestMovie(IEnumerable<Movie> movies)
        {
            var movie = movies.Where(x => x.RunTimeTicks != null)
                              .OrderByDescending(x => x.RunTimeTicks)
                              .ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.Longest);
            }

            return new MoviePoster();
        }

        private MoviePoster LatestAddedMovie(IEnumerable<Movie> movies)
        {
            var movie = movies.Where(x => x.DateCreated != null)
                              .OrderByDescending(x => x.DateCreated)
                              .ThenBy(x => x.SortName)
                              .FirstOrDefault();

            if (movie != null)
            {
                return PosterHelper.ConvertToMoviePoster(movie, Constants.Movies.LatestAdded);
            }

            return new MoviePoster();
        }

        private TimeSpanCard TotalPlayLength(IEnumerable<Movie> movies)
        {
            var playLength = new TimeSpan(movies.Sum(x => x.RunTimeTicks ?? 0));
            return new TimeSpanCard
            {
                Title = Constants.Movies.TotalPlayLength,
                Days = playLength.Days,
                Hours = playLength.Hours,
                Minutes = playLength.Minutes
            };
        }

        private Card<int> TotalTypeCount(IEnumerable<Movie> movies, PersonType type, string title)
        {
            var value = movies.SelectMany(x => x.People)
                .DistinctBy(x => x.Id)
                .Count(x => x.Type == type);
            return new Card<int>
            {
                Value = value,
                Title = title
            };
        }

        private async Task<PersonPoster> GetMostFeaturedPersonAsync(IEnumerable<Movie> movies, PersonType type, string title)
        {
            var personName = movies.SelectMany(x => x.People)
                .Where(x => x.Type == type)
                .GroupBy(x => x.Name, (Name, people) => new { Name, Count = people.Count() })
                .OrderByDescending(x => x.Count)
                .Select(x => x.Name)
                .FirstOrDefault();

            if (personName != null)
            {
                var person = await _personService.GetPersonByNameAsync(personName);
                if (person != null)
                {
                    return PosterHelper.ConvertToPersonPoster(person, title);
                }
            }

            return new PersonPoster(title);

        }

        private async Task<List<PersonPoster>> GetMostFeaturedActorsPerGenreAsync(IReadOnlyCollection<Movie> movies)
        {
            var list = new List<PersonPoster>();
            foreach (var genre in movies.SelectMany(x => x.Genres).Distinct().OrderBy(x => x))
            {
                var selectedMovies = movies.Where(x => x.Genres.Any(y => y == genre));
                var personName = selectedMovies
                    .SelectMany(x => x.People)
                    .Where(x => x.Type == PersonType.Actor)
                    .GroupBy(x => x.Name, (name, people) => new { Name = name, Count = people.Count()})
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

        private Chart CalculateGenreChart(IEnumerable<Movie> movies)
        {
            var genresData = movies
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

        private Chart CalculateOfficialRatingChart(IEnumerable<Movie> movies)
        {
            var ratingData = movies
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
        #endregion
    }
}
