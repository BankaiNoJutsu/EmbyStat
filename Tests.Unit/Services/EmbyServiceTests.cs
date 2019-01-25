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
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services;
using EmbyStat.Services.Models.Emby;
using FluentAssertions;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.System;
using Moq;
using Xunit;

namespace Tests.Unit.Services
{
	[Collection("Mapper collection")]
	public class EmbyServiceTests
    {
	    private readonly EmbyService _subject;
	    private readonly Mock<IEmbyClient> _embyClientMock;
        private readonly Mock<IEmbyRepository> _embyRepository;
        private readonly List<PluginInfo> _plugins;
        private readonly ServerInfo _serverInfo;

		public EmbyServiceTests()
	    {
	        _plugins = new List<PluginInfo>
		    {
			    new PluginInfo { Name = "EmbyStat plugin" },
			    new PluginInfo { Name = "Trakt plugin" }
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

	        var configuration = new List<ConfigurationKeyValue>
	        {
	            new ConfigurationKeyValue{ Id = Constants.Configuration.EmbyUserId, Value = "EmbyUserId" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.Language, Value = "en-US" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.UserName, Value = "admin" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.WizardFinished, Value = "true" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.EmbyServerAddress, Value = "localhost" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.AccessToken, Value = "1234567980" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.EmbyUserName, Value = "reggi" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.ToShortMovie, Value = "10" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.ServerName, Value = "ServerName" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.EmbyServerPort, Value = "80" },
	            new ConfigurationKeyValue{ Id = Constants.Configuration.EmbyServerProtocol, Value = "0" }
            };

            var embyDrives = new List<Drive>
		    {
			    new Drive()
		    };

			var systemInfo = new ServerInfo();

			_embyClientMock = new Mock<IEmbyClient>();
		    _embyClientMock.Setup(x => x.GetInstalledPluginsAsync()).Returns(Task.FromResult(_plugins));
		    _embyClientMock.Setup(x => x.SetAddressAndUrl(It.IsAny<string>(), It.IsAny<string>()));
		    _embyClientMock.Setup(x => x.GetServerInfoAsync()).Returns(Task.FromResult(systemInfo));
		    _embyClientMock.Setup(x => x.GetLocalDrivesAsync()).Returns(Task.FromResult(embyDrives));

            _embyRepository = new Mock<IEmbyRepository>();
            _embyRepository.Setup(x => x.GetAllPlugins()).Returns(_plugins);
            _embyRepository.Setup(x => x.RemoveAllAndInsertPluginRange(It.IsAny<List<PluginInfo>>()));
            _embyRepository.Setup(x => x.AddOrUpdateServerInfo(It.IsAny<ServerInfo>()));
            _embyRepository.Setup(x => x.GetServerInfo()).Returns(_serverInfo);
            _embyRepository.Setup(x => x.RemoveAllAndInsertDriveRange(It.IsAny<List<Drive>>()));
            _embyRepository.Setup(x => x.GetAllDrives()).Returns(drives);

            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
	        configurationRepositoryMock.Setup(x => x.GetConfiguration()).Returns(new Configuration(configuration));

	        var _mapperMock = new Mock<IMapper>();
	        _mapperMock.Setup(x => x.Map<ServerInfo>(It.IsAny<SystemInfo>())).Returns(new ServerInfo());
	        _mapperMock.Setup(x => x.Map<IList<Drive>>(It.IsAny<List<Drive>>())).Returns(new List<Drive> {new Drive()});

			_subject = new EmbyService(_embyClientMock.Object, configurationRepositoryMock.Object, _embyRepository.Object);
	    }

	    //[Fact]
	    //public async void GetEmbyToken()
	    //{
		   // _embyClientMock.Setup(x => x.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			  //  .Returns(Task.FromResult(_authResult));

		   // var login = new EmbyLogin
		   // {
			  //  Address = "http://localhost",
			  //  UserName = "admin",
			  //  Password = "adminpass"
		   // };

		   // var token = await _subject.GetEmbyToken(login);

		   // token.Username.Should().Be(_authResult.User.ConnectUserName);
		   // token.Token.Should().Be(_authResult.AccessToken);
		   // token.IsAdmin.Should().Be(_authResult.User.Policy.IsAdministrator);

		   // _embyClientMock.Verify(x => x.AuthenticateUserAsync(
			  //  It.Is<string>(y => y == login.UserName ),
			  //  It.Is<string>(y => y == login.Password),
			  //  It.Is<string>(y => y == login.Address)));
	    //}

	    [Fact]
		public async void GetEmbyTokenWithNoLoginInfo()
	    {
		    BusinessException ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(null));

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
		    BusinessException ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(login));

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
		    BusinessException ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(login));

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
		    BusinessException ex = await Assert.ThrowsAsync<BusinessException>(() => _subject.GetEmbyToken(login));

		    ex.Message.Should().Be("TOKEN_FAILED");
		    ex.StatusCode.Should().Be(500);
	    }

	    [Fact]
	    public void UpdateServerInfo()
	    {
		    _subject.FireSmallSyncEmbyServerInfo();

		    _embyClientMock.Verify(x => x.GetInstalledPluginsAsync(), Times.Once);
			_embyClientMock.Verify(x => x.GetServerInfoAsync(), Times.Once);
			_embyClientMock.Verify(x => x.GetLocalDrivesAsync(), Times.Once);

            _embyRepository.Verify(x => x.RemoveAllAndInsertPluginRange(It.Is<List<PluginInfo>>(
			    y => y.Count == 2 &&
			         y.First().Name == _plugins.First().Name)));
            _embyRepository.Verify(x => x.AddOrUpdateServerInfo(It.IsAny<ServerInfo>()));
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