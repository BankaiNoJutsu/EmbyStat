﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmbyStat.Repositories.EmbyPlugin;
using EmbyStat.Services.Plugin;
using FluentAssertions;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Unit.Services
{
	[Collection("Services collection")]
	public class PluginServiceTests
    {
	    private readonly PluginService _subject;
	    private readonly List<PluginInfo> _plugins;
	    private readonly Mock<IEmbyPluginRepository> _embyPluginRepositoryMock;
		public PluginServiceTests()
	    {
			_plugins = new List<PluginInfo>
			{
				new PluginInfo { Name = "EmbyStat plugin" },
				new PluginInfo { Name = "Trakt plugin" }
			};

		    _embyPluginRepositoryMock = new Mock<IEmbyPluginRepository>();
		    _embyPluginRepositoryMock.Setup(x => x.GetPlugins()).Returns(_plugins);

		    var loggerMock = new Mock<ILogger<PluginService>>();

			_subject = new PluginService(loggerMock.Object, _embyPluginRepositoryMock.Object);
		}

	    [Fact]
	    public void GetPluginsFromDatabase()
	    {
		    var plugins = _subject.GetInstalledPlugins();

		    plugins.Should().NotBeNull();
		    plugins.Count.Should().Be(2);
		    plugins.First().Name.Should().Be(_plugins.First().Name);
		    plugins.Skip(1).First().Name.Should().Be(_plugins.Skip(1).First().Name);

		    _embyPluginRepositoryMock.Verify(x => x.GetPlugins(), Times.Once);
	    }
	}
}
