﻿using System.Collections.Generic;
using EmbyStat.Common.Models.Entities;

namespace EmbyStat.Clients.Tvdb.Models
{
    public class UpdateShowModel
    {
        public string ShowId { get; set; }
        public IEnumerable<Episode> Episodes { get; set; }
    }
}
