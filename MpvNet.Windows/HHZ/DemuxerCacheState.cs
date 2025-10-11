using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HHZPlayer.Windows
{
    public class DemuxerCacheState
    {
        [JsonPropertyName("cache-end")]
        public double CacheEnd { get; set; }

        [JsonPropertyName("reader-pts")]
        public double ReaderPts { get; set; }

        [JsonPropertyName("cache-duration")]
        public double CacheDuration { get; set; }

        [JsonPropertyName("eof")]
        public bool Eof { get; set; }

        [JsonPropertyName("underrun")]
        public bool Underrun { get; set; }

        [JsonPropertyName("idle")]
        public bool Idle { get; set; }

        [JsonPropertyName("total-bytes")]
        public long TotalBytes { get; set; }

        [JsonPropertyName("fw-bytes")]
        public long ForwardBytes { get; set; }

        [JsonPropertyName("debug-low-level-seeks")]
        public int DebugLowLevelSeeks { get; set; }

        [JsonPropertyName("debug-byte-level-seeks")]
        public int DebugByteLevelSeeks { get; set; }

        [JsonPropertyName("debug-ts-last")]
        public double DebugTsLast { get; set; }

        [JsonPropertyName("ts-per-stream")]
        public List<StreamCache> TsPerStream { get; set; }

        [JsonPropertyName("bof-cached")]
        public bool BofCached { get; set; }

        [JsonPropertyName("eof-cached")]
        public bool EofCached { get; set; }

        [JsonPropertyName("seekable-ranges")]
        public List<object> SeekableRanges { get; set; } // 如果以后有结构，可以再定义
    }

    public class StreamCache
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("cache-duration")]
        public double CacheDuration { get; set; }

        [JsonPropertyName("reader-pts")]
        public double ReaderPts { get; set; }

        [JsonPropertyName("cache-end")]
        public double CacheEnd { get; set; }
    }

    public static class DemuxerCacheParser
    {
        public static DemuxerCacheState Parse(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<DemuxerCacheState>(json, options);
        }
    }
}
