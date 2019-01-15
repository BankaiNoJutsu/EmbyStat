﻿using System;

namespace EmbyStat.Services.Models.Movie
{
    public class MovieDuplicate
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string Reason { get; set; }
        public MovieDuplicateItem ItemOne { get; set; }
        public MovieDuplicateItem ItemTwo { get; set; }
    }

    public class MovieDuplicateItem
    {
        public DateTime? DateCreated { get; set; }
        public Guid Id { get; set; }
        public string Quality { get; set; }
    }
}
