﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EmbyStat.Controllers.ViewModels.Stat
{
    public class ShowPosterViewModel
    {
        public string MediaId { get; set; }
        public string Name { get; set; }
        public string CommunityRating { get; set; }
        public string OfficialRating { get; set; }
        public string Title { get; set; }
        public string Tag { get; set; }
        public int Year { get; set; }
    }
}
