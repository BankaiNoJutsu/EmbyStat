﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Clients.EmbyClient;
using EmbyStat.Clients.EmbyClient.Model;
using EmbyStat.Common;
using EmbyStat.Common.Exceptions;
using EmbyStat.Common.Models.Entities;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Interfaces;
using EmbyStat.Services.Models.Emby;
using MediaBrowser.Model.Plugins;
using Newtonsoft.Json;
using Serilog;

namespace EmbyStat.Services
{
    public class EmbyService : IEmbyService
    {
	    private readonly IEmbyClient _embyClient;
	    private readonly IPluginRepository _embyPluginRepository;
	    private readonly IServerInfoRepository _embyServerInfoRepository;
		private readonly IConfigurationRepository _configurationRepository;
		private readonly IDriveRepository _embyDriveRepository;
        private readonly IEmbyStatusRepository _embyStatusRepository;

        public EmbyService(IEmbyClient embyClient, IPluginRepository embyPluginRepository, IServerInfoRepository embyServerInfoRepository, IConfigurationRepository configurationRepository, IDriveRepository embyDriveRepository, IEmbyStatusRepository embyStatusRepository)
        {
            _embyClient = embyClient;
            _embyPluginRepository = embyPluginRepository;
            _embyServerInfoRepository = embyServerInfoRepository;
            _configurationRepository = configurationRepository;
            _embyDriveRepository = embyDriveRepository;
            _embyStatusRepository = embyStatusRepository;
        }

	    public EmbyUdpBroadcast SearchEmby()
	    {
		    using (var client = new UdpClient())
		    {
			    var requestData = Encoding.ASCII.GetBytes("who is EmbyServer?");
			    var serverEp = new IPEndPoint(IPAddress.Any, 7359);

			    client.EnableBroadcast = true;
			    client.Send(requestData, requestData.Length, new IPEndPoint(IPAddress.Broadcast, 7359));

			    var timeToWait = TimeSpan.FromSeconds(2);

			    var asyncResult = client.BeginReceive(null, null);
			    asyncResult.AsyncWaitHandle.WaitOne(timeToWait);
			    if (asyncResult.IsCompleted)
			    {
				    try
				    {
					    var receivedData = client.EndReceive(asyncResult, ref serverEp);
					    var serverResponse = Encoding.ASCII.GetString(receivedData);
						var udpBroadcastResult = JsonConvert.DeserializeObject<EmbyUdpBroadcast>(serverResponse);

					    var configuration = _configurationRepository.GetConfiguration();
					    configuration.ServerName = udpBroadcastResult.Name;
						_configurationRepository.Update(configuration);

					    return udpBroadcastResult;

				    }
				    catch (Exception)
				    {
					    // No data recieved, swallow exception and return empty object
				    }
			    }

			    return new EmbyUdpBroadcast();
		    }
	    }

	    public async Task<EmbyToken> GetEmbyToken(EmbyLogin login)
		{
		    if (!string.IsNullOrEmpty(login?.Password) && !string.IsNullOrEmpty(login.UserName))
		    {
				try
				{
                    //TODO, fix this code
					var token = await _embyClient.AuthenticateUserAsync(login.UserName, login.Password, login.Address);
					return new EmbyToken
					{
						Token = token.AccessToken,
						Username = token.User.ConnectUserName,
						IsAdmin = token.User.Policy.IsAdministrator,
                        Id = token.User.Id
					};
				}
				catch (Exception e)
				{
					Log.Warning($"{Constants.LogPrefix.ServerApi}\tUsername or password are wrong, user should try again with other credentials!");
					throw new BusinessException("TOKEN_FAILED");
				}
			}

		    Log.Error("Username or password are empty, no use to try a login!");
			throw new BusinessException("TOKEN_FAILED");
	    }

	    public ServerInfo GetServerInfo()
	    {
		    return _embyServerInfoRepository.GetSingle();
	    }

        public Task<ServerInfo> GetLiveServerInfo()
        {
            return _embyClient.GetServerInfoAsync();
        }

        public Task<List<PluginInfo>> GetLivePluginInfo()
        {
            return _embyClient.GetInstalledPluginsAsync();
        }

        public List<Drive> GetLocalDrives()
	    {
		    return _embyDriveRepository.GetAll();
	    }

        public Task<List<Drive>> GetLiveEmbyDriveInfo()
        {
            return _embyClient.GetLocalDrivesAsync();
        }

        public void UpdateOrAddServerInfo(ServerInfo server)
        {
            _embyServerInfoRepository.UpdateOrAdd(server);
        }

        public void RemoveAllAndInsertPluginRange(List<PluginInfo> plugins)
        {
            _embyPluginRepository.RemoveAllAndInsertPluginRange(plugins);
        }

        public void RemoveAllAndInsertDriveRange(List<Drive> drives)
        {
            _embyDriveRepository.RemoveAllAndInsertDriveRange(drives);
        }

        public async void FireSmallSyncEmbyServerInfo()
	    {
		    var settings = _configurationRepository.GetConfiguration();

			_embyClient.SetAddressAndUrl(settings.GetFullEmbyServerAddress(), settings.AccessToken);
		    var systemInfoReponse = await _embyClient.GetServerInfoAsync();
			var pluginsResponse = await _embyClient.GetInstalledPluginsAsync();
		    var drives = await _embyClient.GetLocalDrivesAsync();

		    _embyServerInfoRepository.UpdateOrAdd(systemInfoReponse);
			_embyPluginRepository.RemoveAllAndInsertPluginRange(pluginsResponse);
			_embyDriveRepository.RemoveAllAndInsertDriveRange(drives.ToList());
		}

        public EmbyStatus GetEmbyStatus()
        {
            return _embyStatusRepository.GetEmbyStatus();
        }

        public async Task<string> PingEmbyAsync(CancellationToken cancellationToken)
        {
		    var settings = _configurationRepository.GetConfiguration();
            _embyClient.SetAddressAndUrl(settings.GetFullEmbyServerAddress(), settings.AccessToken);
            return await _embyClient.PingEmbyAsync(cancellationToken);
        }

        public void SetEmbyClientAddressAndUrl(string url, string token)
        {
            _embyClient.SetAddressAndUrl(url, token);
        }

        public void Dispose()
        {
            _embyClient?.Dispose();
        }
    }
}
