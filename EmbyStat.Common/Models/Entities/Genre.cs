﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EmbyStat.Common.Models.Entities.Joins;
namespace EmbyStat.Common.Models.Entities
{
    public class Genre
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<MediaGenre> MediaGenres { get; set; }
    }
}
