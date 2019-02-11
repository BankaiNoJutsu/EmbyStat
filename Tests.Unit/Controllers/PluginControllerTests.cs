﻿using System;
using System.Collections.Generic;
using AutoMapper;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Controllers;
using EmbyStat.Controllers.ViewModels.Emby;
using EmbyStat.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.Unit.Controllers
{
	[Collection("Mapper collection")]
	public class PluginControllerTests : IDisposable
	{
		private readonly PluginController _subject;
		private readonly Mock<IEmbyService> _embyServiceMock;
		private readonly List<PluginInfo> _plugins;

		public PluginControllerTests()
		{
			_plugins = new List<PluginInfo>
			{
				new PluginInfo{ Name = "Trakt plugin"},
				new PluginInfo{ Name = "EmbyStat plugin"}
			};

            _embyServiceMock = new Mock<IEmbyService>();
            _embyServiceMock.Setup(x => x.GetAllPlugins()).Returns(_plugins);

		    var _mapperMock = new Mock<IMapper>();
            _mapperMock.Setup(x => x.Map<IList<EmbyPluginViewModel>>(It.IsAny<List<PluginInfo>>())).Returns(new List<EmbyPluginViewModel>{ new EmbyPluginViewModel { Name = "Trakt plugin" }, new EmbyPluginViewModel { Name = "EmbyStat plugin" } });
            _subject = new PluginController(_embyServiceMock.Object, _mapperMock.Object);
		}

		public void Dispose()
		{
			_subject?.Dispose();
		}

		[Fact]
		public void ArePluginsReturned()
		{
			var result = _subject.Get();
			var resultObject = result.Should().BeOfType<OkObjectResult>().Subject.Value;
			var list = resultObject.Should().BeOfType<List<EmbyPluginViewModel>>().Subject;

			list.Count.Should().Be(2);
			list[0].Name.Should().Be(_plugins[0].Name);
			list[1].Name.Should().Be(_plugins[1].Name);
            _embyServiceMock.Verify(x => x.GetAllPlugins(), Times.Once);
		}	
	}
}
