﻿using LiteDB;

namespace EmbyStat.Common.Models.Entities.Helpers
{
    public class VideoStream
    {
        [BsonId]
        public string Id { get; set; }
        public string AspectRatio { get; set; }
        public float? AverageFrameRate { get; set; }
        public long? BitRate { get; set; }
        public int? Channels { get; set; }
        public int? Height { get; set; }
        public string Language { get; set; }
        public int? Width { get; set; }
        public string VideoId { get; set; }
    }
}
