﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EmbyStat.Clients.Tvdb.Converter;
using EmbyStat.Clients.Tvdb.Models;
using EmbyStat.Common;
using EmbyStat.Common.Exceptions;
using EmbyStat.Common.Models.Show;
using EmbyStat.Common.Net;
using MediaBrowser.Model.Net;
using NLog;
using RestSharp;

namespace EmbyStat.Clients.Tvdb
{
    public class TvdbClient : ITvdbClient
    {
        private TvdbToken _jwToken;
        private readonly Logger _logger;
        private readonly IRestClient _restClient;

        public TvdbClient(IRestClient client)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _restClient = client.UseSerializer(new JsonNetSerializer());
            _restClient.BaseUrl = new Uri(Constants.Tvdb.BaseUrl);
            _jwToken = new TvdbToken();
        }

        public async Task Login(string apiKey)
        {
            _logger.Info($"{Constants.LogPrefix.TheTVDBCLient}\tLogging in on theTVDB API with key: {apiKey}");

            try
            {
                var request = new RestRequest(Constants.Tvdb.LoginUrl, Method.POST);
                request.AddJsonBody(new { apikey = apiKey });

                var result = await _restClient.ExecuteTaskAsync<TvdbToken>(request);
                _jwToken = result.Data;
            }
            catch (HttpException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new BusinessException("TVDB_LOGIN_FAILED", 500, e);
                }

                throw;
            }
        }

        public async Task<IEnumerable<VirtualEpisode>> GetEpisodes(string seriesId)
        {
            var tvdbEpisodes = new List<VirtualEpisode>();
            TvdbEpisodes page;
            var i = 0;
            do
            {
                i++;
                var url = string.Format(Constants.Tvdb.SerieEpisodesUrl, seriesId, i);
                page = await GetEpisodePage(url);
                page.Data
                    .ForEach(x =>
                    {
                        if (!DateTime.TryParse(x.FirstAired, out _))
                        {
                            x.FirstAired = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
                        }
                    });
                tvdbEpisodes.AddRange(page.Data
                    .Where(x => x.AiredSeason != 0 && !string.IsNullOrWhiteSpace(x.FirstAired) && DateTime.Now.Date >= Convert.ToDateTime(x.FirstAired, CultureInfo.InvariantCulture))
                    .Select(x => x.ConvertToVirtualEpisode()));
            } while (page.Links.Next != i && page.Links.Next != null);

            return tvdbEpisodes;
        }

        private async Task<TvdbEpisodes> GetEpisodePage(string url)
        {
            _logger.Info($"{Constants.LogPrefix.TheTVDBCLient}\tCall to THETVDB: {Constants.Tvdb.BaseUrl}{url}");

            var request = new RestRequest(url, Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {_jwToken.Token}");

            var result = await _restClient.ExecuteTaskAsync<TvdbEpisodes>(request);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpException("404 Not Found");
            }

            return result.Data;
        }

        public async Task<List<int>> GetShowsToUpdate(DateTime? lastUpdateTime, CancellationToken cancellationToken)
        {
            if (!lastUpdateTime.HasValue)
            {
                return new List<int>();
            }

            _logger.Info($"{Constants.LogPrefix.TheTVDBCLient}\tCalling TheTVDB for updated shows");
            try
            {
                var updateList = new List<int>();
                for (var i = lastUpdateTime.Value; i < DateTime.Now; i = i.AddDays(7))
                {
                    var offset = new DateTimeOffset(i);
                    var epochTimeFrom = offset.ToUnixTimeSeconds();
                    var epochTimeTo = offset.AddDays(7).ToUnixTimeSeconds();

                    var url = string.Format(Constants.Tvdb.UpdatesUrl, epochTimeFrom, epochTimeTo);

                    var request = new RestRequest(url, Method.GET);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Authorization", $"Bearer {_jwToken.Token}");

                    var result = await _restClient.ExecuteTaskAsync<Updates>(request, cancellationToken);
                    updateList.AddRange(result.Data.Data.Select(x => x.Id));

                    _logger.Info($"{Constants.LogPrefix.TheTVDBCLient}\tCall to THETVDB: {Constants.Tvdb.BaseUrl}{url}");
                }

                return updateList;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{Constants.LogPrefix.TheTVDBCLient} Could not receive show list from TVDB");
                return new List<int>();
            }
        }
    }
}
