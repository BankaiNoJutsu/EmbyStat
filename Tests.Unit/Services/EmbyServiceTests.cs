﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EmbyStat.Clients.EmbyClient;
using EmbyStat.Clients.EmbyClient.Model;
using EmbyStat.Common;
using EmbyStat.Common.Exceptions;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Common.Models.Settings;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Emby;
using FluentAssertions;
using FluentAssertions.Common;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.System;
using Moq;
using Xunit;
using PluginInfo = EmbyStat.Common.Models.Entities.PluginInfo;

namespace Tests.Unit.Services
{
	[Collection("Mapper collection")]
	public class EmbyServiceTests
    {
	    private readonly EmbyService _subject;
	    private readonly Mock<IEmbyClient> _embyClientMock;
        private readonly ServerInfo _serverInfo;

		public EmbyServiceTests()
	    {
            var plugins = new List<PluginInfo>
            {
                new PluginInfo { Name = "EmbyStat plugin" },
                new PluginInfo { Name = "Trakt plugin" }
            };

            var embyPlugins = new List<MediaBrowser.Model.Plugins.PluginInfo>
            {
                new MediaBrowser.Model.Plugins.PluginInfo {Name = "EmbyStat plugin"},
                new MediaBrowser.Model.Plugins.PluginInfo {Name = "Trakt plugin"}
            };

			_serverInfo = new ServerInfo
			{
				Id = Guid.NewGuid().ToString(),
				HttpServerPortNumber = 8096,
				HttpsPortNumber = 8097
			};

		    var drives = new List<Drive>
		    {
		        new Drive {Id = Guid.NewGuid().ToString(), Name = "C:\\" },
		        new Drive {Id = Guid.NewGuid().ToString(), Name = "D:\\" }
		    };

            var embyDrives = new List<FileSystemEntryInfo>
		    {
			    new FileSystemEntryInfo()
		    };

			var systemInfo = new ServerInfo();

			_embyClientMock = new Mock<IEmbyClient>();
		    _embyClientMock.Setup(x => x.GetInstalledPluginsAsync()).Returns(Task.FromResult(embyPlugins));
		    _embyClientMock.Setup(x => x.SetAddressAndUrl(It.IsAny<string>(), It.IsAny<string>()));
		    _embyClientMock.Setup(x => x.GetServerInfoAsync()).Returns(Task.FromResult(systemInfo));
		    _embyClientMock.Setup(x => x.GetLocalDrivesAsync()).Returns(Task.FromResult(embyDrives));

            var embyRepository = new Mock<IEmbyRepository>();
            embyRepository.Setup(x => x.GetAllPlugins()).Returns(plugins);
            embyRepository.Setup(x => x.RemoveAllAndInsertPluginRange(It.IsAny<List<PluginInfo>>()));
            embyRepository.Setup(x => x.AddOrUpdateServerInfo(It.IsAny<ServerInfo>()));
            embyRepository.Setup(x => x.GetServerInfo()).Returns(_serverInfo);
            embyRepository.Setup(x => x.RemoveAllAndInsertDriveRange(It.IsAny<List<Drive>>()));
            embyRepository.Setup(x => x.GetAllDrives()).Returns(drives);

            var settingsServiceMock = new Mock<ISettingsService>();
	        settingsServiceMock.Setup(x => x.GetUserSettings()).Returns(new UserSettings());

	        var mapperMock = new Mock<IMapper>();
	        mapperMock.Setup(x => x.Map<ServerInfo>(It.IsAny<SystemInfo>())).Returns(new ServerInfo());
	        mapperMock.Setup(x => x.Map<IList<Drive>>(It.IsAny<List<FileSystemEntryInfo>>())).Returns(new List<Drive> {new Drive()});
	        mapperMock.Setup(x => x.Map<IList<PluginInfo>>(It.IsAny<List<MediaBrowser.Model.Plugins.PluginInfo>>())).Returns(plugins);

            _subject = new EmbyService(_embyClientMock.Object, settingsServiceMock.Object, embyRepository.Object, mapperMock.Object);
	    }


	    [Fact]
		public async void GetEmbyTokenWithNoLoginInfo()
	    {
		    var ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(null));

		    ex.Message.Should().Be("TOKEN_FAILED");
		    ex.StatusCode.Should().Be(500);
	    }

	    [Fact]
	    public async void GetEmbyTokenWithNoPassword()
	    {
			var login = new EmbyLogin
			{
				UserName = "Admin",
				Address = "http://localhost"
			};
		    var ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(login));

		    ex.Message.Should().Be("TOKEN_FAILED");
		    ex.StatusCode.Should().Be(500);
	    }

	    [Fact]
	    public async void GetEmbyTokenWithNoUserName()
	    {
		    var login = new EmbyLogin
		    {
			    Password = "AdminPass",
			    Address = "http://localhost"
		    };
		    var ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(login));

		    ex.Message.Should().Be("TOKEN_FAILED");
		    ex.StatusCode.Should().Be(500);
	    }

	    [Fact]
	    public async void GetEmbyTokenFailedLogin()
	    {
		    _embyClientMock.Setup(x => x.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			    .ThrowsAsync(new Exception());
			var login = new EmbyLogin
		    {
			    Password = "AdminPass",
			    Address = "http://localhost",
				UserName = "Admin"
		    };
		    var ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(login));

		    ex.Message.Should().Be("TOKEN_FAILED");
		    ex.StatusCode.Should().Be(500);
	    }

	    [Fact]
	    public void GetServerInfoFromDatabase()
	    {
		    var serverInfo = _subject.GetServerInfo();

		    serverInfo.Should().NotBeNull();
		    serverInfo.Result.Id.Should().Be(_serverInfo.Id);
		    serverInfo.Result.HttpServerPortNumber.Should().Be(_serverInfo.HttpServerPortNumber);
		    serverInfo.Result.HttpsPortNumber.Should().Be(_serverInfo.HttpsPortNumber);
	    }

	    [Fact]
	    public void GetDrivesFromDatabase()
	    {
		    var drives = _subject.GetLocalDrives();

		    drives.Should().NotBeNull();
		    drives.Count.Should().Be(2);
	    }
	}
}