﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Api.EmbyClient.Model;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.System;
using MediaBrowser.Controller.Authentication;

namespace EmbyStat.Api.EmbyClient
{
    public interface IEmbyClient : IDisposable
    {
	    void SetAddressAndUrl(string url, string token);
		Task<AuthenticationResult> AuthenticateUserAsync(string username, string password, string address);
		Task<List<PluginInfo>> GetInstalledPluginsAsync();
		Task<SystemInfo> GetServerInfoAsync();
	    Task<List<Drive>> GetLocalDrivesAsync();
        Task<string> PingEmbyAsync(CancellationToken cancellationToken);
        Task<QueryResult<BaseItemDto>> GetItemsAsync(ItemQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task<BaseItemDto> GetItemAsync(ItemQuery personQuery, Guid personId, CancellationToken cancellationToken);
        Task<Folder> GetRootFolderAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));
        Task<QueryResult<BaseItemDto>> GetPeopleAsync(PersonsQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task<QueryResult<BaseItemDto>> GetGenresAsync(ItemsByNameQuery query, CancellationToken cancellationToken = default(CancellationToken));
        Task<QueryResult<BaseItemDto>> GetMediaFolders(CancellationToken cancellationToken = default(CancellationToken));
    }
}
