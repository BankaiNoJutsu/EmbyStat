﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Common.Models;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Services.Models;
using EmbyStat.Services.Models.Emby;

namespace EmbyStat.Services.Interfaces
{
    public interface IEmbyService
	{
	    EmbyUdpBroadcast SearchEmby();
	    Task<EmbyToken> GetEmbyToken(EmbyLogin login);
		ServerInfo GetServerInfo();
		List<Drives> GetLocalDrives();
		void FireSmallSyncEmbyServerInfo();
	    EmbyStatus GetEmbyStatus();
	    Task<string> PingEmbyAsync(CancellationToken cancellationToken);
	}
}
