﻿using System;
using System.Collections.Generic;
using System.Text;
using EmbyStat.Services.Models.Graph;

namespace EmbyStat.Services.Models.Movie
{
    public class MovieGraphs
    {
        public List<Graph<SimpleGraphValue>> BarGraphs { get; set; }
        public List<Graph<SimpleGraphValue>> PieGraphs { get; set; }
    }
}
