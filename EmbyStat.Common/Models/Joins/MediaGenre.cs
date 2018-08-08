﻿using System;
using System.ComponentModel.DataAnnotations;
using EmbyStat.Common.Models.Helpers;

namespace EmbyStat.Common.Models.Joins
{
    public class MediaGenre
    {
        [Key]
        public Guid Id { get; set; }
        public Guid MediaId { get; set; }
        public Media Media { get; set; }
        public Guid GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
