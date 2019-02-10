﻿using System;
using System.Collections.Generic;

namespace EmbyStat.Controllers.ViewModels.Configuration
{
    public class FullSettingsViewModel
    {
        public string AppName { get; set; }
        public Guid? Id { get; set; }
        public bool WizardFinished { get; set; }
        public string Username { get; set; }
        public string Language { get; set; }
        public int ToShortMovie { get; set; }
        public int KeepLogsCount { get; set; }
        public List<int> MovieCollectionTypes { get; set; }
        public List<int> ShowCollectionTypes { get; set; }
        public bool AutoUpdate { get; set; }
        public int UpdateTrain { get; set; }
        public bool UpdateInProgress { get; set; }
        public EmbySettingsViewModel Emby { get; set; }
        public TvdbSettingsViewModel Tvdb { get; set; }
        public string Version { get; set; }

        public class EmbySettingsViewModel
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string ServerName { get; set; }
            public string AccessToken { get; set; }
            public string ServerAddress { get; set; }
            public int ServerPort { get; set; }
            public string AuthorizationScheme { get; set; }
            public int ServerProtocol { get; set; }
        }

        public class TvdbSettingsViewModel
        {
            public DateTime? LastUpdate { get; set; }
            public string ApiKey { get; set; }
        }
    }
}
