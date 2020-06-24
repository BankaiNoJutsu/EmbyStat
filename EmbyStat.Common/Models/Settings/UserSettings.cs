﻿using System;
using System.Collections.Generic;
using EmbyStat.Common.Enums;
using Newtonsoft.Json;

namespace EmbyStat.Common.Models.Settings
{
    public class UserSettings
    {
        public string AppName { get; set; }
        public Guid? Id { get; set; }
        public long Version { get; set; }
        public bool WizardFinished { get; set; }
        public string Username { get; set; }
        public string Language { get; set; }
        public bool ToShortMovieEnabled { get; set; }
        public int ToShortMovie { get; set; }
        public int KeepLogsCount { get; set; }
        public List<string> MovieLibraries { get; set; }
        public List<string> ShowLibraries { get; set; }
        public bool AutoUpdate { get; set; }
        public UpdateTrain UpdateTrain { get; set; }
        public bool UpdateInProgress { get; set; }
        public MediaServerSettings MediaServer { get; set; }
        public TvdbSettings Tvdb { get; set; }
        public bool EnableRollbarLogging { get; set; }
    }

    public class MediaServerSettings
    {
        public string ServerName { get; set; }
        public string ApiKey { get; set; }
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string AuthorizationScheme { get; set; }
        public ConnectionProtocol ServerProtocol { get; set; }
        public ServerType ServerType { get; set; }
        public string UserId { get; set; }
        public string BaseUrl { get; set; }
        public string ServerId { get; set; }

        [JsonIgnore]
        public string FullMediaServerAddress
        {
            get
            {
                var protocol = ServerProtocol == ConnectionProtocol.Https ? "https" : "http";
                return $"{protocol}://{ServerAddress}:{ServerPort}{BaseUrl}";
            }
        }

        [JsonIgnore]
        public string FullSocketAddress
        {
            get
            {
                var protocol = ServerProtocol == ConnectionProtocol.Https ? "wss" : "ws";
                return $"{protocol}://{ServerAddress}:{ServerPort}{BaseUrl}";
            }
        }
    }

    public class TvdbSettings
    {
        public DateTime? LastUpdate { get; set; }
        public string ApiKey { get; set; }
    }
}
