﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Common.Enums;
using EmbyStat.Common.Extensions;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Common.Models.Settings;
using EmbyStat.Services;
using EmbyStat.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Unit.Services
{
    public class LogServiceTests
    {
        private readonly Mock<ISettingsService> _settingsServiceMock;

        public LogServiceTests()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
        }

        [Fact]
        public void GetLogList()
        {
            if (Directory.Exists(Path.Combine("config", "Logs-test1").GetLocalPath()))
            {
                Directory.Delete(Path.Combine("config", "Logs-test1").GetLocalPath(), true);
            }

            Directory.CreateDirectory(Path.Combine("config", "Logs-test1").GetLocalPath());

            var settings = new AppSettings { Dirs = new Dirs { Logs = "Logs-test1", Config = "config" } };
            var userSettings = new UserSettings { KeepLogsCount = 10 };

            _settingsServiceMock.Setup(x => x.GetAppSettings()).Returns(settings);
            _settingsServiceMock.Setup(x => x.GetUserSettings()).Returns(userSettings);

            var embyServiceMock = new Mock<IEmbyService>();

            var service = new LogService(_settingsServiceMock.Object, embyServiceMock.Object);

            File.Create(Path.Combine("config", "Logs-test1", "log1.txt").GetLocalPath());
            Thread.Sleep(1001);
            File.Create(Path.Combine("config", "Logs-test1", "log2.txt").GetLocalPath());

            var list = service.GetLogFileList();
            list.Should().NotBeNull();
            list.Count.Should().Be(2);
            list[0].FileName.Should().Be("log2");
            list[1].FileName.Should().Be("log1");
        }

        [Fact]
        public void GetLimitedLogList()
        {
            if (Directory.Exists(Path.Combine("config", "Logs-test2").GetLocalPath()))
            {
                Directory.Delete(Path.Combine("config", "Logs-test2").GetLocalPath(), true);
            }

            Directory.CreateDirectory(Path.Combine("config", "Logs-test2").GetLocalPath());

            var settings = new AppSettings { Dirs = new Dirs { Logs = "Logs-test2", Config = "config" } };
            _settingsServiceMock.Setup(x => x.GetAppSettings()).Returns(settings);

            var userSettings = new UserSettings { KeepLogsCount = 1 };

            _settingsServiceMock.Setup(x => x.GetUserSettings()).Returns(userSettings);
            var embyServiceMock = new Mock<IEmbyService>();

            var service = new LogService(_settingsServiceMock.Object, embyServiceMock.Object);

            File.Create(Path.Combine("config", "Logs-test2", "log1.txt").GetLocalPath());
            Thread.Sleep(1001);
            File.Create(Path.Combine("config", "Logs-test2", "log2.txt").GetLocalPath());

            var list = service.GetLogFileList();
            list.Should().NotBeNull();
            list.Count.Should().Be(1);
            list[0].FileName.Should().Be("log2");
        }

        [Fact]
        public async Task GetLogStream()
        {
            if (Directory.Exists(Path.Combine("config", "Logs-test3").GetLocalPath()))
            {
                Directory.Delete(Path.Combine("config", "Logs-test3").GetLocalPath(), true);
            }

            Directory.CreateDirectory(Path.Combine("config", "Logs-test3").GetLocalPath());

            var settings = new AppSettings { Dirs = new Dirs { Logs = "Logs-test3", Config = "config" } };
            var userSettings = new UserSettings { Emby = new EmbySettings { UserName = "reggi", ServerProtocol = ConnectionProtocol.Http, ServerAddress = "192.168.1.1", ServerPort = 8001 }, Tvdb = new TvdbSettings { ApiKey = "0000" } };
            _settingsServiceMock.Setup(x => x.GetAppSettings()).Returns(settings);
            _settingsServiceMock.Setup(x => x.GetUserSettings()).Returns(userSettings);

            var embyServiceMock = new Mock<IEmbyService>();
            embyServiceMock.Setup(x => x.GetServerInfoAsync()).ReturnsAsync(new ServerInfo { Id = Guid.NewGuid().ToString() });

            var service = new LogService(_settingsServiceMock.Object, embyServiceMock.Object);

            var line = "Log line: http://192.168.1.1:8001; ApiKey:0000; username:reggi";
            File.AppendAllText(Path.Combine("config", "Logs-test3", "log1.txt").GetLocalPath(), line);

            var stream = await service.GetLogStream("log1.txt", false);
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var lines = reader.ReadToEnd();
                lines.Should().Be(line);
            }
        }

        [Fact]
        public async Task GetAnonymousLogStream()
        {
            if (Directory.Exists(Path.Combine("config", "Logs-test4").GetLocalPath()))
            {
                Directory.Delete(Path.Combine("config", "Logs-test4").GetLocalPath(), true);
            }

            Directory.CreateDirectory(Path.Combine("config", "Logs-test4").GetLocalPath());

            var settings = new AppSettings { Dirs = new Dirs { Logs = "Logs-test4", Config = "config" } };
            var userSettings = new UserSettings { Emby = new EmbySettings { UserName = "reggi", ServerProtocol = ConnectionProtocol.Http, ServerAddress = "192.168.1.1", ServerPort = 8001, AccessToken = "123456" }, Tvdb = new TvdbSettings { ApiKey = "0000" } };
            _settingsServiceMock.Setup(x => x.GetAppSettings()).Returns(settings);
            _settingsServiceMock.Setup(x => x.GetUserSettings()).Returns(userSettings);

            var embyServiceMock = new Mock<IEmbyService>();
            embyServiceMock.Setup(x => x.GetServerInfoAsync()).ReturnsAsync(new ServerInfo { Id = "654321" });

            var service = new LogService(_settingsServiceMock.Object, embyServiceMock.Object);

            const string line = "Log line: http://192.168.1.1:8001; ApiKey:0000; username:reggi; ws://192.168.1.1:8001; accessToken:123456; serverId:654321\r\n";
            const string anonymousLine = "Log line: http://xxx.xxx.xxx.xxx:xxxx; ApiKey:xxxxxxxxxxxxxx; username:{EMBY ADMIN USER}; wss://xxx.xxx.xxx.xxx:xxxx; accessToken:xxxxxxxxxxxxxx; serverId:xxxxxxxxxxxxxx\r\n";
            File.AppendAllText(Path.Combine("config", "Logs-test4", "log1.txt").GetLocalPath(), line);


            var stream = await service.GetLogStream("log1.txt", true);
            using (var reader = new StreamReader(stream))
            {
                var lines = reader.ReadToEnd();
                lines.Should().Be(anonymousLine);
            }
        }
    }
}
