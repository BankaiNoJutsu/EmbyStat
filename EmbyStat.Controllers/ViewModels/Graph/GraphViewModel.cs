﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EmbyStat.Controllers.ViewModels.Graph
{
    public class GraphViewModel<T> where T : class
    {
        public string Title { get; set; }
        public List<T> Data { get; set; }
    }
}
