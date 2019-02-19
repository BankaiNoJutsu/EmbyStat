﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmbyStat.Common.Enums;
using EmbyStat.Common.Models.Entities.Events;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Services.Interfaces;
using MediaBrowser.Model.Extensions;

namespace EmbyStat.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionService(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public List<string> GetMediaIdsForUser(string id, PlayType type)
        {
            return _sessionRepository.GetMediaIdsForUser(id, type);
        }

        public List<Play> GetLastWatchedMediaForUser(string id, int count)
        {
            var plays = _sessionRepository.GetPlaysForUser(id);
            var cleanedPlays = CleanPlayList(plays);

            return cleanedPlays
                .Select(x => x.PlayStates.First())
                .OrderByDescending(x => x.TimeLogged)
                .Take(count)
                .Select(x => x.Play)
                .ToList();
        }

        /// <summary>
        /// When 2 states in the same play are logged more then 1 hour time in between we calculate this as a new play entry.
        /// If not, we will presume that the previous state is connected with the current one.
        /// </summary>
        /// <param name="plays">List of Play that needs to be cleaned</param>
        /// <returns>Clean Play list</returns>
        private IEnumerable<Play> CleanPlayList(IEnumerable<Play> plays)
        {
            foreach (var play in plays)
            {
                PlayState prevState = null;
                foreach (var state in play.PlayStates)
                {
                    if (prevState == null || state.TimeLogged > prevState.TimeLogged.AddHours(1))
                    {
                        yield return play;
                    }

                    prevState = state;
                }
            }
        }
    }
}
