﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Api.EmbyClient.Net;
using EmbyStat.Api.Tvdb.Converter;
using EmbyStat.Api.Tvdb.Models;
using EmbyStat.Common;
using EmbyStat.Common.Helpers;
using EmbyStat.Common.Models.Entities;
using Serilog;

namespace EmbyStat.Api.Tvdb
{
    public class TvdbClient : ITvdbClient
    {
        private TvdbToken JWtoken;
        private readonly IAsyncHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;

        public TvdbClient(IAsyncHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }

        public async Task Login(string apiKey, CancellationToken cancellationToken)
        {
            Log.Information($"{Constants.LogPrefix.TheTVDBCLient}\tLogging in on theTVDB API with key: {apiKey}");
            var httpRequest = new HttpRequest
            {
                CancellationToken = cancellationToken,
                Method = "POST",
                Url = $"{Constants.Tvdb.BaseUrl}{Constants.Tvdb.LoginUrl}",
                RequestContent = "{ \"apikey\": \""+ apiKey + "\"}",
                RequestContentType = "application/json"
            };

            using (var stream = await _httpClient.SendAsync(httpRequest))
            {
                JWtoken = _jsonSerializer.DeserializeFromStream<TvdbToken>(stream);
            }
        }

        public async Task<IEnumerable<VirtualEpisode>> GetEpisodes(string seriesId, CancellationToken cancellationToken)
        {
            var tvdbEpisodes = new List<VirtualEpisode>();
            var page = new TvdbEpisodes();
            var i = 0;
            do
            {
                i++;
                var url = string.Format(Constants.Tvdb.SerieEpisodesUrl, seriesId, i);
                page = await GetEpisodePage(url, cancellationToken);
                tvdbEpisodes.AddRange(page.Data
                    .Where(x => x.AiredSeason != 0 && !string.IsNullOrWhiteSpace(x.FirstAired) && DateTime.Now.Date >= Convert.ToDateTime(x.FirstAired)).Select(EpisodeHelper.ConvertToEpisode));
            } while (page.Links.Next != i && page.Links.Next != null);

            return tvdbEpisodes;
        }

        private async Task<TvdbEpisodes> GetEpisodePage(string url, CancellationToken cancellationToken)
        {
            Log.Information($"{Constants.LogPrefix.TheTVDBCLient}\tCall to THETVDB: {Constants.Tvdb.BaseUrl}{url}");
            var headers = new HttpHeaders {{"Authorization", $"Bearer {JWtoken.Token}"}};
            var httpRequest = new HttpRequest
            {
                CancellationToken = cancellationToken,
                Method = "GET",
                Url = $"{Constants.Tvdb.BaseUrl}{url}",
                RequestHeaders = headers
            };

            using (var stream = await _httpClient.SendAsync(httpRequest))
            {
                return _jsonSerializer.DeserializeFromStream<TvdbEpisodes>(stream);
            }
        }

        public async Task<IEnumerable<string>> GetShowsToUpdate(IEnumerable<string> showIds, DateTime lastUpdateTime, CancellationToken cancellationToken)
        {
            Log.Information($"{Constants.LogPrefix.TheTVDBCLient}\tCalling TheTVDB to udpated shows");
            try
            {
                var updateList = new List<string>();
                for (var i = lastUpdateTime; i < DateTime.Now; i = i.AddDays(7))
                {
                    var offset = new DateTimeOffset(i);
                    var epochTimeFrom = offset.ToUnixTimeSeconds();
                    var epochTimeTo = offset.AddDays(7).ToUnixTimeSeconds();

                    var url = string.Format(Constants.Tvdb.UpdatesUrl, epochTimeFrom, epochTimeTo);
                    var httpRequest = new HttpRequest
                    {
                        CancellationToken = cancellationToken,
                        Method = "GET",
                        Url = $"{Constants.Tvdb.BaseUrl}{url}"
                    };

                    Log.Information($"{Constants.LogPrefix.TheTVDBCLient}\tCall to THETVDB: {Constants.Tvdb.BaseUrl}{url}");
                    using (var stream = await _httpClient.SendAsync(httpRequest))
                    {
                        var list = _jsonSerializer.DeserializeFromStream<Updates>(stream);
                        var neededList = list.Data.Where(x => showIds.Any(y => y == x.Id.ToString())).Select(x => x.Id.ToString()).ToList();
                        updateList.AddRange(neededList);
                    }
                }

                return updateList;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{Constants.LogPrefix.TheTVDBCLient}Could not receive show list from TVDB");
                return new List<string>();
            }
        }
    }
}
