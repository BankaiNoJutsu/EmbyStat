﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmbyStat.Common.Models;
using EmbyStat.Controllers;
using EmbyStat.Controllers.ViewModels.Emby;
using EmbyStat.Controllers.ViewModels.Movie;
using EmbyStat.Controllers.ViewModels.Stat;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Movie;
using EmbyStat.Services.Models.Stat;
using FluentAssertions;
using MediaBrowser.Model.Plugins;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.Unit.Controllers
{
    [Collection("Mapper collection")]
    public class MovieControllerTests
    {
        private readonly MovieController _subject;
        private readonly Mock<IMovieService> _movieServiceMock;
        private readonly List<Collection> _collections;
        private readonly  MovieStats _movieStats;

        public MovieControllerTests()
        {
            _collections = new List<Collection>
            {
                new Collection{ Id = "id1", Name = "collection1", PrimaryImage = "image1", Type = CollectionType.Movies},
                new Collection{ Id = "id2", Name = "collection2", PrimaryImage = "image2", Type = CollectionType.Movies}
            };

            _movieStats = new MovieStats
            {
                LongestMovie = new Poster { Name = "The lord of the rings"}
            };

            _movieServiceMock = new Mock<IMovieService>();
            _movieServiceMock.Setup(x => x.GetMovieCollections()).Returns(_collections);
            _movieServiceMock.Setup(x => x.GetGeneralStatsForCollections(It.IsAny<List<string>>()))
                .Returns(_movieStats);

            _subject = new MovieController(_movieServiceMock.Object);
        }

        public void Dispose()
        {
            _subject?.Dispose();
        }

        [Fact]
        public void AreMovieCollectionsReturned()
        {
            var result = _subject.GetCollections();
            var resultObject = result.Should().BeOfType<OkObjectResult>().Subject.Value;
            var list = resultObject.Should().BeOfType<List<CollectionViewModel>>().Subject;

            list.Count.Should().Be(2);
            list[0].Name.Should().Be(_collections[0].Name);
            list[1].Name.Should().Be(_collections[1].Name);
            _movieServiceMock.Verify(x => x.GetMovieCollections(), Times.Once);
        }

        [Fact]
        public void AreMovieStatsReturned()
        {
            var result = _subject.GetGeneralStats(_collections.Select(x => x.Id).ToList());
            var resultObject = result.Should().BeOfType<OkObjectResult>().Subject.Value;
            var stat = resultObject.Should().BeOfType<MovieStatsViewModel>().Subject;

            stat.Should().NotBeNull();
            stat.LongestMovie.Name.Should().Be(_movieStats.LongestMovie.Name);
            _movieServiceMock.Verify(x => x.GetGeneralStatsForCollections(It.Is<List<string>>(
                y => y[0] == _collections[0].Id &&
                     y[1] == _collections[1].Id)), Times.Once);
        }
    }
}
