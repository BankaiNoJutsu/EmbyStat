﻿using System.Collections.Generic;

namespace EmbyStat.Common.Models.Helpers
{
    public class Video : Extra
    {
        public string Container { get; set; }
        public bool? HasSubtitles { get; set; }
        public bool? IdHD { get; set; }
        public string MediaType { get; set; }
        public ICollection<MediaSource> MediaSources { get; set; }
        public ICollection<VideoStream> VideoStreams { get; set; }
        public ICollection<AudioStream> AudioStreams { get; set; }
        public ICollection<SubtitleStream> SubtitleStreams { get; set; }

        public Video()
        {
            MediaSources = new List<MediaSource>();
            VideoStreams = new List<VideoStream>();
            AudioStreams = new List<AudioStream>();
            SubtitleStreams = new List<SubtitleStream>();
        }
    }
}
