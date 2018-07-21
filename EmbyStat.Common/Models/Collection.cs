﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EmbyStat.Common.Models
{
    public class Collection
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string PrimaryImage { get; set; }
        public CollectionType Type { get; set; }
    }

    public enum CollectionType
    {
        Other = 0,
        Movies = 1,
        TvShow = 2,
        Music = 3,
        MusicVideos = 4,
        Trailers = 5,
        HomeVideos = 6,
        BoxSets = 7,
        Books = 8,
        Photos = 9,
        Games = 10,
        LiveTv = 11,
        Playlists = 12,
        Folders = 13
    }
}
